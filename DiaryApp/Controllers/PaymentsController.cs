using DiaryApp.Core.Data;
using DiaryApp.Core.Models;
using DiaryApp.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DiaryApp.Controllers
{
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
        public ActionResult Index(int? personId)
        {
            var paymentsQuery = _db.Payments
                .Include(p => p.Person)
                .AsQueryable();

            // Filtrar por persona si se proporciona el parámetro
            if (personId.HasValue && personId.Value > 0)
            {
                paymentsQuery = paymentsQuery.Where(p => p.PeoplesId == personId.Value);
                
                // Obtener el nombre y la imagen de la persona para mostrarlos
                var person = _db.Peoples.Find(personId.Value);
                if (person != null)
                {
                    ViewBag.PersonName = person.Nombre;
                    ViewBag.PersonImageUrl = person.ImagenUrl;
                    ViewBag.PersonId = personId.Value;
                }
            }

            List<Payment> paymentList = paymentsQuery
                .OrderByDescending(p => p.Fecha)
                .ToList();

            return View(paymentList);
        }

        // GET: Payments/Create
        public ActionResult Create(int? personId)
        {
            ViewBag.Peoples = new SelectList(_db.Peoples, "Id", "Nombre");
            
            // Crear un modelo con valores por defecto
            var payment = new Payment
            {
                Ano = 2026,
                Fecha = DateTime.Now
            };
            
            // Si se recibe un personId, preseleccionar la persona
            if (personId.HasValue && personId.Value > 0)
            {
                payment.PeoplesId = personId.Value;
            }
            
            return View(payment);
        }

        // POST: Payments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Payment obj, IFormFile? comprobanteFile)
        {
            // Server-side validation
            if (obj != null && obj.PeoplesId == 0)
            {
                ModelState.AddModelError("PeoplesId", "Debe seleccionar una persona");
            }

            // Validar que no exista un pago duplicado
            var existingPayment = _db.Payments
                .FirstOrDefault(p => p.PeoplesId == obj.PeoplesId && p.Ano == obj.Ano && p.Mes == obj.Mes);
    
            if (existingPayment != null)
            {
                ModelState.AddModelError("", $"Ya existe un pago registrado para esta persona en {obj.Mes}/{obj.Ano}");
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
                        ViewBag.Peoples = new SelectList(_db.Peoples, "Id", "Nombre", obj.PeoplesId);
                        ViewBag.PersonId = obj.PeoplesId;
                        return View(obj);
                    }
                }

                _db.Payments.Add(obj);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { personId = obj.PeoplesId });
            }

            ViewBag.Peoples = new SelectList(_db.Peoples, "Id", "Nombre", obj.PeoplesId);
            ViewBag.PersonId = obj.PeoplesId;
            return View(obj);
        }

        // GET: Payments/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            Payment payment = _db.Payments.Find(id);

            if (payment == null)
            {
                return NotFound();
            }

            ViewBag.Peoples = new SelectList(_db.Peoples, "Id", "Nombre", payment.PeoplesId);
            ViewBag.PersonId = payment.PeoplesId;
            return View(payment);
        }

        // POST: Payments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Payment obj, IFormFile? comprobanteFile)
        {
            if (obj != null && obj.PeoplesId == 0)
            {
                ModelState.AddModelError("PeoplesId", "Debe seleccionar una persona");
            }

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
                        ViewBag.Peoples = new SelectList(_db.Peoples, "Id", "Nombre", obj.PeoplesId);
                        ViewBag.PersonId = obj.PeoplesId;
                        return View(obj);
                    }
                }

                _db.Payments.Update(obj);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { personId = obj.PeoplesId });
            }

            ViewBag.Peoples = new SelectList(_db.Peoples, "Id", "Nombre", obj.PeoplesId);
            ViewBag.PersonId = obj.PeoplesId;
            return View(obj);
        }

        // GET: Payments/Delete/5
        [HttpGet]
        public ActionResult Delete(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            Payment payment = _db.Payments.Include(p => p.Person).FirstOrDefault(p => p.Id == id);

            if (payment == null)
            {
                return NotFound();
            }
            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Payment payment = _db.Payments.Find(id);

            if (payment == null)
            {
                return NotFound();
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
            return RedirectToAction("Index");
        }
    }
}