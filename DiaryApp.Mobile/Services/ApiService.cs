using System.Net.Http.Json;
using System.Net.Http.Headers;
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

    // ✅ NUEVO: Obtener Person por UserId
    public async Task<Person?> GetPersonByUserIdAsync(string userId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔍 Fetching person by UserId: {userId}");
            
            var response = await _httpClient.GetAsync($"persons/by-user/{userId}");
            
            System.Diagnostics.Debug.WriteLine($"📊 Response Status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Person not found for UserId: {userId}");
                return null;
            }
            
            var content = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"📄 Response Content: {content}");
            
            var person = JsonSerializer.Deserialize<Person>(content, _jsonOptions);
            
            System.Diagnostics.Debug.WriteLine($"✅ Successfully loaded person for UserId: {userId}, PersonId: {person?.Id}");
            
            return person;
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ JSON Error getting person by UserId: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error getting person by UserId: {ex.Message}");
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

    public async Task<string> UploadPersonImageAsync(int personId, string base64Image, string fileName)
    {
        try
        {
            var payload = new
            {
                PersonId = personId,
                Base64Image = base64Image,
                FileName = fileName
            };

            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            var response = await _httpClient.PostAsync("persons/upload-image", 
                new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
            
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ImageUploadResponse>(content, _jsonOptions);
            
            return result?.ImageUrl ?? throw new Exception("No image URL returned");
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Error uploading image: {ex.Message}");
            throw new Exception($"Error uploading image: {ex.Message}", ex);
        }
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
        try
        {
            var url = personId.HasValue ? $"payments?personId={personId}" : "payments";
            
            System.Diagnostics.Debug.WriteLine($"🔍 Fetching payments from: {url}");
            
            var response = await _httpClient.GetAsync(url);
            
            System.Diagnostics.Debug.WriteLine($"📊 Response Status: {response.StatusCode}");
            
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            
            System.Diagnostics.Debug.WriteLine($"📄 Response Content Length: {content?.Length ?? 0}");
            System.Diagnostics.Debug.WriteLine($"📄 Response Content: {content}");
            
            if (string.IsNullOrWhiteSpace(content))
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Empty response from API - returning empty list");
                return new List<Payment>();
            }
            
            var payments = JsonSerializer.Deserialize<List<Payment>>(content, _jsonOptions) ?? new List<Payment>();
            
            System.Diagnostics.Debug.WriteLine($"✅ Successfully deserialized {payments.Count} payments");
            
            return payments;
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ JSON Error getting payments: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
            throw new Exception($"Error parsing payments data: {ex.Message}", ex);
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ HTTP Error getting payments: {ex.Message}");
            throw new Exception($"Error fetching payments: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Unexpected error getting payments: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<Payment?> GetPaymentAsync(int id)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔍 Fetching payment ID: {id}");
            
            var response = await _httpClient.GetAsync($"payments/{id}");
            
            System.Diagnostics.Debug.WriteLine($"📊 Response Status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Payment {id} not found");
                return null;
            }
            
            var content = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"📄 Response Content: {content}");
            
            var payment = JsonSerializer.Deserialize<Payment>(content, _jsonOptions);
            
            System.Diagnostics.Debug.WriteLine($"✅ Successfully loaded payment {id}");
            
            return payment;
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ JSON Error getting payment {id}: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error getting payment {id}: {ex.Message}");
            throw;
        }
    }

    public async Task<Payment> CreatePaymentAsync(Payment payment)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"➕ Creating payment for PersonId: {payment.PeoplesId}");
            
            var json = JsonSerializer.Serialize(payment, _jsonOptions);
            
            System.Diagnostics.Debug.WriteLine($"📄 Payload: {json}");
            
            var response = await _httpClient.PostAsync("payments", 
                new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
            
            System.Diagnostics.Debug.WriteLine($"📊 Response Status: {response.StatusCode}");
            
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            
            System.Diagnostics.Debug.WriteLine($"📄 Response: {content}");
            
            var createdPayment = JsonSerializer.Deserialize<Payment>(content, _jsonOptions) 
                ?? throw new Exception("Error creating payment");
            
            System.Diagnostics.Debug.WriteLine($"✅ Payment created with ID: {createdPayment.Id}");
            
            return createdPayment;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error creating payment: {ex.Message}");
            throw;
        }
    }

    public async Task UpdatePaymentAsync(Payment payment)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"✏️ Updating payment ID: {payment.Id}");
            
            var json = JsonSerializer.Serialize(payment, _jsonOptions);
            var response = await _httpClient.PutAsync($"payments/{payment.Id}", 
                new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
            
            System.Diagnostics.Debug.WriteLine($"📊 Response Status: {response.StatusCode}");
            
            response.EnsureSuccessStatusCode();
            
            System.Diagnostics.Debug.WriteLine($"✅ Payment {payment.Id} updated successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error updating payment {payment.Id}: {ex.Message}");
            throw;
        }
    }

    public async Task DeletePaymentAsync(int id)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🗑️ Deleting payment ID: {id}");
            
            var response = await _httpClient.DeleteAsync($"payments/{id}");
            
            System.Diagnostics.Debug.WriteLine($"📊 Response Status: {response.StatusCode}");
            
            response.EnsureSuccessStatusCode();
            
            System.Diagnostics.Debug.WriteLine($"✅ Payment {id} deleted successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error deleting payment {id}: {ex.Message}");
            throw;
        }
    }

    // Helper class for image upload response
    private class ImageUploadResponse
    {
        public string? ImageUrl { get; set; }
    }
}
