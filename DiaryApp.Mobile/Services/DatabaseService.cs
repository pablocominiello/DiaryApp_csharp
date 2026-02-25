using Microsoft.EntityFrameworkCore;
using DiaryApp.Mobile.Data;
using DiaryApp.Mobile.Models;

namespace DiaryApp.Mobile.Services;

public class DatabaseService : IDatabaseService
{
    private readonly AppDbContext _context;

    public DatabaseService(AppDbContext context)
    {
        _context = context;
    }

    public async Task InitializeDatabaseAsync()
    {
        await _context.Database.MigrateAsync();
    }

    // DiaryEntries
    public async Task<List<DiaryEntry>> GetDiaryEntriesAsync()
    {
        return await _context.DiaryEntries
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
            _context.DiaryEntries.Add(entry);
        else
            _context.DiaryEntries.Update(entry);

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
        var query = _context.Peoples.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(p => p.Nombre.Contains(searchText));
        }

        return await query.OrderBy(p => p.Nombre).ToListAsync();
    }

    public async Task<Person?> GetPersonAsync(int id)
    {
        return await _context.Peoples
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
        _context.Peoples.Remove(person);
        return await _context.SaveChangesAsync();
    }

    // Payments
    public async Task<List<Payment>> GetPaymentsAsync(int? personId = null)
    {
        var query = _context.Payments.Include(p => p.Person).AsQueryable();

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
        if (payment.Id == 0)
            _context.Payments.Add(payment);
        else
            _context.Payments.Update(payment);

        return await _context.SaveChangesAsync();
    }

    public async Task<int> DeletePaymentAsync(Payment payment)
    {
        _context.Payments.Remove(payment);
        return await _context.SaveChangesAsync();
    }
}