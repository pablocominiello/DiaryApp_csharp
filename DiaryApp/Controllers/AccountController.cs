using DiaryApp.Core.Interfaces;
using DiaryApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DiaryApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailSender _emailSender;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<AccountController> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuario creado exitosamente: {Email}", model.Email);

                // ✅ NO crear Person aquí, esperar a que complete el perfil después

                // Generar token de confirmación de email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new { userId = user.Id, token = token },
                    protocol: Request.Scheme);

                // ✅ Enviar email de confirmación
                try
                {
                    await _emailSender.SendEmailAsync(
                        model.Email,
                        "Confirma tu correo electrónico - Círculo 9 de Julio",
                        $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <style>
                                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                                .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
                                .content {{ padding: 20px; background-color: #f9f9f9; }}
                                .button {{ display: inline-block; padding: 12px 24px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                                .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>
                                    <h2>Bienvenido a DiaryApp</h2>
                                    <p>Círculo 9 de Julio</p>
                                </div>
                                <div class='content'>
                                    <h3>¡Hola!</h3>
                                    <p>Gracias por registrarte. Para activar tu cuenta, por favor confirma tu correo electrónico haciendo clic en el botón de abajo:</p>
                                    <p style='text-align: center;'>
                                        <a href='{callbackUrl}' class='button'>Confirmar mi correo electrónico</a>
                                    </p>
                                    <p style='font-size: 12px; color: #666;'>Si el botón no funciona, copia y pega este enlace en tu navegador:</p>
                                    <p style='word-break: break-all; font-size: 11px; color: #999;'>{callbackUrl}</p>
                                    <hr>
                                    <p><strong>⚠️ Importante:</strong> Si no has creado esta cuenta, puedes ignorar este mensaje.</p>
                                </div>
                                <div class='footer'>
                                    <p>&copy; 2025 Círculo 9 de Julio - DiaryApp</p>
                                </div>
                            </div>
                        </body>
                        </html>
                        ");

                    _logger.LogInformation("Email de confirmación enviado a {Email}", model.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar email de confirmación a {Email}", model.Email);
                    // Continuar de todos modos, el usuario puede solicitar reenvío
                }

                return RedirectToAction("RegisterConfirmation", new { email = model.Email });
            }

            // Agregar errores al ModelState
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: /Account/RegisterConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterConfirmation(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Register");
            }

            ViewBag.Email = email;
            return View();
        }

        // GET: /Account/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                ViewBag.StatusMessage = "El enlace de confirmación no es válido.";
                ViewBag.IsSuccess = false;
                return View();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.StatusMessage = $"No se pudo encontrar el usuario.";
                ViewBag.IsSuccess = false;
                return View();
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Email confirmado para usuario {Email}", user.Email);
                ViewBag.StatusMessage = "¡Gracias por confirmar tu email! Ahora puedes iniciar sesión.";
                ViewBag.IsSuccess = true;
            }
            else
            {
                _logger.LogWarning("Error al confirmar email para usuario {UserId}: {Errors}", 
                    userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                ViewBag.StatusMessage = "Error al confirmar tu email. El enlace puede haber expirado.";
                ViewBag.IsSuccess = false;
            }

            return View();
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // ✅ Verificar si el usuario existe y si confirmó su email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, 
                    "⚠️ Debes confirmar tu email antes de poder iniciar sesión. Revisa tu bandeja de entrada.");
                _logger.LogWarning("Intento de login con email no confirmado: {Email}", model.Email);
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuario {Email} inició sesión exitosamente", model.Email);
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Cuenta bloqueada: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, 
                    "Tu cuenta ha sido bloqueada por múltiples intentos fallidos. Intenta de nuevo en 15 minutos.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos");
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuario cerró sesión");
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Método auxiliar para redireccionar
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Persons");
        }
    }
}