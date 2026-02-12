using DiaryApp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        public PersonsController(AplicationDbContext db)
        {
            _db = db;
        }

        // GET: PersonsController1 
        public ActionResult Index()
        {
            List<Models.Person> objDiaryEntryList = _db.Peoples.ToList(); 

            return View(objDiaryEntryList);
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
                // Procesar la imagen si fue subida
                if (imagenFile != null && imagenFile.Length > 0)
                {
                    // Crear carpeta wwwroot/images/persons si no existe
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "persons");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generar nombre único para la imagen
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + imagenFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Guardar la imagen
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagenFile.CopyToAsync(fileStream);
                    }

                    // Guardar la ruta relativa en la base de datos
                    obj.ImagenUrl = "/images/persons/" + uniqueFileName;
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
}
