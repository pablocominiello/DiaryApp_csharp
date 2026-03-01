using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiaryApp.Core.Data; // ✅ Cambiar de DiaryApp.Api.Data
using DiaryApp.Shared.Models;

namespace DiaryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiaryEntriesController : ControllerBase
{
    private readonly ApplicationDbContext _context; // ✅ Cambiar de AppDbContext

    public DiaryEntriesController(ApplicationDbContext context) // ✅ Cambiar tipo
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DiaryEntry>>> GetDiaryEntries()
    {
        return await _context.DiaryEntries
            .OrderByDescending(d => d.DateCreated)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DiaryEntry>> GetDiaryEntry(int id)
    {
        var entry = await _context.DiaryEntries.FindAsync(id);

        if (entry == null)
        {
            return NotFound();
        }

        return entry;
    }

    [HttpPost]
    public async Task<ActionResult<DiaryEntry>> CreateDiaryEntry(DiaryEntry entry)
    {
        entry.DateCreated = DateTime.Now;
        _context.DiaryEntries.Add(entry);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDiaryEntry), new { id = entry.Id }, entry);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDiaryEntry(int id, DiaryEntry entry)
    {
        if (id != entry.Id)
        {
            return BadRequest();
        }

        _context.Entry(entry).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DiaryEntryExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDiaryEntry(int id)
    {
        var entry = await _context.DiaryEntries.FindAsync(id);
        if (entry == null)
        {
            return NotFound();
        }

        _context.DiaryEntries.Remove(entry);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool DiaryEntryExists(int id)
    {
        return _context.DiaryEntries.Any(e => e.Id == id);
    }
}