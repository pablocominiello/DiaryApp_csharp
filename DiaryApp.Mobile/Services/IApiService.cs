using DiaryApp.Shared.Models;

namespace DiaryApp.Mobile.Services;

public interface IApiService
{
    Task<List<Person>> GetPersonsAsync(string? searchText = null);
    Task<Person?> GetPersonAsync(int id);
    Task<Person> CreatePersonAsync(Person person);
    Task UpdatePersonAsync(Person person);
    Task DeletePersonAsync(int id);

    Task<List<DiaryEntry>> GetDiaryEntriesAsync();
    Task<DiaryEntry?> GetDiaryEntryAsync(int id);
    Task<DiaryEntry> CreateDiaryEntryAsync(DiaryEntry entry);
    Task UpdateDiaryEntryAsync(DiaryEntry entry);
    Task DeleteDiaryEntryAsync(int id);

    Task<List<Payment>> GetPaymentsAsync(int? personId = null);
    Task<Payment?> GetPaymentAsync(int id);
    Task<Payment> CreatePaymentAsync(Payment payment);
    Task UpdatePaymentAsync(Payment payment);
    Task DeletePaymentAsync(int id);
}