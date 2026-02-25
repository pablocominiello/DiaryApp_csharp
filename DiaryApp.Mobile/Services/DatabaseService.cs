using Microsoft.EntityFrameworkCore;
using DiaryApp.Mobile.Data;
using DiaryApp.Mobile.Models;

namespace DiaryApp.Mobile.Services;

public class DatabaseService : IDatabaseService
{
    private readonly AppDbContext _context;
    private bool _isInitialized = false;

    public DatabaseService(AppDbContext context)
    {
        _context = context;
    }

    public async Task InitializeDatabaseAsync()
    {
        if (_isInitialized)
            return;

        try
        {
            // Crear la base de datos si no existe
            await _context.Database.EnsureCreatedAsync();

            // Aplicar migraciones pendientes (si usas migraciones)
            // await _context.Database.MigrateAsync();

            // Seed data inicial
            await SeedDataAsync();

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            // Log del error (considera usar ILogger en producción)
            System.Diagnostics.Debug.WriteLine($"Error inicializando BD: {ex.Message}");
            throw;
        }
    }

    private async Task SeedDataAsync()
    {
        // Seed DiaryEntries
        if (!await _context.DiaryEntries.AnyAsync())
        {
            var entries = new List<DiaryEntry>
            {
                new DiaryEntry
                {
                    Title = "Learning .NET MAUI",
                    Content = "Learning .NET MAUI mobile development with C# 13",
                    DateCreated = DateTime.Now.AddDays(-10)
                },
                new DiaryEntry
                {
                    Title = "SQLite Integration",
                    Content = "Implementing SQLite database for offline storage",
                    DateCreated = DateTime.Now.AddDays(-5)
                },
                new DiaryEntry
                {
                    Title = "Azure Blob Storage",
                    Content = "Integrating Azure Blob Storage for image uploads",
                    DateCreated = DateTime.Now.AddDays(-2)
                }
            };
            await _context.DiaryEntries.AddRangeAsync(entries);
        }

        // Seed Persons
        if (!await _context.Peoples.AnyAsync())
        {
            var persons = new List<Person>
            {
                new Person
                {
                    Nombre = "Pablo Eugenio Cominiello",
                    Content = "Kili",
                    Born = new DateTime(1976, 6, 30)
                }
            };
            await _context.Peoples.AddRangeAsync(persons);
        }

        await _context.SaveChangesAsync();
    }

    // DiaryEntries
    public async Task<List<DiaryEntry>> GetDiaryEntriesAsync()
    {
        return await _context.DiaryEntries
            .AsNoTracking()
            .OrderByDescending(d => d.DateCreated)
            .ToListAsync();
    }

    public async Task<DiaryEntry?> GetDiaryEntryAsync(int id)
    {
        return await _context.DiaryEntries.FindAsync(id);
    }

    public async Task<int> SaveDiaryEntryAsync(DiaryEntry entry)
    {
        if (entry.Id == 0)
        {
            entry.DateCreated = DateTime.Now;
            _context.DiaryEntries.Add(entry);
        }
        else
        {
            _context.DiaryEntries.Update(entry);
        }

        return await _context.SaveChangesAsync();
    }

    public async Task<int> DeleteDiaryEntryAsync(DiaryEntry entry)
    {
        _context.DiaryEntries.Remove(entry);
        return await _context.SaveChangesAsync();
    }

    // Persons
    public async Task<List<Person>> GetPersonsAsync(string? searchText = null)
    {
        var query = _context.Peoples.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(p => p.Nombre.Contains(searchText) || 
                                     p.Content.Contains(searchText));
        }

        return await query.OrderBy(p => p.Nombre).ToListAsync();
    }

    public async Task<Person?> GetPersonAsync(int id)
    {
        return await _context.Peoples
            .AsNoTracking()
            .Include(p => p.Payments)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<int> SavePersonAsync(Person person)
    {
        if (person.Id == 0)
            _context.Peoples.Add(person);
        else
            _context.Peoples.Update(person);

        return await _context.SaveChangesAsync();
    }

    public async Task<int> DeletePersonAsync(Person person)
    {
        // Eliminar pagos asociados (CASCADE)
        _context.Peoples.Remove(person);
        return await _context.SaveChangesAsync();
    }

    // Payments
    public async Task<List<Payment>> GetPaymentsAsync(int? personId = null)
    {
        var query = _context.Payments
            .AsNoTracking()
            .Include(p => p.Person)
            .AsQueryable();

        if (personId.HasValue)
        {
            query = query.Where(p => p.PeoplesId == personId.Value);
        }

        return await query
            .OrderByDescending(p => p.Ano)
            .ThenByDescending(p => p.Mes)
            .ToListAsync();
    }

    public async Task<Payment?> GetPaymentAsync(int id)
    {
        return await _context.Payments
            .Include(p => p.Person)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<int> SavePaymentAsync(Payment payment)
    {
        // Validar duplicados (Año/Mes/Persona)
        var exists = await _context.Payments
            .AnyAsync(p => p.PeoplesId == payment.PeoplesId &&
                          p.Ano == payment.Ano &&
                          p.Mes == payment.Mes &&
                          p.Id != payment.Id);

        if (exists)
            throw new InvalidOperationException("Ya existe un pago para esta persona en este período");

        if (payment.Id == 0)
        {
            payment.Fecha = DateTime.Now;
            _context.Payments.Add(payment);
        }
        else
        {
            _context.Payments.Update(payment);
        }

        return await _context.SaveChangesAsync();
    }

    public async Task<int> DeletePaymentAsync(Payment payment)
    {
        _context.Payments.Remove(payment);
        return await _context.SaveChangesAsync();
    }
}