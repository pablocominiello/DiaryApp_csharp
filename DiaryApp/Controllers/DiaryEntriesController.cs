using DiaryApp.Data;
using DiaryApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;

namespace DiaryApp.Controllers
{
    public class DiaryEntriesController : Controller
    {
        private readonly AplicationDbContext _db;

        public DiaryEntriesController(AplicationDbContext db)
        {
            _db = db;
        }

        // GET: DiaryEntriesController1 
        public ActionResult Index()
        {
            List<Models.DiaryEntry> objDiaryEntryList = _db.DiaryEntries.ToList();

            return View(objDiaryEntryList);
        }

        // GET: DiaryEntriesController1/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }
        // GET: DiaryEntriesController1/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        // GET: DiaryEntriesController1/Create
        public ActionResult Create(DiaryApp.Models.DiaryEntry obj)
        {
            // Server-side validation example
            if (obj != null && obj.Title.Length < 3)
            {
                ModelState.AddModelError("Title", "Titulo muy corto");
            }

            if (ModelState.IsValid)
            {
                _db.DiaryEntries.Add(obj); // add new entry to database
                _db.SaveChanges(); // save changes to database
                return RedirectToAction("index");
            }

            return View(obj);
        }


        [HttpGet]
        // GET: DiaryEntriesController1/Edit/5
        public ActionResult Edit(int id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Models.DiaryEntry diaryEntry = _db.DiaryEntries.Find(id);

            if (diaryEntry == null)
            {
                return NotFound();
            }
            return View(diaryEntry);
        }

        // POST: DiaryEntriesController1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DiaryApp.Models.DiaryEntry obj)
        {
            // Server-side validation example
            if (obj != null && obj.Title.Length < 3)
            {
                ModelState.AddModelError("Title", "Titulo muy corto");
            }

            if (ModelState.IsValid)
            {
                _db.DiaryEntries.Update(obj); // Update entry to database
                _db.SaveChanges(); // save changes to database
                return RedirectToAction("index");
            }

            return View(obj);
        }



        // GET: DiaryEntriesController1/Delete/5
        public ActionResult Delete(int id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Models.DiaryEntry diaryEntry = _db.DiaryEntries.Find(id);

            if (diaryEntry == null)
            {
                return NotFound();
            }
            return View(diaryEntry);
        }

        // POST: DiaryEntriesController1/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(DiaryApp.Models.DiaryEntry obj)
        {
            // Server-side validation example
            if (obj != null && obj.Title.Length < 3)
            {
                ModelState.AddModelError("Title", "Titulo muy corto");
            }

            if (ModelState.IsValid)
            {
                _db.DiaryEntries.Remove(obj); // Update entry to database
                _db.SaveChanges(); // save changes to database
                return RedirectToAction("index");
            }

            return View(obj);
        }
    }
    
    public class PersonsController : Controller
    {
        private readonly AplicationDbContext _db;
        private readonly IBlobStorageService _blobStorageService;

        public PersonsController(AplicationDbContext db, IBlobStorageService blobStorageService)
        {
            _db = db;
            _blobStorageService = blobStorageService;
        }

        // GET: PersonsController1 
        public ActionResult Index(string searchString, int? page)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            // Obtener todas las personas
            var peoplesQuery = _db.Peoples.AsQueryable();

            // Aplicar filtro si se proporciona un término de búsqueda
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                peoplesQuery = peoplesQuery.Where(p => p.Nombre.Contains(searchString));
            }

            // Ordenar y paginar
            var objPersonList = peoplesQuery
                .OrderBy(p => p.Nombre)
                .ToPagedList(pageNumber, pageSize);

            // Pasar el término de búsqueda a la vista para mantenerlo en el formulario
            ViewBag.SearchString = searchString;

            return View(objPersonList);
        }

        // GET: PeoplesController1/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }
        // GET: PeoplesController1/Create
        public ActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        // POST: PersonsController/Create
        public async Task<ActionResult> Create(DiaryApp.Models.Person obj, IFormFile? imagenFile)
        {
            // Server-side validation example
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
                        obj.ImagenUrl = await _blobStorageService.UploadImageAsync(imagenFile, "persons");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Error al subir la imagen: {ex.Message}");
                        return View(obj);
                    }
                }

                _db.Peoples.Add(obj); // add new entry to database
                await _db.SaveChangesAsync(); // save changes to database
                return RedirectToAction("index");
            }

            return View(obj);
        }


        [HttpGet]
        // GET: DiaryEntriesController1/Edit/5
        public ActionResult Edit(int id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Models.Person person = _db.Peoples.Find(id);

            if (person == null)
            {
                return NotFound();
            }
            return View(person);
        }

        // POST: DiaryEntriesController1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DiaryApp.Models.Person obj, IFormFile? imagenFile)
        {
            // Server-side validation example
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
                        obj.ImagenUrl = await _blobStorageService.UploadImageAsync(imagenFile, "persons");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Error al actualizar la imagen: {ex.Message}");
                        return View(obj);
                    }
                }

                _db.Peoples.Update(obj); // Update entry to database
                await _db.SaveChangesAsync(); // save changes to database
                return RedirectToAction("index");
            }

            return View(obj);
        }



        // GET: DiaryEntriesController1/Delete/5
        public ActionResult Delete(int id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Models.Person objperson = _db.Peoples.Find(id);

            if (objperson == null)
            {
                return NotFound();
            }
            return View(objperson);
        }

        // POST: DiaryEntriesController1/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(DiaryApp.Models.Person obj)
        {
            // Server-side validation example
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

                _db.Peoples.Remove(obj); // Update entry to database
                await _db.SaveChangesAsync(); // save changes to database
                return RedirectToAction("index");
            }

            return View(obj);
        }
    }
}
