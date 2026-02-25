using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DiaryApp.Shared.Models;

namespace DiaryApp.Mobile.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        // Configure JSON options to handle circular references
        _jsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
    
    // Persons
    public async Task<List<Person>> GetPersonsAsync(string? searchText = null)
    {
        try
        {
            var url = string.IsNullOrWhiteSpace(searchText) 
                ? "persons" 
                : $"persons?searchText={Uri.EscapeDataString(searchText)}";
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            
            // Log para ver qué está devolviendo el API
            System.Diagnostics.Debug.WriteLine($"API Response: {content}");
            
            // Si el contenido está vacío o es null
            if (string.IsNullOrWhiteSpace(content))
            {
                System.Diagnostics.Debug.WriteLine("Empty response from API");
                return new List<Person>();
            }
            
            return JsonSerializer.Deserialize<List<Person>>(content, _jsonOptions) ?? new List<Person>();
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"JSON Error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
            throw new Exception($"Error parsing persons data: {ex.Message}", ex);
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Error: {ex.Message}");
            throw new Exception($"Error fetching persons: {ex.Message}", ex);
        }
    }

    public async Task<Person?> GetPersonAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"persons/{id}");
            if (!response.IsSuccessStatusCode)
                return null;
            
            var content = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"API Response (single person): {content}");
            
            return JsonSerializer.Deserialize<Person>(content, _jsonOptions);
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"JSON Error getting person: {ex.Message}");
            return null;
        }
    }

    public async Task<Person> CreatePersonAsync(Person person)
    {
        var json = JsonSerializer.Serialize(person, _jsonOptions);
        var response = await _httpClient.PostAsync("persons", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Person>(content, _jsonOptions) 
            ?? throw new Exception("Error creating person");
    }

    public async Task UpdatePersonAsync(Person person)
    {
        var json = JsonSerializer.Serialize(person, _jsonOptions);
        var response = await _httpClient.PutAsync($"persons/{person.Id}", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
    }

    public async Task DeletePersonAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"persons/{id}");
        response.EnsureSuccessStatusCode();
    }

    // DiaryEntries
    public async Task<List<DiaryEntry>> GetDiaryEntriesAsync()
    {
        var response = await _httpClient.GetAsync("diaryentries");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<DiaryEntry>>(content, _jsonOptions) ?? new List<DiaryEntry>();
    }

    public async Task<DiaryEntry?> GetDiaryEntryAsync(int id)
    {
        var response = await _httpClient.GetAsync($"diaryentries/{id}");
        if (!response.IsSuccessStatusCode)
            return null;
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DiaryEntry>(content, _jsonOptions);
    }

    public async Task<DiaryEntry> CreateDiaryEntryAsync(DiaryEntry entry)
    {
        var json = JsonSerializer.Serialize(entry, _jsonOptions);
        var response = await _httpClient.PostAsync("diaryentries", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DiaryEntry>(content, _jsonOptions) 
            ?? throw new Exception("Error creating diary entry");
    }

    public async Task UpdateDiaryEntryAsync(DiaryEntry entry)
    {
        var json = JsonSerializer.Serialize(entry, _jsonOptions);
        var response = await _httpClient.PutAsync($"diaryentries/{entry.Id}", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteDiaryEntryAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"diaryentries/{id}");
        response.EnsureSuccessStatusCode();
    }

    // Payments
    public async Task<List<Payment>> GetPaymentsAsync(int? personId = null)
    {
        var url = personId.HasValue ? $"payments?personId={personId}" : "payments";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Payment>>(content, _jsonOptions) ?? new List<Payment>();
    }

    public async Task<Payment?> GetPaymentAsync(int id)
    {
        var response = await _httpClient.GetAsync($"payments/{id}");
        if (!response.IsSuccessStatusCode)
            return null;
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Payment>(content, _jsonOptions);
    }

    public async Task<Payment> CreatePaymentAsync(Payment payment)
    {
        var json = JsonSerializer.Serialize(payment, _jsonOptions);
        var response = await _httpClient.PostAsync("payments", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Payment>(content, _jsonOptions) 
            ?? throw new Exception("Error creating payment");
    }

    public async Task UpdatePaymentAsync(Payment payment)
    {
        var json = JsonSerializer.Serialize(payment, _jsonOptions);
        var response = await _httpClient.PutAsync($"payments/{payment.Id}", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
    }

    public async Task DeletePaymentAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"payments/{id}");
        response.EnsureSuccessStatusCode();
    }
}