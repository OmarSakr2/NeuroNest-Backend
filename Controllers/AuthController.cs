using AustimAPI.DTOs;
using AustimAPI.Models;
using AustimAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AustimAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly apiDBContext _context;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;

        public AuthController(apiDBContext context, IConfiguration config, EmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            if (_context.User.Any(x => x.Email == dto.Email))
                return BadRequest(new { message = "الإيميل ده موجود بالفعل" });

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsVerified = false
            };
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            var code = new Random().Next(100000, 999999).ToString();
            _context.PasswordReset.Add(new PasswordReset
            {
                UserID = user.UserID,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            });
            await _context.SaveChangesAsync();

            await _emailService.SendOtpAsync(dto.Email, code);
            return Ok(new { message = "تم إنشاء الحساب! افتح إيميلك وادخل الكود", email = dto.Email });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailDTO dto)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return BadRequest(new { message = "الإيميل مش موجود" });
            if (user.IsVerified) return Ok(new { message = "الإيميل متأكد بالفعل" });

            var record = await _context.PasswordReset
                .Where(p => p.UserID == user.UserID && p.Code == dto.Code &&
                            !p.IsUsed && p.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (record == null) return BadRequest(new { message = "الكود غلط أو انتهت صلاحيته" });

            user.IsVerified = true;
            record.IsUsed = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم تأكيد إيميلك! تقدر تدخل دلوقتي" });
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification(ForgotPasswordDTO dto)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return Ok(new { message = "لو الإيميل صح هتستلم كود" });
            if (user.IsVerified) return BadRequest(new { message = "الإيميل متأكد بالفعل" });

            var oldCodes = _context.PasswordReset.Where(p => p.UserID == user.UserID && !p.IsUsed);
            _context.PasswordReset.RemoveRange(oldCodes);

            var code = new Random().Next(100000, 999999).ToString();
            _context.PasswordReset.Add(new PasswordReset
            {
                UserID = user.UserID,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            });
            await _context.SaveChangesAsync();
            await _emailService.SendOtpAsync(dto.Email, code);
            return Ok(new { message = "تم إرسال كود جديد" });
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDTO login)
        {
            var user = _context.User.FirstOrDefault(x => x.Email == login.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
                return Unauthorized(new { message = "الإيميل أو الباسورد غلط" });

            if (!user.IsVerified)
                return Unauthorized(new
                {
                    message = "لازم تأكد إيميلك الأول",
                    isVerified = false
                });

            return Ok(new
            {
                token = GenerateToken(user),
                tokenType = "Bearer",
                userId = user.UserID,
                fullName = user.FullName,
                isVerified = true
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO dto)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return Ok(new { message = "لو الإيميل صح هتستلم الكود" });

            var oldCodes = _context.PasswordReset.Where(p => p.UserID == user.UserID && !p.IsUsed);
            _context.PasswordReset.RemoveRange(oldCodes);

            var code = new Random().Next(100000, 999999).ToString();
            _context.PasswordReset.Add(new PasswordReset
            {
                UserID = user.UserID,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            });
            await _context.SaveChangesAsync();
            await _emailService.SendOtpAsync(dto.Email, code);
            return Ok(new { message = "تم إرسال الكود على إيميلك", expiresInMinutes = 15 });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO dto)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return BadRequest(new { message = "الإيميل مش موجود" });

            var resetRecord = await _context.PasswordReset
                .Where(p => p.UserID == user.UserID && p.Code == dto.Code &&
                            !p.IsUsed && p.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (resetRecord == null) return BadRequest(new { message = "الكود غلط أو انتهت صلاحيته" });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            resetRecord.IsUsed = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم تغيير الباسورد بنجاح" });
        }
        [AllowAnonymous]
        [HttpPost("google-signin")]
        public async Task<IActionResult> GoogleSignIn(GoogleSignInDTO dto)
        {
            GooglePayload? payload;
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetStringAsync(
                    $"https://oauth2.googleapis.com/tokeninfo?id_token={dto.IdToken}");
                payload = System.Text.Json.JsonSerializer.Deserialize<GooglePayload>(response,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (payload == null || string.IsNullOrEmpty(payload.Email))
                    return Unauthorized(new { message = "Google Token غير صالح" });
            }
            catch { return Unauthorized(new { message = "فشل التحقق من Google Token" }); }

            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == payload.Email);
            if (user == null)
            {
                user = new User
                {
                    FullName = payload.Name ?? payload.Email,
                    Email = payload.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    IsVerified = true
                };
                _context.User.Add(user);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                token = GenerateToken(user),
                tokenType = "Bearer",
                userId = user.UserID,
                fullName = user.FullName
            });
        }

        private string GenerateToken(User user)
        {
            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var claims = new List<Claim>
            {
                new Claim("id", user.UserID.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"], audience: jwt["Audience"], claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwt["AccessExpirationMinutes"]!)),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class GooglePayload
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Sub { get; set; }
    }
}