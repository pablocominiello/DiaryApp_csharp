using DiaryApp.Core.Data;
using DiaryApp.Shared.Models;
using DiaryApp.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace DiaryApp.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IBlobStorageService _blobStorageService;

        public PaymentsController(ApplicationDbContext db, IBlobStorageService blobStorageService)
        {
            _db = db;
            _blobStorageService = blobStorageService;
        }

        // GET: Payments
        public async Task<ActionResult> Index()
        {
            // Obtener el ID del usuario logueado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Obtener la persona asociada al usuario logueado
            var currentPerson = await _db.Peoples.FirstOrDefaultAsync(p => p.UserId == userId);
            
            if (currentPerson == null)
            {
                TempData["Error"] = "No se encontró un perfil de persona asociado a su usuario.";
                return RedirectToAction("Index", "Persons");
            }

            // Filtrar solo los pagos de la persona logueada
            var paymentsQuery = _db.Payments
                .Include(p => p.Person)
                .Where(p => p.PeoplesId == currentPerson.Id);

            ViewBag.PersonName = currentPerson.Nombre;
            ViewBag.PersonImageUrl = currentPerson.ImagenUrl;
            ViewBag.PersonId = currentPerson.Id;

            List<Payment> paymentList = await paymentsQuery
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            return View(paymentList);
        }

        // GET: Payments/Create
        public async Task<ActionResult> Create()
        {
            // Obtener el ID del usuario logueado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Obtener la persona asociada al usuario logueado
            var currentPerson = await _db.Peoples.FirstOrDefaultAsync(p => p.UserId == userId);
            
            if (currentPerson == null)
            {
                TempData["Error"] = "No se encontró un perfil de persona asociado a su usuario.";
                return RedirectToAction("Index", "Persons");
            }

            // Solo mostrar la persona logueada en la lista (aunque no se podrá cambiar)
            ViewBag.Peoples = new SelectList(new[] { currentPerson }, "Id", "Nombre", currentPerson.Id);
            
            // Crear un modelo con valores por defecto
            var payment = new Payment
            {
                PeoplesId = currentPerson.Id,
                Ano = DateTime.Now.Year,
                Mes = DateTime.Now.Month, // ✅ Agregar el mes actual
                Fecha = DateTime.Now
            };
            
            return View(payment);
        }

        // POST: Payments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Payment obj, IFormFile? comprobanteFile)
        {
            // Obtener el ID del usuario logueado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Obtener la persona asociada al usuario logueado
            var currentPerson = await _db.Peoples.FirstOrDefaultAsync(p => p.UserId == userId);
            
            if (currentPerson == null)
            {
                TempData["Error"] = "No se encontró un perfil de persona asociado a su usuario.";
                return RedirectToAction("Index", "Persons");
            }

            // Forzar que el pago sea para la persona logueada (seguridad)
            obj.PeoplesId = currentPerson.Id;

            // Validar que no exista un pago duplicado
            var existingPayment = await _db.Payments
                .FirstOrDefaultAsync(p => p.PeoplesId == obj.PeoplesId && p.Ano == obj.Ano && p.Mes == obj.Mes);
    
            if (existingPayment != null)
            {
                ModelState.AddModelError("", $"Ya existe un pago registrado para {obj.Mes}/{obj.Ano}");
            }

            if (ModelState.IsValid)
            {
                // Subir comprobante a Azure Blob Storage
                if (comprobanteFile != null && comprobanteFile.Length > 0)
                {
                    try
                    {
                        using var stream = comprobanteFile.OpenReadStream();
                        obj.ComprobanteUrl = await _blobStorageService.UploadImageAsync(stream, comprobanteFile.FileName, "comprobantes");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Error al subir el comprobante: {ex.Message}");
                        ViewBag.Peoples = new SelectList(new[] { currentPerson }, "Id", "Nombre", currentPerson.Id);
                        return View(obj);
                    }
                }

                _db.Payments.Add(obj);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Pago registrado exitosamente";
                return RedirectToAction("Index");
            }

            ViewBag.Peoples = new SelectList(new[] { currentPerson }, "Id", "Nombre", currentPerson.Id);
            return View(obj);
        }

        // GET: Payments/Edit/5
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            // Obtener el ID del usuario logueado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Obtener la persona asociada al usuario logueado
            var currentPerson = await _db.Peoples.FirstOrDefaultAsync(p => p.UserId == userId);
            
            if (currentPerson == null)
            {
                TempData["Error"] = "No se encontró un perfil de persona asociado a su usuario.";
                return RedirectToAction("Index", "Persons");
            }

            Payment payment = await _db.Payments.FindAsync(id);

            if (payment == null)
            {
                return NotFound();
            }

            // Verificar que el pago pertenezca a la persona logueada
            if (payment.PeoplesId != currentPerson.Id)
            {
                TempData["Error"] = "No tiene permiso para editar este pago.";
                return RedirectToAction("Index");
            }

            ViewBag.Peoples = new SelectList(new[] { currentPerson }, "Id", "Nombre", currentPerson.Id);
            return View(payment);
        }

        // POST: Payments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Payment obj, IFormFile? comprobanteFile)
        {
            // Obtener el ID del usuario logueado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Obtener la persona asociada al usuario logueado
            var currentPerson = await _db.Peoples.FirstOrDefaultAsync(p => p.UserId == userId);
            
            if (currentPerson == null)
            {
                TempData["Error"] = "No se encontró un perfil de persona asociado a su usuario.";
                return RedirectToAction("Index", "Persons");
            }

            // Forzar que el pago sea para la persona logueada (seguridad)
            obj.PeoplesId = currentPerson.Id;

            if (ModelState.IsValid)
            {
                // Subir nuevo comprobante a Azure Blob Storage
                if (comprobanteFile != null && comprobanteFile.Length > 0)
                {
                    try
                    {
                        // Eliminar comprobante anterior de Azure Blob Storage
                        if (!string.IsNullOrEmpty(obj.ComprobanteUrl))
                        {
                            await _blobStorageService.DeleteImageAsync(obj.ComprobanteUrl, "comprobantes");
                        }

                        // Subir nuevo comprobante
                        using var stream = comprobanteFile.OpenReadStream();
                        obj.ComprobanteUrl = await _blobStorageService.UploadImageAsync(stream, comprobanteFile.FileName, "comprobantes");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Error al actualizar el comprobante: {ex.Message}");
                        ViewBag.Peoples = new SelectList(new[] { currentPerson }, "Id", "Nombre", currentPerson.Id);
                        return View(obj);
                    }
                }

                _db.Payments.Update(obj);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Pago actualizado exitosamente";
                return RedirectToAction("Index");
            }

            ViewBag.Peoples = new SelectList(new[] { currentPerson }, "Id", "Nombre", currentPerson.Id);
            return View(obj);
        }

        // GET: Payments/Delete/5
        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            // Obtener el ID del usuario logueado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Obtener la persona asociada al usuario logueado
            var currentPerson = await _db.Peoples.FirstOrDefaultAsync(p => p.UserId == userId);
            
            if (currentPerson == null)
            {
                TempData["Error"] = "No se encontró un perfil de persona asociado a su usuario.";
                return RedirectToAction("Index", "Persons");
            }

            Payment payment = await _db.Payments.Include(p => p.Person).FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            // Verificar que el pago pertenezca a la persona logueada
            if (payment.PeoplesId != currentPerson.Id)
            {
                TempData["Error"] = "No tiene permiso para eliminar este pago.";
                return RedirectToAction("Index");
            }

            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            // Obtener el ID del usuario logueado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Obtener la persona asociada al usuario logueado
            var currentPerson = await _db.Peoples.FirstOrDefaultAsync(p => p.UserId == userId);
            
            if (currentPerson == null)
            {
                TempData["Error"] = "No se encontró un perfil de persona asociado a su usuario.";
                return RedirectToAction("Index", "Persons");
            }

            Payment payment = await _db.Payments.FindAsync(id);

            if (payment == null)
            {
                return NotFound();
            }

            // Verificar que el pago pertenezca a la persona logueada
            if (payment.PeoplesId != currentPerson.Id)
            {
                TempData["Error"] = "No tiene permiso para eliminar este pago.";
                return RedirectToAction("Index");
            }

            // Eliminar comprobante de Azure Blob Storage
            if (!string.IsNullOrEmpty(payment.ComprobanteUrl))
            {
                try
                {
                    await _blobStorageService.DeleteImageAsync(payment.ComprobanteUrl, "comprobantes");
                }
                catch
                {
                    // Continuar con la eliminación aunque falle eliminar el comprobante
                }
            }

            _db.Payments.Remove(payment);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Pago eliminado exitosamente";
            return RedirectToAction("Index");
        }
    }
}