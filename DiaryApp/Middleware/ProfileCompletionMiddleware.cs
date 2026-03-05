using DiaryApp.Core.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DiaryApp.Middleware
{
    public class ProfileCompletionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ProfileCompletionMiddleware> _logger;

        public ProfileCompletionMiddleware(RequestDelegate next, ILogger<ProfileCompletionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            // Solo verificar si el usuario está autenticado
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

                // ✅ Rutas que NO requieren perfil completo
                var excludedPaths = new[]
                {
                    "/account/logout",
                    "/account/confirmemail",
                    "/persons/completeprofile",
                    "/persons/edit", // ✅ Agregar Edit
                    "/api/",
                    "/lib/",
                    "/css/",
                    "/js/",
                    "/images/"
                };

                if (!excludedPaths.Any(excluded => path.StartsWith(excluded)))
                {
                    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var user = await userManager.FindByIdAsync(userId);
                        
                        // Verificar si el email está confirmado
                        if (user != null && !await userManager.IsEmailConfirmedAsync(user))
                        {
                            // Permitir acceso hasta que confirme el email
                            await _next(context);
                            return;
                        }

                        // Verificar si tiene perfil completo
                        var hasPerson = await dbContext.Peoples.AnyAsync(p => p.UserId == userId);

                        if (!hasPerson)
                        {
                            _logger.LogInformation("Usuario {UserId} no tiene perfil, redirigiendo a completar perfil", userId);
                            context.Response.Redirect("/Persons/CompleteProfile");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }

    // ✅ Extension method para registrar el middleware
    public static class ProfileCompletionMiddlewareExtensions
    {
        public static IApplicationBuilder UseProfileCompletion(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ProfileCompletionMiddleware>();
        }
    }
}