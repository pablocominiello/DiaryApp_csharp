using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiaryApp.Core.Data;
using DiaryApp.Shared.Models;

namespace DiaryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PaymentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Payment>>> GetPayments([FromQuery] int? personId = null)
    {
        var query = _context.Payments.Include(p => p.Person).AsQueryable();

        if (personId.HasValue)
        {
            query = query.Where(p => p.PeoplesId == personId.Value);
        }

        return await query
            .OrderByDescending(p => p.Ano)
            .ThenByDescending(p => p.Mes)
            .ThenByDescending(p => p.Fecha)
            .ToListAsync();
    }

    [HttpPost("upload-image")]
    public async Task<ActionResult<string>> UploadImage([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        // Add your image upload logic here
        // For example, save to blob storage or local file system
        
        return Ok(new { url = "image-url-here" });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Payment>> GetPayment(int id)
    {
        var payment = await _context.Payments
            .Include(p => p.Person)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment == null)
        {
            return NotFound();
        }

        return payment;
    }

    [HttpPost]
    public async Task<ActionResult<Payment>> CreatePayment(Payment payment)
    {
        // ✅ ELIMINADO: Validación de duplicados
        // Ahora se permite múltiples pagos por persona/mes

        payment.Fecha = DateTime.Now;
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePayment(int id, Payment payment)
    {
        if (id != payment.Id)
        {
            return BadRequest();
        }

        // ✅ ELIMINADO: Validación de duplicados

        _context.Entry(payment).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PaymentExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePayment(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
        {
            return NotFound();
        }

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PaymentExists(int id)
    {
        return _context.Payments.Any(e => e.Id == id);
    }
}