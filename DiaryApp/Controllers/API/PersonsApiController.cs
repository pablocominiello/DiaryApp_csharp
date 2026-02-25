using DiaryApp.Core.Data;
using DiaryApp.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiaryApp.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public PersonsController(ApplicationDbContext db)
        {
            _db = db;
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

                var persons = await query
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                return Ok(persons);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        // GET: api/persons/5
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

        // POST: api/persons
        [HttpPost]
        public async Task<ActionResult<Person>> CreatePerson(Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _db.Peoples.Add(person);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPerson), new { id = person.Id }, person);
        }

        // PUT: api/persons/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePerson(int id, Person person)
        {
            if (id != person.Id)
            {
                return BadRequest();
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

        // DELETE: api/persons/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerson(int id)
        {
            var person = await _db.Peoples.FindAsync(id);
            if (person == null)
            {
                return NotFound();
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