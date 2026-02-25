using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiaryApp.Api.Data;
using DiaryApp.Shared.Models;

namespace DiaryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PersonsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/persons
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Person>>> GetPersons([FromQuery] string? searchText = null)
    {
        var query = _context.Peoples.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(p => p.Nombre.Contains(searchText) || p.Content.Contains(searchText));
        }

        return await query.OrderBy(p => p.Nombre).ToListAsync();
    }

    // GET: api/persons/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Person>> GetPerson(int id)
    {
        var person = await _context.Peoples
            .Include(p => p.Payments)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (person == null)
        {
            return NotFound();
        }

        return person;
    }

    // POST: api/persons
    [HttpPost]
    public async Task<ActionResult<Person>> CreatePerson(Person person)
    {
        _context.Peoples.Add(person);
        await _context.SaveChangesAsync();

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

        _context.Entry(person).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PersonExists(id))
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
        var person = await _context.Peoples.FindAsync(id);
        if (person == null)
        {
            return NotFound();
        }

        _context.Peoples.Remove(person);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PersonExists(int id)
    {
        return _context.Peoples.Any(e => e.Id == id);
    }
}