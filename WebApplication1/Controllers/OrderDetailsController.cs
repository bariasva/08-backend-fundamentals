using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
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
        public async Task<ActionResult<IEnumerable<OrderDetailReadDto>>> GetOrderDetails()
        {
            var orderDetails = await _dbContext.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Article)
                .Select(od => new OrderDetailReadDto
                {
                    Id = od.id,
                    OrderId = od.orderId,
                    ArticleId = od.articleId,
                    Quantity = od.quantity
                })
                .ToListAsync();

            return Ok(orderDetails);
        }

        // GET: api/orderdetails/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetailReadDto>> GetOrderDetail(int id)
        {
            var orderDetail = await _dbContext.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Article)
                .Where(od => od.id == id)
                .Select(od => new OrderDetailReadDto
                {
                    Id = od.id,
                    OrderId = od.orderId,
                    ArticleId = od.articleId,
                    Quantity = od.quantity
                })
                .FirstOrDefaultAsync();

            if (orderDetail == null)
            {
                return NotFound(new { message = "OrderDetail not found" });
            }

            return Ok(orderDetail);
        }

        // POST: api/orderdetails
        [HttpPost]
        public async Task<ActionResult<OrderDetailReadDto>> CreateOrderDetail(OrderDetailCreateDto orderDetailDto)
        {
            // Validate if associated order exists
            var orderExists = await _dbContext.Orders.AnyAsync(o => o.id == orderDetailDto.OrderId);
            if (!orderExists)
            {
                return BadRequest(new { message = "OrderDetail must be linked to an existing order" });
            }

            // Validate if associated article exists
            var articleExists = await _dbContext.Articles.AnyAsync(a => a.id == orderDetailDto.ArticleId);
            if (!articleExists)
            {
                return BadRequest(new { message = "OrderDetail must be linked to an existing article" });
            }

            var orderDetail = new OrderDetail
            {
                orderId = orderDetailDto.OrderId,
                articleId = orderDetailDto.ArticleId,
                quantity = orderDetailDto.Quantity
            };

            _dbContext.OrderDetails.Add(orderDetail);
            await _dbContext.SaveChangesAsync();

            var readDto = new OrderDetailReadDto
            {
                Id = orderDetail.id,
                OrderId = orderDetail.orderId,
                ArticleId = orderDetail.articleId,
                Quantity = orderDetail.quantity
            };

            return CreatedAtAction(nameof(GetOrderDetail), new { id = readDto.Id }, readDto);
        }

        // PUT: api/orderdetails/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrderDetail(int id, OrderDetailCreateDto orderDetailDto)
        {
            if (id != orderDetailDto.OrderId)
            {
                return BadRequest(new { message = "OrderDetail ID mismatch" });
            }

            var existingOrderDetail = await _dbContext.OrderDetails.FindAsync(id);

            if (existingOrderDetail == null)
            {
                return NotFound(new { message = "OrderDetail not found" });
            }

            existingOrderDetail.orderId = orderDetailDto.OrderId;
            existingOrderDetail.articleId = orderDetailDto.ArticleId;
            existingOrderDetail.quantity = orderDetailDto.Quantity;

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
