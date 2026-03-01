using DiaryApp.Core.Data;
using DiaryApp.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiaryApp.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiaryEntriesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public DiaryEntriesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: api/diaryentries
        [HttpGet]
        public async Task<ActionResult<List<DiaryEntry>>> GetDiaryEntries()
        {
            var entries = await _db.DiaryEntries
                .OrderByDescending(d => d.DateCreated)
                .ToListAsync();

            return Ok(entries);
        }

        // GET: api/diaryentries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DiaryEntry>> GetDiaryEntry(int id)
        {
            var entry = await _db.DiaryEntries.FindAsync(id);

            if (entry == null)
            {
                return NotFound();
            }

            return Ok(entry);
        }

        // POST: api/diaryentries
        [HttpPost]
        public async Task<ActionResult<DiaryEntry>> CreateDiaryEntry(DiaryEntry entry)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            entry.DateCreated = DateTime.Now;
            _db.DiaryEntries.Add(entry);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDiaryEntry), new { id = entry.Id }, entry);
        }

        // PUT: api/diaryentries/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDiaryEntry(int id, DiaryEntry entry)
        {
            if (id != entry.Id)
            {
                return BadRequest();
            }

            _db.Entry(entry).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await EntryExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/diaryentries/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiaryEntry(int id)
        {
            var entry = await _db.DiaryEntries.FindAsync(id);
            if (entry == null)
            {
                return NotFound();
            }

            _db.DiaryEntries.Remove(entry);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> EntryExists(int id)
        {
            return await _db.DiaryEntries.AnyAsync(e => e.Id == id);
        }
    }
}