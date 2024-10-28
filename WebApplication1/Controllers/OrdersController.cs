using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public OrdersController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderReadDto>>> GetOrders()
        {
            var orders = await _dbContext.Orders
                .Include(o => o.OrderDetails)
                .Select(o => new OrderReadDto
                {
                    Id = o.id,
                    Name = o.name,
                    EmployeeId = o.employeeId,
                    TotalValue = o.totalValue,
                    Status = o.status,
                    OrderDetails = o.OrderDetails.Select(od => new OrderDetailReadDto
                    {
                        ArticleId = od.articleId,
                        Quantity = od.quantity
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderReadDto>> GetOrder(int id)
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.id == id)
                .Select(o => new OrderReadDto
                {
                    Id = o.id,
                    Name = o.name,
                    EmployeeId = o.employeeId,
                    TotalValue = o.totalValue,
                    Status = o.status,
                    OrderDetails = o.OrderDetails.Select(od => new OrderDetailReadDto
                    {
                        ArticleId = od.articleId,
                        Quantity = od.quantity
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound("Order not found");
            }

            return Ok(order);
        }

        // POST: api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto orderDto)
        {
            if (orderDto.OrderDetails == null || !orderDto.OrderDetails.Any())
            {
                return BadRequest("An order must contain at least one article.");
            }

            // Ensure articles belong to the same company as the employee who owns the order
            var employeeCompanyId = await _dbContext.Employees
                .Where(e => e.id == orderDto.EmployeeId)
                .Select(e => e.companyId)
                .FirstOrDefaultAsync();

            if (employeeCompanyId == 0 || !orderDto.OrderDetails.All(od =>
                _dbContext.Articles.Any(a => a.id == od.ArticleId && a.companyId == employeeCompanyId)))
            {
                return BadRequest("All articles must belong to the same company as the employee.");
            }

            var order = new Order
            {
                name = orderDto.Name,
                employeeId = orderDto.EmployeeId,
                totalValue = 0, // You might want to calculate total value based on order details
                status = "Pending", // Default status
                OrderDetails = orderDto.OrderDetails.Select(od => new OrderDetail
                {
                    articleId = od.ArticleId,
                    quantity = od.Quantity
                }).ToList()
            };

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.id }, order);
        }

        // PUT: api/orders/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] OrderCreateDto orderDto)
        {
            if (id != orderDto.EmployeeId)
            {
                return BadRequest("Order ID mismatch");
            }

            if (orderDto.OrderDetails == null || !orderDto.OrderDetails.Any())
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

            existingOrder.OrderDetails = orderDto.OrderDetails.Select(od => new OrderDetail
            {
                articleId = od.ArticleId,
                quantity = od.Quantity
            }).ToList();
            existingOrder.name = orderDto.Name; // Update other properties as needed

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
