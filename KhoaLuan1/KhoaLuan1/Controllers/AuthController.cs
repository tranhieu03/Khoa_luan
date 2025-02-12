using KhoaLuan1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace KhoaLuan1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly KhoaLuantestContext _context;

        public AuthController(KhoaLuantestContext context)
        {
            _context = context;
        }

        // API Đăng ký
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kiểm tra email đã tồn tại
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                return BadRequest(new { message = "Email is already in use." });

            // Tạo người dùng mới
            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role,
                PhoneNumber = model.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful." });
        }

        // API Đăng nhập
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            Console.WriteLine($"Login request received: {model.Email}");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password." });

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("PhoneNumber", user.PhoneNumber);


            return Ok(new { message = "Login successful." });
        }


        // API Kiểm tra trạng thái đăng nhập
        [HttpGet("status")]
        public IActionResult Status()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized(new { message = "Not logged in." });

            var fullName = HttpContext.Session.GetString("FullName");
            var email = HttpContext.Session.GetString("Email");
            var role = HttpContext.Session.GetString("Role");
            var phoneNumber = HttpContext.Session.GetString("PhoneNumber");

            return Ok(new
            {
                userId,
                fullName,
                email,
                role,
                phoneNumber
            });
        }

        // API Đăng xuất
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa toàn bộ dữ liệu trong Session
            return Ok(new { message = "Logout successful." });
        }
    }
    public class RegisterRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [RegularExpression("^(Customer|Seller|DeliveryPerson|Admin)$", ErrorMessage = "Invalid role.")]
        public string Role { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

}
