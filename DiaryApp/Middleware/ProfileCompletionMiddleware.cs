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

                // ✅ CORREGIDO: Rutas que NO requieren perfil completo
                var excludedPaths = new[]
                {
                    "/account/",                    // ✅ Todas las rutas de Account (Login, Logout, Register, etc.)
                    "/persons/completeprofile",     // ✅ La página de completar perfil
                    "/persons/edit",                // ✅ Editar perfil
                    "/api/",                        // ✅ APIs
                    "/health",                      // ✅ Health checks de Azure
                    "/_framework/",                 // ✅ Framework files
                    "/lib/",                        // ✅ Librerías estáticas
                    "/css/",                        // ✅ CSS
                    "/js/",                         // ✅ JavaScript
                    "/images/",                     // ✅ Imágenes
                    "/favicon.ico"                  // ✅ Favicon
                };

                // ✅ Si está en una ruta excluida, permitir el acceso SIN verificar perfil
                if (excludedPaths.Any(excluded => path.StartsWith(excluded)))
                {
                    await _next(context);
                    return;
                }

                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (!string.IsNullOrEmpty(userId))
                {
                    try
                    {
                        var user = await userManager.FindByIdAsync(userId);
                        
                        // ✅ Si el email no está confirmado, permitir acceso (Identity lo maneja)
                        if (user != null && !await userManager.IsEmailConfirmedAsync(user))
                        {
                            await _next(context);
                            return;
                        }

                        // ✅ Verificar si tiene perfil completo
                        var hasPerson = await dbContext.Peoples.AnyAsync(p => p.UserId == userId);

                        if (!hasPerson)
                        {
                            // ✅ CRÍTICO: Evitar bucle infinito
                            if (!path.Contains("/persons/completeprofile"))
                            {
                                _logger.LogInformation("Usuario {UserId} sin perfil, redirigiendo desde {Path}", userId, path);
                                context.Response.Redirect("/Persons/CompleteProfile");
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error en ProfileCompletionMiddleware para usuario {UserId}", userId);
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