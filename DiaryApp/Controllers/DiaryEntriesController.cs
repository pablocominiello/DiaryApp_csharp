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

            if ( ModelState.IsValid) 
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
}
