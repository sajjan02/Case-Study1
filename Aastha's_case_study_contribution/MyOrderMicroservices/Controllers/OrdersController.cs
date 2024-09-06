using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyOrderMicroservices.Models;
using Newtonsoft.Json;

namespace MyOrderMicroservices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly RetailManagement1Context _context;

        public OrdersController(RetailManagement1Context context)
        {
            _context = context;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders.ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // PUT: api/Orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.OrderId)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            _context.Orders.Add(order);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (OrderExists(order.OrderId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
        }

        // POST: api/Orders/addorders
        [HttpPost("addorders")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDto orderDto)
        {
            Console.WriteLine("Received OrderDto:");
            Console.WriteLine(JsonConvert.SerializeObject(orderDto, Formatting.Indented));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return BadRequest(ModelState);
            }

            if (orderDto == null)
            {
                return BadRequest(new { message = "OrderDto cannot be null." });
            }

            if (orderDto.Items == null || !orderDto.Items.Any())
            {
                return BadRequest(new { message = "Order items cannot be null or empty." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create new order
                var order = new Order
                {
                    UserId = orderDto.UserId,
                    OrderDate = DateOnly.FromDateTime(DateTime.Now), // Convert DateTime to DateOnly
                    Status = "Booked"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Process each item in the order
                foreach (var item in orderDto.Items)
                {
                    if (item == null) continue;

                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        UnitPrice = item.UnitPrice,  // Ensure that OrderItem has a Price property
                        Quantity = item.Quantity
                    };

                    _context.OrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Find the order by ID and include related entities
                    var order = await _context.Orders
                        .Include(o => o.OrderItems)
                        .Include(o => o.Shipments)
                        .FirstOrDefaultAsync(o => o.OrderId == id);

                    if (order == null)
                    {
                        return NotFound(new { message = "Order not found." });
                    }

                    // Remove related data first
                    if (order.OrderItems != null && order.OrderItems.Any())
                    {
                        _context.OrderItems.RemoveRange(order.OrderItems);
                    }

                    if (order.Shipments != null && order.Shipments.Any())
                    {
                        _context.Shipments.RemoveRange(order.Shipments);
                    }

                    // Remove the order itself
                    _context.Orders.Remove(order);

                    // Save changes to the database
                    await _context.SaveChangesAsync();

                    // Commit the transaction
                    await transaction.CommitAsync();

                    return NoContent();
                }
                catch (DbUpdateException dbEx)
                {
                    // Log the detailed exception message and inner exception details
                    Console.WriteLine($"DbUpdateException: {dbEx.Message}");
                    if (dbEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
                    }

                    // Rollback the transaction in case of an error
                    await transaction.RollbackAsync();
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = dbEx.Message });
                }
                catch (Exception ex)
                {
                    // Log the general exception message and stack trace
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                    // Rollback the transaction in case of an error
                    await transaction.RollbackAsync();
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
                }
            }
        }

        public class OrderDto
    {
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}
