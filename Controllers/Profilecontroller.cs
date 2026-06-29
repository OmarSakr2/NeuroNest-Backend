using AustimAPI.DTOs;
using AustimAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AustimAPI.Controllers
{
    [Authorize]   // كل الـ endpoints دي محتاجة Login
    [Route("api/profile")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly apiDBContext _context;

        public ProfileController(apiDBContext context)
        {
            _context = context;
        }

         [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            var user = await _context.User
                .Where(u => u.UserID == userId)
                .Select(u => new
                {
                    u.UserID,
                    u.FullName,
                    u.Email,
                    u.CreatedAt,

                    Children = u.Children.Select(c => new
                    {
                        c.ChildID,
                        c.ChildName,
                        c.DateOfBirth,
                        c.Gender,

                        LastScreening = c.Screenings
                            .OrderByDescending(s => s.ScreeningDate)
                            .Select(s => new
                            {
                                s.ScreeningID,
                                s.ScreeningDate,
                                s.TotalScore,
                                s.RiskLevel,
                                s.Status
                            })
                            .FirstOrDefault()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("المستخدم مش موجود");

            return Ok(user);
        }

      
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDTO dto)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            var user = await _context.User.FindAsync(userId);
            if (user == null)
                return NotFound("المستخدم مش موجود");

            if (string.IsNullOrWhiteSpace(dto.FullName))
                return BadRequest("الاسم لازم يكون موجود");

            user.FullName = dto.FullName;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تحديث البيانات بنجاح",
                fullName = user.FullName
            });
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO dto)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            var user = await _context.User.FindAsync(userId);
            if (user == null)
                return NotFound("المستخدم مش موجود");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return BadRequest("الباسورد الحالي غلط");

            if (dto.CurrentPassword == dto.NewPassword)
                return BadRequest("الباسورد الجديد لازم يكون مختلف عن الحالي");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تغيير الباسورد بنجاح" });
        }
    }
}
