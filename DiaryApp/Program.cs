using DiaryApp.Core.Data;
using DiaryApp.Core.Interfaces;
using DiaryApp.Middleware;
using DiaryApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ✅ NUEVO: Agregar soporte para Razor Pages
builder.Services.AddRazorPages();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Usar ApplicationDbContext de Core
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection")));

// ✅ Agregar Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

// Configurar ASP.NET Core Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // ✅ Configuración de contraseña MÁS SIMPLE
    options.Password.RequireDigit = false;           // ❌ No requiere números
    options.Password.RequireLowercase = false;       // ❌ No requiere minúsculas
    options.Password.RequireUppercase = false;       // ❌ No requiere mayúsculas
    options.Password.RequireNonAlphanumeric = false; // ❌ No requiere caracteres especiales
    options.Password.RequiredLength = 1;             // ✅ Mínimo 1 carácter (sin restricción práctica)
    options.Password.RequiredUniqueChars = 1;        // ✅ Al menos 1 carácter único

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configurar cookies de autenticación
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Registrar servicios
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

var app = builder.Build();

// ✅ AUTO-MIGRACIÓN: Aplicar migraciones automáticamente al iniciar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Aplicando migraciones de base de datos...");
        context.Database.Migrate();
        logger.LogInformation("✅ Migraciones aplicadas exitosamente");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Error al aplicar migraciones de base de datos");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// ✅ ORDEN CORRECTO:
app.UseAuthentication();      // 1️⃣ Primero autenticar
app.UseAuthorization();       // 2️⃣ Luego autorizar
app.UseProfileCompletion();   // 3️⃣ Por último, verificar perfil

app.MapStaticAssets();

// ✅ Health Check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.MapControllers();

// ✅ NUEVO: Mapear Razor Pages
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Persons}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
