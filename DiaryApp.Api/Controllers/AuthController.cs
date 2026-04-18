using DiaryApp.Shared.Models;
using DiaryApp.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DiaryApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("🔐 Attempting login for: {Email}", request.Email);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("⚠️ Invalid model state for login");
                    return BadRequest(ModelState);
                }

                // Buscar usuario por email
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("⚠️ User not found: {Email}", request.Email);
                    return Unauthorized(new { message = "Email o contraseña incorrectos" });
                }

                _logger.LogInformation("✅ User found: {UserId}", user.Id);

                // Verificar que el email esté confirmado
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    _logger.LogWarning("⚠️ Email not confirmed for: {Email}", request.Email);
                    return Unauthorized(new { message = "Debes confirmar tu email antes de iniciar sesión" });
                }

                _logger.LogInformation("✅ Email confirmed");

                // Verificar la contraseña
                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("🔒 Account locked: {Email}", request.Email);
                        return Unauthorized(new { message = "Cuenta bloqueada por múltiples intentos fallidos" });
                    }

                    _logger.LogWarning("❌ Invalid password for: {Email}", request.Email);
                    return Unauthorized(new { message = "Email o contraseña incorrectos" });
                }

                _logger.LogInformation("✅ Password verified");

                // ✅ CORREGIDO: Buscar el Person asociado al usuario
                var person = await _context.Peoples
                    .FirstOrDefaultAsync(p => p.UserId == user.Id);

                int? personId = person?.Id;
                
                if (personId.HasValue)
                {
                    _logger.LogInformation("✅ Person found: Id={PersonId}, Name={Name}", personId, person?.Nombre);
                }
                else
                {
                    _logger.LogWarning("⚠️ No Person record found for UserId: {UserId}", user.Id);
                }

                // Generar JWT token
                var token = await GenerateJwtToken(user);
                
                _logger.LogInformation("✅ Login successful for {Email}", request.Email);

                return Ok(new LoginResponse
                {
                    Token = token,
                    Email = user.Email!,
                    UserId = user.Id,
                    PersonId = personId, // ✅ NUEVO: Incluir PersonId
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Login error for {Email}: {Message}", request.Email, ex.Message);
                _logger.LogError("❌ Stack trace: {StackTrace}", ex.StackTrace);
                return StatusCode(500, new 
                { 
                    message = "Error interno del servidor", 
                    error = ex.Message,
                    stackTrace = ex.StackTrace 
                });
            }
        }

        // POST: api/auth/register (opcional - si quieres permitir registro desde MAUI)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new IdentityUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuario registrado: {Email}", request.Email);
                
                // Nota: El usuario debe confirmar su email desde la web
                return Ok(new { message = "Usuario creado. Revisa tu email para confirmar la cuenta." });
            }

            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        // GET: api/auth/me - Obtener información del usuario actual
        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(new
            {
                userId = user.Id,
                email = user.Email,
                emailConfirmed = user.EmailConfirmed
            });
        }

        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Agregar roles del usuario
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var jwtSecret = _configuration["JwtSettings:Secret"];
            
            // ✅ VALIDACIÓN: Asegurar que el secret existe
            if (string.IsNullOrEmpty(jwtSecret))
            {
                _logger.LogError("❌ JWT Secret not configured!");
                throw new InvalidOperationException("JWT Secret is not configured in app settings");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"] ?? "DiaryApp.Api",
                audience: _configuration["JwtSettings:Audience"] ?? "DiaryApp.Mobile",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // Modelos para Request/Response
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int? PersonId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}