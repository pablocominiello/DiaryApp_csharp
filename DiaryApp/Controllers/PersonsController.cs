using DiaryApp.Core.Data;
using DiaryApp.Core.Interfaces;
using DiaryApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DiaryApp.Controllers
{
    // Modelo para paginación
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    [Authorize]
    public class PersonsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IBlobStorageService _blobStorageService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<PersonsController> _logger;

        public PersonsController(
            ApplicationDbContext db, 
            IBlobStorageService blobStorageService,
            UserManager<IdentityUser> userManager,
            ILogger<PersonsController> logger)
        {
            _db = db;
            _blobStorageService = blobStorageService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Persons/CompleteProfile - Primera vez que completa el perfil
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CompleteProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var existingPerson = await _db.Peoples.FirstOrDefaultAsync(p => p.UserId == userId);
            if (existingPerson != null)
            {
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var person = new Person
            {
                UserId = userId,
                Born = DateTime.Now.AddYears(-18)
            };

            ViewBag.UserEmail = user.Email;
            ViewBag.IsNewProfile = true;
            return View("CompleteProfile", person);
        }

        // POST: /Persons/CompleteProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> CompleteProfile(Person person, IFormFile? imagenFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (person.UserId != userId)
            {
                _logger.LogWarning("Intento de crear perfil con UserId diferente al usuario logueado");
                return Forbid();
            }

            var existingPerson = await _db.Peoples.FirstOrDefaultAsync(p => p.UserId == userId);
            if (existingPerson != null)
            {
                return RedirectToAction("Index");
            }

            if (person.Nombre.Length < 3)
            {
                ModelState.AddModelError("Nombre", "El nombre debe tener al menos 3 caracteres");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (imagenFile != null && imagenFile.Length > 0)
                    {
                        using var stream = imagenFile.OpenReadStream();
                        person.ImagenUrl = await _blobStorageService.UploadImageAsync(
                            stream, 
                            imagenFile.FileName, 
                            "persons");
                    }

                    _db.Peoples.Add(person);
                    await _db.SaveChangesAsync();
                    
                    _logger.LogInformation("Perfil completado para usuario {UserId}", userId);
                    
                    TempData["SuccessMessage"] = "¡Perfil completado exitosamente!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al completar perfil para usuario {UserId}", userId);
                    ModelState.AddModelError("", "Error al guardar el perfil. Por favor, intenta de nuevo.");
                }
            }

            var user = await _userManager.FindByIdAsync(userId);
            ViewBag.UserEmail = user?.Email;
            ViewBag.IsNewProfile = true;
            return View(person);
        }

        // GET: /Persons/Index - Ver mi perfil
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var person = await _db.Peoples
                .Include(p => p.Payments)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (person == null)
            {
                return RedirectToAction("CompleteProfile");
            }

            return View("Profile", person);
        }

        // GET: /Persons/List - Lista todas las personas con paginación y búsqueda
        [HttpGet]
        public async Task<IActionResult> List(string? searchString, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var peoplesQuery = _db.Peoples.AsQueryable();

            // Filtrar por nombre si se proporciona búsqueda
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                peoplesQuery = peoplesQuery.Where(p => 
                    p.Nombre.Contains(searchString) || 
                    p.Content.Contains(searchString));
            }

            // Obtener el total antes de paginar
            int totalCount = await peoplesQuery.CountAsync();

            // Ordenar y aplicar paginación
            var peoples = await peoplesQuery
                .OrderBy(p => p.Nombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagedResult = new PagedResult<Person>
            {
                Items = peoples,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            ViewBag.SearchString = searchString;
            return View(pagedResult);
        }

        // GET: /Persons/Edit - Editar mi perfil
        [HttpGet]
        public async Task<ActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var person = await _db.Peoples.FirstOrDefaultAsync(p => p.UserId == userId);

            if (person == null)
            {
                return RedirectToAction("CompleteProfile");
            }

            var user = await _userManager.FindByIdAsync(userId);
            ViewBag.UserEmail = user?.Email;
            ViewBag.IsNewProfile = false;
            return View(person);
        }

        // POST: /Persons/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Person person, IFormFile? imagenFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (person.UserId != userId)
            {
                return Forbid();
            }

            if (person.Nombre.Length < 3)
            {
                ModelState.AddModelError("Nombre", "El nombre debe tener al menos 3 caracteres");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (imagenFile != null && imagenFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(person.ImagenUrl))
                        {
                            await _blobStorageService.DeleteImageAsync(person.ImagenUrl, "persons");
                        }

                        using var stream = imagenFile.OpenReadStream();
                        person.ImagenUrl = await _blobStorageService.UploadImageAsync(
                            stream, 
                            imagenFile.FileName, 
                            "persons");
                    }

                    _db.Peoples.Update(person);
                    await _db.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Perfil actualizado exitosamente";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al actualizar perfil para usuario {UserId}", userId);
                    ModelState.AddModelError("", "Error al actualizar el perfil. Por favor, intenta de nuevo.");
                }
            }

            var user = await _userManager.FindByIdAsync(userId);
            ViewBag.UserEmail = user?.Email;
            ViewBag.IsNewProfile = false;
            return View(person);
        }
    }
}