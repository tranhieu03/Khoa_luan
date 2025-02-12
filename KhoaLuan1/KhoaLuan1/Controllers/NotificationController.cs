﻿using KhoaLuan1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhoaLuan1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly KhoaLuantestContext _context;

        public NotificationController(KhoaLuantestContext context)
        {
            _context = context;
        }

        [HttpGet("get-notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return Unauthorized(new { message = "Bạn chưa đăng nhập." });

            List<Notification> notifications;

            if (role == "DeliveryPerson")
            {
                // Shipper chỉ nhận thông báo về đơn hàng đang giao
                notifications = await _context.Notifications
                    .Where(n => n.Message.Contains("Shipper đã nhận đơn hàng"))
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(20)
                    .ToListAsync();
            }
            else if (role == "seller")
            {
                // Seller nhận thông báo về trạng thái đơn hàng của họ
                notifications = await _context.Notifications
                    .Where(n => n.Message.Contains("đã nhận đơn hàng") || n.Message.Contains("đã giao"))
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(20)
                    .ToListAsync();
            }
            else
            {
                // Khách hàng nhận thông báo về đơn hàng của họ
                notifications = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(20)
                    .ToListAsync();
            }

            return Ok(notifications);
        }
        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized(new { message = "Người dùng chưa đăng nhập." });
            }

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (notifications.Count == 0)
            {
                return Ok(new { message = "Không có thông báo chưa đọc." });
            }

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Tất cả thông báo đã được đánh dấu là đã đọc." });
        }
        [HttpPost("mark-as-read")]
        public async Task<IActionResult> MarkNotificationsAsRead()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return Unauthorized(new { message = "User not logged in." });

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Thông báo đã được đánh dấu là đã đọc." });
        }

    }
}
