using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


namespace WebApplication1.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public InvoicesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/invoices
        [HttpGet]
        public async Task<ActionResult> GetInvoices()
        {
            var invoices = await _dbContext.Invoices.Include(i => i.Order).ToListAsync();
            return Ok(invoices);
        }

        // GET: api/invoices/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult> GetInvoice(int id)
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.Order)
                .FirstOrDefaultAsync(i => i.id == id);

            if (invoice == null)
            {
                return NotFound(new { message = "Invoice not found" });
            }

            return Ok(invoice);
        }

        // POST: api/invoices
        [HttpPost]
        public async Task<ActionResult> CreateInvoice(Invoice invoice)
        {
            // Validate if associated order exists
            var orderExists = await _dbContext.Orders.AnyAsync(o => o.id == invoice.orderId);

            if (!orderExists)
            {
                return BadRequest(new { message = "Invoice must be linked to an existing order" });
            }

            _dbContext.Invoices.Add(invoice);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.id }, invoice);
        }

        // PUT: api/invoices/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateInvoice(int id, Invoice invoice)
        {
            if (id != invoice.id)
            {
                return BadRequest(new { message = "Invoice ID mismatch" });
            }

            var existingInvoice = await _dbContext.Invoices.FindAsync(id);

            if (existingInvoice == null)
            {
                return NotFound(new { message = "Invoice not found" });
            }

            existingInvoice.orderId = invoice.orderId;

            _dbContext.Entry(existingInvoice).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceExists(id))
                {
                    return NotFound(new { message = "Invoice not found" });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/invoices/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteInvoice(int id)
        {
            var invoice = await _dbContext.Invoices.FindAsync(id);

            if (invoice == null)
            {
                return NotFound(new { message = "Invoice not found" });
            }

            _dbContext.Invoices.Remove(invoice);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool InvoiceExists(int id)
        {
            return _dbContext.Invoices.Any(i => i.id == id);
        }
    }
}
