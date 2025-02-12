﻿using KhoaLuan1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhoaLuan1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryController : ControllerBase
    {
        private readonly KhoaLuantestContext _context;

        public DeliveryController (KhoaLuantestContext context)
        {
            _context = context;
        }

        [HttpGet("available-orders")]
        public async Task<IActionResult> GetAvailableOrders()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            try
            {
                var availableOrders = await _context.Orders
                    .Where(o => o.Status == "ReadyForDelivery" && o.DeliveryPersonId == null)
                    .Include(o => o.Restaurant)
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                     .Include(o => o.User)
                    .ToListAsync();

                return Ok(new
                {
                    Success = true,
                    Orders = availableOrders.Select(o => new
                    {
                        o.OrderId,
                        o.Status,
                        o.TotalAmount,
                        o.OrderDate,
                        o.PaymentStatus,
                        RestaurantName = o.Restaurant.Name,
                        RestaurantAddress = o.Restaurant.Address,
                        CustomerName = o.User.FullName, // Thông tin khách hàng
                        CustomerPhone = o.User.PhoneNumber, // Số điện thoại khách hàng
                        o.Address,
                        Items = o.OrderDetails.Select(od => new
                        {
                            od.Product.Name,
                            od.Quantity,
                            od.Price
                        })
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

    }
}
