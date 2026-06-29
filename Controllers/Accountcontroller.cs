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

        // ==========================================
        // ✅ DELETE /api/account
        // حذف الحساب كامل — محتاج Token
        // ==========================================
        // ليه محتاجه؟
        // عشان المستخدم يقدر يحذف حسابه من الـ App
        // وعشان Cascade Delete هيحذف الأطفال والـ Screenings معاه
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            var user = await _context.User.FindAsync(userId);
            if (user == null)
                return NotFound("المستخدم مش موجود");

            // Cascade Delete هيحذف تلقائياً:
            // Children → Screenings → Answers + AIResults + PasswordResets
            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم حذف الحساب بنجاح" });
        }

        // ==========================================
        // ✅ DELETE /api/account/by-email/{email}
        // حذف أي حساب بالإيميل — للـ Testing بس
        // ==========================================
        // ليه محتاجه؟
        // عشان تقدر تمسح الإيميلات اللي بتعمل بيها Testing في Postman
        // من غير ما تدخل على الداتابيز يدوياً
        //
        // ⚠️ تحذير: اشيل الـ endpoint ده قبل ما تطلع Production
        // أو على الأقل ضيف عليه API Key خاص
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

        // ==========================================
        // ✅ GET /api/account/all-emails
        // شوف كل الإيميلات الموجودة — للـ Testing بس
        // ==========================================
        [HttpGet("all-emails")]
        public async Task<IActionResult> GetAllEmails()
        {
            var emails = await _context.User
                .Select(u => new { u.UserID, u.Email, u.FullName, u.CreatedAt })
                .ToListAsync();

            return Ok(emails);
        }

        // ==========================================
        // ✅ DELETE /api/account/clear-test-data
        // مسح كل بيانات الـ Testing دفعة واحدة
        // ==========================================
        // بيمسح كل المستخدمين ما عدا المستخدم رقم 1
        // عشان تبدأ من أول وجديد في الـ Testing
        [HttpDelete("clear-test-data")]
        public async Task<IActionResult> ClearTestData()
        {
            // احذف كل المستخدمين غير الـ admin (UserID > 1)
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