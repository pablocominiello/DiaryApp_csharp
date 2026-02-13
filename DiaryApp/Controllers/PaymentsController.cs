using DiaryApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DiaryApp.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly AplicationDbContext _db;

        public PaymentsController(AplicationDbContext db)
        {
            _db = db;
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

            List<Models.Payment> paymentList = paymentsQuery
                .OrderByDescending(p => p.Fecha)
                .ToList();

            return View(paymentList);
        }

        // GET: Payments/Create
        public ActionResult Create(int? personId)
        {
            ViewBag.Peoples = new SelectList(_db.Peoples, "Id", "Nombre");
            
            // Si se recibe un personId, crear un modelo con ese valor preseleccionado
            if (personId.HasValue && personId.Value > 0)
            {
                var payment = new Models.Payment
                {
                    PeoplesId = personId.Value
                };
                return View(payment);
            }
            
            return View();
        }

        // POST: Payments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DiaryApp.Models.Payment obj, IFormFile? comprobanteFile)
        {
            // Server-side validation
            if (obj != null && obj.PeoplesId == 0)
            {
                ModelState.AddModelError("PeoplesId", "Debe seleccionar una persona");
            }

            if (ModelState.IsValid)
            {
                // Procesar el comprobante si fue subido
                if (comprobanteFile != null && comprobanteFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "comprobantes");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + comprobanteFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await comprobanteFile.CopyToAsync(fileStream);
                    }

                    obj.ComprobanteUrl = "/images/comprobantes/" + uniqueFileName;
                }

                _db.Payments.Add(obj);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Peoples = new SelectList(_db.Peoples, "Id", "Nombre", obj.PeoplesId);
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

            Models.Payment payment = _db.Payments.Find(id);

            if (payment == null)
            {
                return NotFound();
            }

            ViewBag.Peoples = new SelectList(_db.Peoples, "Id", "Nombre", payment.PeoplesId);
            return View(payment);
        }

        // POST: Payments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DiaryApp.Models.Payment obj, IFormFile? comprobanteFile)
        {
            if (obj != null && obj.PeoplesId == 0)
            {
                ModelState.AddModelError("PeoplesId", "Debe seleccionar una persona");
            }

            if (ModelState.IsValid)
            {
                if (comprobanteFile != null && comprobanteFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(obj.ComprobanteUrl))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", obj.ComprobanteUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "comprobantes");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + comprobanteFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await comprobanteFile.CopyToAsync(fileStream);
                    }

                    obj.ComprobanteUrl = "/images/comprobantes/" + uniqueFileName;
                }

                _db.Payments.Update(obj);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Peoples = new SelectList(_db.Peoples, "Id", "Nombre", obj.PeoplesId);
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

            Models.Payment payment = _db.Payments.Include(p => p.Person).FirstOrDefault(p => p.Id == id);

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
            Models.Payment payment = _db.Payments.Find(id);

            if (payment == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(payment.ComprobanteUrl))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", payment.ComprobanteUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _db.Payments.Remove(payment);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}