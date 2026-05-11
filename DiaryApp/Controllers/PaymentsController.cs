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

        // GET: Payments?personId=5 (opcional para admins)
        public async Task<ActionResult> Index(int? personId)
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

            // ✅ CORREGIDO: Determinar qué persona consultar según si es admin y hay personId
            Person targetPerson;
            
            if (currentPerson.Admin && personId.HasValue && personId.Value != currentPerson.Id)
            {
                // Admin consultando pagos de otra persona
                targetPerson = await _db.Peoples.FirstOrDefaultAsync(p => p.Id == personId.Value);
                
                if (targetPerson == null)
                {
                    TempData["Error"] = "No se encontró la persona especificada.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                // Usuario normal o admin sin personId: ver sus propios pagos
                targetPerson = currentPerson;
            }

            // ✅ Filtrar pagos de la persona objetivo (no siempre currentPerson)
            var paymentsQuery = _db.Payments
                .Include(p => p.Person)
                .Where(p => p.PeoplesId == targetPerson.Id);

            ViewBag.PersonName = targetPerson.Nombre;
            ViewBag.PersonImageUrl = targetPerson.ImagenUrl;
            ViewBag.PersonId = targetPerson.Id;
            ViewBag.IsAdmin = currentPerson.Admin;
            ViewBag.IsViewingOtherPerson = currentPerson.Id != targetPerson.Id;

            List<Payment> paymentList = await paymentsQuery
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            return View(paymentList);
        }

        // GET: Payments/Create?personId=5 (opcional para admins)
        public async Task<ActionResult> Create(int? personId)
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

            // ✅ NUEVO: Determinar para quién crear el pago
            Person targetPerson;
            
            if (currentPerson.Admin && personId.HasValue)
            {
                targetPerson = await _db.Peoples.FirstOrDefaultAsync(p => p.Id == personId.Value);
                
                if (targetPerson == null)
                {
                    TempData["Error"] = "No se encontró la persona especificada.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                targetPerson = currentPerson;
            }

            // Solo mostrar la persona objetivo en la lista
            ViewBag.Peoples = new SelectList(new[] { targetPerson }, "Id", "Nombre", targetPerson.Id);
            
            // ✅ NUEVO: Agregar ViewBag para mostrar si está viendo otra persona
            ViewBag.PersonName = targetPerson.Nombre;
            ViewBag.PersonImageUrl = targetPerson.ImagenUrl;
            ViewBag.PersonId = targetPerson.Id;
            ViewBag.IsViewingOtherPerson = currentPerson.Id != targetPerson.Id;
            
            // Crear un modelo con valores por defecto
            var payment = new Payment
            {
                PeoplesId = targetPerson.Id,
                Ano = DateTime.Now.Year,
                Mes = DateTime.Now.Month,
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

            // ✅ CORREGIDO: Declarar targetPerson una sola vez
            Person? targetPerson = null;

            // ✅ MODIFICADO: Validar permisos para crear pagos
            if (!currentPerson.Admin)
            {
                // Usuario normal: solo puede crear pagos para sí mismo
                obj.PeoplesId = currentPerson.Id;
                targetPerson = currentPerson;
            }
            else
            {
                // Admin: validar que la persona objetivo exista
                targetPerson = await _db.Peoples.FindAsync(obj.PeoplesId);
                if (targetPerson == null)
                {
                    TempData["Error"] = "La persona especificada no existe.";
                    return RedirectToAction("Index");
                }
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
                        ViewBag.Peoples = new SelectList(new[] { targetPerson }, "Id", "Nombre", obj.PeoplesId);
                        ViewBag.PersonName = targetPerson?.Nombre;
                        ViewBag.PersonImageUrl = targetPerson?.ImagenUrl;
                        ViewBag.PersonId = targetPerson?.Id;
                        ViewBag.IsViewingOtherPerson = currentPerson.Id != targetPerson?.Id;
                        return View(obj);
                    }
                }

                _db.Payments.Add(obj);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Pago registrado exitosamente";
                
                // ✅ Redirigir correctamente según contexto
                if (currentPerson.Admin && obj.PeoplesId != currentPerson.Id)
                {
                    return RedirectToAction("Index", new { personId = obj.PeoplesId });
                }
                
                return RedirectToAction("Index");
            }

            // ✅ Si llegamos aquí, targetPerson ya está asignado
            ViewBag.Peoples = new SelectList(new[] { targetPerson }, "Id", "Nombre", obj.PeoplesId);
            ViewBag.PersonName = targetPerson?.Nombre;
            ViewBag.PersonImageUrl = targetPerson?.ImagenUrl;
            ViewBag.PersonId = targetPerson?.Id;
            ViewBag.IsViewingOtherPerson = currentPerson.Id != targetPerson?.Id;
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

            // ✅ MODIFICADO: Admins pueden editar pagos de cualquiera
            if (!currentPerson.Admin && payment.PeoplesId != currentPerson.Id)
            {
                TempData["Error"] = "No tiene permiso para editar este pago.";
                return RedirectToAction("Index");
            }

            var targetPerson = await _db.Peoples.FindAsync(payment.PeoplesId);
            ViewBag.Peoples = new SelectList(new[] { targetPerson }, "Id", "Nombre", payment.PeoplesId);
            
            // ✅ NUEVO: Agregar ViewBag para mostrar si está viendo otra persona
            ViewBag.PersonName = targetPerson?.Nombre;
            ViewBag.PersonImageUrl = targetPerson?.ImagenUrl;
            ViewBag.PersonId = targetPerson?.Id;
            ViewBag.IsViewingOtherPerson = currentPerson.Id != payment.PeoplesId;
            
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

            // ✅ MODIFICADO: Validar permisos
            if (!currentPerson.Admin)
            {
                // Usuario normal: forzar que el pago sea para sí mismo
                obj.PeoplesId = currentPerson.Id;
            }
            // Admin: puede mantener el PeoplesId original del pago

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
                        var person = await _db.Peoples.FindAsync(obj.PeoplesId);
                        ViewBag.Peoples = new SelectList(new[] { person }, "Id", "Nombre", obj.PeoplesId);
                        ViewBag.PersonName = person?.Nombre;
                        ViewBag.PersonImageUrl = person?.ImagenUrl;
                        ViewBag.PersonId = person?.Id;
                        ViewBag.IsViewingOtherPerson = currentPerson.Id != obj.PeoplesId;
                        return View(obj);
                    }
                }

                _db.Payments.Update(obj);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Pago actualizado exitosamente";
                
                // ✅ Redirigir correctamente según contexto
                if (currentPerson.Admin && obj.PeoplesId != currentPerson.Id)
                {
                    return RedirectToAction("Index", new { personId = obj.PeoplesId });
                }
                
                return RedirectToAction("Index");
            }

            var targetPerson = await _db.Peoples.FindAsync(obj.PeoplesId);
            ViewBag.Peoples = new SelectList(new[] { targetPerson }, "Id", "Nombre", obj.PeoplesId);
            ViewBag.PersonName = targetPerson?.Nombre;
            ViewBag.PersonImageUrl = targetPerson?.ImagenUrl;
            ViewBag.PersonId = targetPerson?.Id;
            ViewBag.IsViewingOtherPerson = currentPerson.Id != obj.PeoplesId;
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

            // ✅ MODIFICADO: Admins pueden eliminar pagos de cualquiera
            if (!currentPerson.Admin && payment.PeoplesId != currentPerson.Id)
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

            // ✅ MODIFICADO: Admins pueden eliminar pagos de cualquiera
            if (!currentPerson.Admin && payment.PeoplesId != currentPerson.Id)
            {
                TempData["Error"] = "No tiene permiso para eliminar este pago.";
                return RedirectToAction("Index");
            }

            var targetPersonId = payment.PeoplesId;

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
            
            // ✅ Redirigir correctamente según contexto
            if (currentPerson.Admin && targetPersonId != currentPerson.Id)
            {
                return RedirectToAction("Index", new { personId = targetPersonId });
            }
            
            return RedirectToAction("Index");
        }
    }
}