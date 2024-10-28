using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public OrdersController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/orders
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _dbContext.Orders
                .Include(o => o.OrderDetails)
                .ToListAsync();
            return Ok(orders);
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.id == id);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            return Ok(order);
        }

        // POST: api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order order)
        {
            if (order.OrderDetails == null || !order.OrderDetails.Any())
            {
                return BadRequest("An order must contain at least one article.");
            }

            // Ensure articles belong to the same company as the employee who owns the order
            var employeeCompanyId = await _dbContext.Employees
                .Where(e => e.id == order.employeeId)
                .Select(e => e.companyId)
                .FirstOrDefaultAsync();

            if (employeeCompanyId == 0 || !order.OrderDetails.All(od =>
                _dbContext.Articles.Any(a => a.id == od.articleId && a.companyId == employeeCompanyId)))
            {
                return BadRequest("All articles must belong to the same company as the employee.");
            }

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.id }, order);
        }

        // PUT: api/orders/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, Order order)
        {
            if (id != order.id)
            {
                return BadRequest("Order ID mismatch");
            }

            if (order.OrderDetails == null || !order.OrderDetails.Any())
            {
                return BadRequest("An order must contain at least one article.");
            }

            var existingOrder = await _dbContext.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.id == id);

            if (existingOrder == null)
            {
                return NotFound("Order not found");
            }

            existingOrder.OrderDetails = order.OrderDetails;
            _dbContext.Entry(existingOrder).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound("Order not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/orders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.id == id);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            _dbContext.Orders.Remove(order);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderExists(int id)
        {
            return _dbContext.Orders.Any(o => o.id == id);
        }
    }
}
