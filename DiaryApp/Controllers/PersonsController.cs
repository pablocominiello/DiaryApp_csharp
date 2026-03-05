using DiaryApp.Core.Data;
using DiaryApp.Core.Interfaces;
using DiaryApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagedList.Core;

namespace DiaryApp.Controllers
{
    [Authorize] // ✅ Solo usuarios autenticados pueden acceder
    public class PersonsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IBlobStorageService _blobStorageService;

        public PersonsController(ApplicationDbContext db, IBlobStorageService blobStorageService)
        {
            _db = db;
            _blobStorageService = blobStorageService;
        }

        // GET: PersonsController 
        public ActionResult Index(string searchString, int? page)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var peoplesQuery = _db.Peoples.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                peoplesQuery = peoplesQuery.Where(p => p.Nombre.Contains(searchString));
            }

            peoplesQuery = peoplesQuery.OrderBy(p => p.Nombre);

            var pagedList = new PagedList<Person>(peoplesQuery, pageNumber, pageSize);

            ViewBag.SearchString = searchString;
            return View(pagedList);
        }

        // GET: PersonsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PersonsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PersonsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Person obj, IFormFile? imagenFile)
        {
            if (obj != null && obj.Nombre.Length < 3)
            {
                ModelState.AddModelError("Nombre", "Nombre muy corto");
            }

            if (ModelState.IsValid)
            {
                // Subir imagen a Azure Blob Storage
                if (imagenFile != null && imagenFile.Length > 0)
                {
                    try
                    {
                        using var stream = imagenFile.OpenReadStream();
                        obj.ImagenUrl = await _blobStorageService.UploadImageAsync(stream, imagenFile.FileName, "persons");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Error al subir la imagen: {ex.Message}");
                        return View(obj);
                    }
                }

                _db.Peoples.Add(obj);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(obj);
        }

        // GET: PersonsController/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            Person? person = _db.Peoples.Find(id);

            if (person == null)
            {
                return NotFound();
            }
            return View(person);
        }

        // POST: PersonsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Person obj, IFormFile? imagenFile)
        {
            if (obj != null && obj.Nombre.Length < 3)
            {
                ModelState.AddModelError("Nombre", "Nombre muy corto");
            }

            if (ModelState.IsValid)
            {
                // Procesar la nueva imagen si fue subida
                if (imagenFile != null && imagenFile.Length > 0)
                {
                    try
                    {
                        // Eliminar la imagen anterior de Azure Blob Storage
                        if (!string.IsNullOrEmpty(obj.ImagenUrl))
                        {
                            await _blobStorageService.DeleteImageAsync(obj.ImagenUrl, "persons");
                        }

                        // Subir nueva imagen a Azure Blob Storage
                        using var stream = imagenFile.OpenReadStream();
                        obj.ImagenUrl = await _blobStorageService.UploadImageAsync(stream, imagenFile.FileName, "persons");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Error al actualizar la imagen: {ex.Message}");
                        return View(obj);
                    }
                }

                _db.Peoples.Update(obj);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // ✅ AGREGADO: Retornar la vista si ModelState no es válido
            return View(obj);
        }

        // GET: PersonsController/Delete/5
        public ActionResult Delete(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            Person? objperson = _db.Peoples.Find(id);

            if (objperson == null)
            {
                return NotFound();
            }
            return View(objperson);
        }

        // POST: PersonsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(Person obj)
        {
            if (obj != null && obj.Nombre.Length < 3)
            {
                ModelState.AddModelError("Title", "Titulo muy corto");
            }

            if (ModelState.IsValid)
            {
                // Eliminar imagen de Azure Blob Storage antes de eliminar la persona
                if (!string.IsNullOrEmpty(obj.ImagenUrl))
                {
                    try
                    {
                        await _blobStorageService.DeleteImageAsync(obj.ImagenUrl, "persons");
                    }
                    catch
                    {
                        // Continuar con la eliminación aunque falle eliminar la imagen
                    }
                }

                _db.Peoples.Remove(obj);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(obj);
        }
    }
}