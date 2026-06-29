using AustimAPI.DTOs;
using AustimAPI.Models;
using AustimAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AustimAPI.Controllers
{
    [Authorize]
    [Route("api/video")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly apiDBContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly AIService _aiService;

        private static readonly string[] AllowedExtensions =
            { ".mp4", ".mov", ".avi", ".mkv" };

        public VideoController(apiDBContext context, IWebHostEnvironment env, AIService aiService)
        {
            _context = context;
            _env = env;
            _aiService = aiService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadVideo([FromForm] UploadVideoDTO dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("id")?.Value ?? "0");

                var child = await _context.Child
                    .FirstOrDefaultAsync(c => c.ChildID == dto.ChildID && c.ParentID == userId);

                if (child == null)
                    return Unauthorized(new { message = "الطفل مش موجود أو مش ليك" });

                var ext = Path.GetExtension(dto.Video.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                    return BadRequest(new
                    {
                        message = "نوع الملف غير مسموح. الأنواع المسموحة: mp4, mov, avi, mkv"
                    });

                // ✅ حفظ الفيديو
                var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "videos");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await dto.Video.CopyToAsync(stream);

                var videoUrl = $"{Request.Scheme}://{Request.Host}/videos/{fileName}";

                // ✅ إنشاء Screening للفيديو
                var screening = new Screening
                {
                    ChildID = dto.ChildID,
                    ScreeningDate = DateTime.UtcNow,
                    Status = "Processing",
                    RiskLevel = "Pending",
                    TotalScore = 0,
                    ScreeningType = "Video"
                };

                _context.Screening.Add(screening);
                await _context.SaveChangesAsync();

                // ✅ بعت الـ filePath الكامل للـ AI
                var aiResult = await _aiService.AnalyzeVideo(filePath);

                // ✅ لو الـ AI فشل — احذف الفيديو وحدّث الـ Status
                if (aiResult == null)
                {
                    screening.Status = "Failed";
                    await _context.SaveChangesAsync();

                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);

                    return BadRequest(new { message = "فشل تحليل الفيديو" });
                }

                // ✅ تأمين القيمة null قبل أي عملية حسابية (RiskScorePercentage بقت float?)
                var scorePercentage = aiResult.RiskScorePercentage ?? 0f;

                // ✅ تحديد RiskLevel
                var riskLevel = scorePercentage >= 70 ? "High"
                              : scorePercentage >= 40 ? "Moderate"
                              : "Low";

                // ✅ رسالة واضحة بـ switch
                var message = riskLevel switch
                {
                    "High" => "ننصح بزيارة متخصص فوراً",
                    "Moderate" => "يستحسن متابعة الطفل مع طبيب",
                    _ => "النتيجة في المدى الطبيعي"
                };

                // ✅ تخزين النتيجة — fileName مش filePath
                var result = new AIResult
                {
                    ScreeningID = screening.ScreeningID,
                    VideoPath = fileName,   // ✅ اسم الملف بس
                    VideoUrl = videoUrl,    // ✅ الـ URL الكامل
                    RiskScorePercentage = aiResult.RiskScorePercentage,   // يبقى nullable في AIResult، تمام
                    OverallConfidence = aiResult.OverallConfidence,
                    AI_JSON_Data = aiResult.RawJson
                };

                screening.Status = "Completed";
                screening.RiskLevel = riskLevel;
                screening.TotalScore = scorePercentage;   // ✅ float عادي، مش nullable في Screening

                _context.AIResult.Add(result);
                await _context.SaveChangesAsync();

                // ✅ الـ JSON الكامل لـ Flutter
                var aiResultFull = System.Text.Json.JsonSerializer
                    .Deserialize<object>(aiResult.RawJson);

                return Ok(new
                {
                    screeningID = screening.ScreeningID,
                    childID = dto.ChildID,
                    videoUrl,
                    riskLevel,
                    totalScore = scorePercentage,
                    aiResult = aiResultFull,
                    message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}