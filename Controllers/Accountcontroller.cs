using AustimAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AustimAPI.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly apiDBContext _context;

        public AccountController(apiDBContext context)
        {
            _context = context;
        }


        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            var user = await _context.User.FindAsync(userId);
            if (user == null)
                return NotFound("المستخدم مش موجود");

            
            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم حذف الحساب بنجاح" });
        }

          [HttpDelete("by-email/{email}")]
        public async Task<IActionResult> DeleteByEmail(string email)
        {
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return NotFound($"مفيش حساب بالإيميل ده: {email}");

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"تم حذف الحساب: {email}" });
        }

      
        [HttpGet("all-emails")]
        public async Task<IActionResult> GetAllEmails()
        {
            var emails = await _context.User
                .Select(u => new { u.UserID, u.Email, u.FullName, u.CreatedAt })
                .ToListAsync();

            return Ok(emails);
        }

     
        [HttpDelete("clear-test-data")]
        public async Task<IActionResult> ClearTestData()
        {
            var testUsers = await _context.User
                .Where(u => u.UserID > 1)
                .ToListAsync();

            _context.User.RemoveRange(testUsers);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"تم حذف {testUsers.Count} مستخدم",
                remaining = await _context.User.CountAsync()
            });
        }
    }
}
