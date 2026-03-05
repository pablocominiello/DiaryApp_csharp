using DiaryApp.Core.Data;
using DiaryApp.Core.Interfaces;
using DiaryApp.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace DiaryApp.Controllers.Api
{
    [Route("api/persons")]
    [ApiController]
    public class PersonsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<PersonsApiController> _logger;

        public PersonsApiController(
            ApplicationDbContext db, 
            IBlobStorageService blobStorageService,
            ILogger<PersonsApiController> logger)
        {
            _db = db;
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        // GET: api/persons
        [HttpGet]
        public async Task<ActionResult<List<Person>>> GetPersons([FromQuery] string? searchText = null)
        {
            try
            {
                var query = _db.Peoples.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(p => p.Nombre.Contains(searchText));
                }

                var persons = await query.OrderBy(p => p.Nombre).ToListAsync();
                return Ok(persons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting persons");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/persons/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Person>> GetPerson(int id)
        {
            try
            {
                var person = await _db.Peoples.FindAsync(id);

                if (person == null)
                {
                    return NotFound(new { error = $"Person with ID {id} not found" });
                }

                return Ok(person);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting person {PersonId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST: api/persons
        [HttpPost]
        public async Task<ActionResult<Person>> CreatePerson([FromBody] Person person)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _db.Peoples.Add(person);
                await _db.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPerson), new { id = person.Id }, person);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating person");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // PUT: api/persons/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePerson(int id, [FromBody] Person person)
        {
            try
            {
                if (id != person.Id)
                {
                    return BadRequest(new { error = "ID mismatch" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _db.Entry(person).State = EntityState.Modified;

                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await PersonExists(id))
                    {
                        return NotFound(new { error = $"Person with ID {id} not found" });
                    }
                    throw;
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating person {PersonId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // DELETE: api/persons/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerson(int id)
        {
            try
            {
                var person = await _db.Peoples.FindAsync(id);
                if (person == null)
                {
                    return NotFound(new { error = $"Person with ID {id} not found" });
                }

                // Delete image from Azure Blob Storage
                if (!string.IsNullOrEmpty(person.ImagenUrl))
                {
                    try
                    {
                        await _blobStorageService.DeleteImageAsync(person.ImagenUrl, "persons");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete image for person {PersonId}", id);
                        // Continue with deletion even if image deletion fails
                    }
                }

                _db.Peoples.Remove(person);
                await _db.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting person {PersonId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST: api/persons/upload-image
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage([FromBody] ImageUploadRequest request)
        {
            try
            {
                _logger.LogInformation("Received upload image request for person {PersonId}", request?.PersonId);

                // Validate request
                if (request == null)
                {
                    _logger.LogWarning("Upload image request is null");
                    return BadRequest(new { error = "Request cannot be null" });
                }

                if (string.IsNullOrEmpty(request.Base64Image))
                {
                    _logger.LogWarning("Base64 image is empty for person {PersonId}", request.PersonId);
                    return BadRequest(new { error = "Base64Image cannot be empty" });
                }

                // Find the person
                var person = await _db.Peoples.FindAsync(request.PersonId);
                if (person == null)
                {
                    _logger.LogWarning("Person {PersonId} not found", request.PersonId);
                    return NotFound(new { error = $"Person with ID {request.PersonId} not found" });
                }

                // Delete old image if exists
                if (!string.IsNullOrEmpty(person.ImagenUrl))
                {
                    try
                    {
                        await _blobStorageService.DeleteImageAsync(person.ImagenUrl, "persons");
                        _logger.LogInformation("Deleted old image for person {PersonId}", request.PersonId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old image for person {PersonId}", request.PersonId);
                    }
                }

                // Convert base64 to stream
                byte[] imageBytes;
                try
                {
                    imageBytes = Convert.FromBase64String(request.Base64Image);
                    _logger.LogInformation("Successfully decoded base64 image, size: {Size} bytes", imageBytes.Length);
                }
                catch (FormatException ex)
                {
                    _logger.LogError(ex, "Invalid base64 format for person {PersonId}", request.PersonId);
                    return BadRequest(new { error = "Invalid base64 image format" });
                }

                using var imageStream = new MemoryStream(imageBytes);

                // Upload to Azure Blob Storage
                var imageUrl = await _blobStorageService.UploadImageAsync(
                    imageStream,
                    request.FileName ?? "image.jpg",
                    "persons");

                _logger.LogInformation("Uploaded image to blob storage: {ImageUrl}", imageUrl);

                // Update person record
                person.ImagenUrl = imageUrl;
                _db.Peoples.Update(person);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Successfully updated person {PersonId} with new image", request.PersonId);

                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error uploading image for person {PersonId}", request?.PersonId);
                return StatusCode(500, new { error = $"Error uploading image: {ex.Message}" });
            }
        }

        private async Task<bool> PersonExists(int id)
        {
            return await _db.Peoples.AnyAsync(e => e.Id == id);
        }
    }

    public class ImageUploadRequest
    {
        [JsonPropertyName("personId")]
        public int PersonId { get; set; }

        [JsonPropertyName("base64Image")]
        public string Base64Image { get; set; } = string.Empty;

        [JsonPropertyName("fileName")]
        public string? FileName { get; set; }
    }
}