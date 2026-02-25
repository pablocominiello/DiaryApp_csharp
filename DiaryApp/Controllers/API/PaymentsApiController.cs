using DiaryApp.Core.Data;
using DiaryApp.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiaryApp.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public PaymentsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: api/payments
        [HttpGet]
        public async Task<ActionResult<List<Payment>>> GetPayments([FromQuery] int? personId = null)
        {
            var query = _db.Payments.AsQueryable();

            if (personId.HasValue)
            {
                query = query.Where(p => p.PeoplesId == personId.Value);
            }

            var payments = await query
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            return Ok(payments);
        }

        // GET: api/payments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _db.Payments.FindAsync(id);

            if (payment == null)
            {
                return NotFound();
            }

            return Ok(payment);
        }

        // POST: api/payments
        [HttpPost]
        public async Task<ActionResult<Payment>> CreatePayment(Payment payment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar si ya existe un pago para esa persona en ese mes/año
            var existingPayment = await _db.Payments
                .FirstOrDefaultAsync(p => p.PeoplesId == payment.PeoplesId && 
                                        p.Ano == payment.Ano && 
                                        p.Mes == payment.Mes);

            if (existingPayment != null)
            {
                return Conflict(new { message = "Ya existe un pago para esta persona en este mes/año" });
            }

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
        }

        // PUT: api/payments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, Payment payment)
        {
            if (id != payment.Id)
            {
                return BadRequest();
            }

            _db.Entry(payment).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PaymentExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/payments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _db.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            _db.Payments.Remove(payment);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> PaymentExists(int id)
        {
            return await _db.Payments.AnyAsync(e => e.Id == id);
        }
    }
}