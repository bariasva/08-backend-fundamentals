using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public OrderDetailsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/orderdetails
        [HttpGet]
        public async Task<ActionResult> GetOrderDetails()
        {
            var orderDetails = await _dbContext.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Article)
                .ToListAsync();

            return Ok(orderDetails);
        }

        // GET: api/orderdetails/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult> GetOrderDetail(int id)
        {
            var orderDetail = await _dbContext.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Article)
                .FirstOrDefaultAsync(od => od.id == id);

            if (orderDetail == null)
            {
                return NotFound(new { message = "OrderDetail not found" });
            }

            return Ok(orderDetail);
        }

        // POST: api/orderdetails
        [HttpPost]
        public async Task<ActionResult> CreateOrderDetail(OrderDetail orderDetail)
        {
            // Validate if associated order exists
            var orderExists = await _dbContext.Orders.AnyAsync(o => o.id == orderDetail.orderId);
            if (!orderExists)
            {
                return BadRequest(new { message = "OrderDetail must be linked to an existing order" });
            }

            // Validate if associated article exists
            var articleExists = await _dbContext.Articles.AnyAsync(a => a.id == orderDetail.articleId);
            if (!articleExists)
            {
                return BadRequest(new { message = "OrderDetail must be linked to an existing article" });
            }

            _dbContext.OrderDetails.Add(orderDetail);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrderDetail), new { id = orderDetail.id }, orderDetail);
        }

        // PUT: api/orderdetails/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrderDetail(int id, OrderDetail orderDetail)
        {
            if (id != orderDetail.id)
            {
                return BadRequest(new { message = "OrderDetail ID mismatch" });
            }

            var existingOrderDetail = await _dbContext.OrderDetails.FindAsync(id);

            if (existingOrderDetail == null)
            {
                return NotFound(new { message = "OrderDetail not found" });
            }

            existingOrderDetail.orderId = orderDetail.orderId;
            existingOrderDetail.articleId = orderDetail.articleId;
            existingOrderDetail.quantity = orderDetail.quantity;

            _dbContext.Entry(existingOrderDetail).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderDetailExists(id))
                {
                    return NotFound(new { message = "OrderDetail not found" });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/orderdetails/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrderDetail(int id)
        {
            var orderDetail = await _dbContext.OrderDetails.FindAsync(id);

            if (orderDetail == null)
            {
                return NotFound(new { message = "OrderDetail not found" });
            }

            _dbContext.OrderDetails.Remove(orderDetail);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderDetailExists(int id)
        {
            return _dbContext.OrderDetails.Any(od => od.id == id);
        }
    }
}
