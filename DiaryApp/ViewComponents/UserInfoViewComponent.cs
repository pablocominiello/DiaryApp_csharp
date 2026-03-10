using DiaryApp.Core.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DiaryApp.ViewComponents
{
    public class UserInfoViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public UserInfoViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return View(new UserInfoViewModel { IsAdmin = false });
            }

            var person = await _db.Peoples
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var model = new UserInfoViewModel
            {
                IsAdmin = person?.Admin ?? false,
                UserName = HttpContext.User.Identity?.Name
            };

            return View(model);
        }
    }

    public class UserInfoViewModel
    {
        public bool IsAdmin { get; set; }
        public string? UserName { get; set; }
    }
}