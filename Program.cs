var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// TEMPORAL: Ver qué connection string está usando
var connString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine("=================================================");
Console.WriteLine($"ENVIRONMENT: {builder.Environment.EnvironmentName}");
Console.WriteLine($"CONNECTION STRING: {connString}");
Console.WriteLine("=================================================");

builder.Services.AddDbContext<AplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// ... resto del código