using DiaryApp.Core.Data;
using DiaryApp.Core.Interfaces;
using DiaryApp.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiaryApp.Controllers.Api
{
    [Route("api/persons")]
    [ApiController]
    public class PersonsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IBlobStorageService _blobStorageService;

        public PersonsApiController(ApplicationDbContext db, IBlobStorageService blobStorageService)
        {
            _db = db;
            _blobStorageService = blobStorageService;
        }

        // GET: api/persons
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Person>>> GetAllPersons()
        {
            var persons = await _db.Peoples.ToListAsync();
            return Ok(persons);
        }

        // GET: api/persons/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Person>> GetPerson(int id)
        {
            var person = await _db.Peoples.FindAsync(id);
            
            if (person == null)
            {
                return NotFound();
            }

            return Ok(person);
        }

        // POST: api/persons/upload-image
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage([FromForm] int? id, [FromForm] IFormFile? imagenFile)
        {
            if (imagenFile == null || imagenFile.Length == 0)
            {
                return BadRequest(new { message = "No se proporcionó ninguna imagen" });
            }

            try
            {
                // Subir imagen a Azure Blob Storage
                using var stream = imagenFile.OpenReadStream();
                var imageUrl = await _blobStorageService.UploadImageAsync(stream, imagenFile.FileName, "persons");

                // Si se proporcionó un id, actualizar la persona existente
                if (id.HasValue && id.Value > 0)
                {
                    var person = await _db.Peoples.FindAsync(id.Value);
                    if (person == null)
                    {
                        return NotFound(new { message = "Persona no encontrada" });
                    }

                    // Eliminar la imagen anterior si existe
                    if (!string.IsNullOrEmpty(person.ImagenUrl))
                    {
                        await _blobStorageService.DeleteImageAsync(person.ImagenUrl, "persons");
                    }

                    person.ImagenUrl = imageUrl;
                    await _db.SaveChangesAsync();

                    return Ok(new { imageUrl, personId = person.Id, message = "Imagen actualizada exitosamente" });
                }

                // Si no se proporcionó id, solo devolver la URL de la imagen
                return Ok(new { imageUrl, message = "Imagen subida exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al subir la imagen: {ex.Message}" });
            }
        }

        // POST: api/persons
        [HttpPost]
        public async Task<ActionResult<Person>> CreatePerson([FromBody] Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _db.Peoples.Add(person);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPerson), new { id = person.Id }, person);
        }

        // PUT: api/persons/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePerson(int id, [FromBody] Person person)
        {
            if (id != person.Id)
            {
                return BadRequest(new { message = "El ID no coincide" });
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
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/persons/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerson(int id)
        {
            var person = await _db.Peoples.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }

            // Eliminar imagen de Azure Blob Storage si existe
            if (!string.IsNullOrEmpty(person.ImagenUrl))
            {
                try
                {
                    await _blobStorageService.DeleteImageAsync(person.ImagenUrl, "persons");
                }
                catch
                {
                    // Continuar con la eliminación aunque falle eliminar la imagen
                }
            }

            _db.Peoples.Remove(person);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> PersonExists(int id)
        {
            return await _db.Peoples.AnyAsync(e => e.Id == id);
        }
    }
}