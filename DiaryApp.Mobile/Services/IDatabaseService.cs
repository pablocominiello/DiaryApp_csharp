using DiaryApp.Mobile.Models;

namespace DiaryApp.Mobile.Services;

public interface IDatabaseService
{
    Task InitializeDatabaseAsync();
    
    // DiaryEntries
    Task<List<DiaryEntry>> GetDiaryEntriesAsync();
    Task<DiaryEntry?> GetDiaryEntryAsync(int id);
    Task<int> SaveDiaryEntryAsync(DiaryEntry entry);
    Task<int> DeleteDiaryEntryAsync(DiaryEntry entry);
    
    // Persons
    Task<List<Person>> GetPersonsAsync(string? searchText = null);
    Task<Person?> GetPersonAsync(int id);
    Task<int> SavePersonAsync(Person person);
    Task<int> DeletePersonAsync(Person person);
    
    // Payments
    Task<List<Payment>> GetPaymentsAsync(int? personId = null);
    Task<Payment?> GetPaymentAsync(int id);
    Task<int> SavePaymentAsync(Payment payment);
    Task<int> DeletePaymentAsync(Payment payment);
}