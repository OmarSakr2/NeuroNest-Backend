using AustimAPI.DTOs;
using AustimAPI.Models;
using AustimAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AustimAPI.Controllers
{
    [Authorize]
    [Route("api/Screening")]
    [ApiController]
    public class ScreeningController : ControllerBase
    {
        private readonly apiDBContext _context;
        private readonly AIService _aiService;

        public ScreeningController(apiDBContext context, AIService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            return Ok(await _context.Screening
                .Where(s => s.Child.ParentID == userId)
                .Select(s => new
                {
                    s.ScreeningID,
                    s.ChildID,
                    ChildName = s.Child.ChildName,
                    s.ScreeningDate,
                    s.TotalScore,
                    s.RiskLevel,
                    s.Status,
                    s.ScreeningType
                })
                .OrderByDescending(s => s.ScreeningDate)
                .ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateScreeningDTO dto)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            var childExists = await _context.Child
                .AnyAsync(c => c.ChildID == dto.ChildID && c.ParentID == userId);

            if (!childExists)
                return Unauthorized(new { message = "الطفل مش موجود أو مش ليك" });

            var screening = new Screening
            {
                ChildID = dto.ChildID,
                ScreeningDate = DateTime.UtcNow,
                Status = "Pending",
                TotalScore = 0,
                RiskLevel = "NotCalculated",
                ScreeningType = dto.ScreeningType ?? "Questions"
            };

            _context.Screening.Add(screening);
            await _context.SaveChangesAsync();

            var nextStep = dto.ScreeningType == "Video"
                ? "POST /api/video/upload"
                : "POST /api/Screening/submit-answers";

            return Ok(new
            {
                screening.ScreeningID,
                screening.ChildID,
                screening.Status,
                screening.ScreeningType,
                nextStep
            });
        }

        [HttpPost("submit-answers")]
        public async Task<IActionResult> SubmitAnswers([FromBody] SubmitAnswersDTO dto)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            var screening = await _context.Screening
                .Include(s => s.Child)
                .FirstOrDefaultAsync(s =>
                    s.ScreeningID == dto.ScreeningId &&
                    s.Child.ParentID == userId);

            if (screening == null)
                return Unauthorized(new { message = "الجلسة مش موجودة أو مش ليك" });

            if (dto.Answers == null || dto.Answers.Count != 20)
                return BadRequest(new
                {
                    message = $"لازم تبعت 20 إجابة - تم إرسال {dto.Answers?.Count ?? 0}"
                });

            var child = await _context.Child
                .FirstOrDefaultAsync(c => c.ChildID == dto.ChildId && c.ParentID == userId);

            if (child == null)
                return NotFound(new { message = "الطفل مش موجود" });

            int ageMonths = (int)((DateTime.UtcNow - child.DateOfBirth).TotalDays / 30.44);
            int sex = child.Gender?.ToLower() == "male" ? 1 : 0;

            var oldAnswers = await _context.QuestionnaireAnswer
                .Where(a => a.ScreeningID == dto.ScreeningId)
                .ToListAsync();

            if (oldAnswers.Any())
                _context.QuestionnaireAnswer.RemoveRange(oldAnswers);

            var questions = await _context.Question
                .OrderBy(q => q.QuestionNumber)
                .ToListAsync();

            var answersToSave = new List<QuestionnaireAnswer>();
            for (int i = 0; i < dto.Answers.Count && i < questions.Count; i++)
            {
                answersToSave.Add(new QuestionnaireAnswer
                {
                    ScreeningID = dto.ScreeningId,
                    QuestionID = questions[i].QuestionID,
                    AnswerValue = dto.Answers[i]
                });
            }

            _context.QuestionnaireAnswer.AddRange(answersToSave);
            await _context.SaveChangesAsync();

            var pythonPayload = new
            {
                answers = dto.Answers,
                Age = ageMonths,
                Sex = sex,
                Jaundice = child.HasJaundice ? 1 : 0,
                Family_ASD = child.FamilyASD ? 1 : 0
            };

            var aiResult = await _aiService.AnalyzeQuestions(pythonPayload);

            string riskLevel;
            float riskScorePercentage;
            object? aiResultFull = null;

            if (aiResult != null)
            {
                riskLevel = aiResult.RiskLevel;
                riskScorePercentage = aiResult.RiskScorePercentage;

                aiResultFull = System.Text.Json.JsonSerializer
                    .Deserialize<object>(aiResult.RawJson);

                var existingAiResult = await _context.AIResult
                    .FirstOrDefaultAsync(r => r.ScreeningID == screening.ScreeningID);

                if (existingAiResult != null)
                {
                    existingAiResult.AI_JSON_Data = aiResult.RawJson;
                }
                else
                {
                    _context.AIResult.Add(new AIResult
                    {
                        ScreeningID = screening.ScreeningID,
                        AI_JSON_Data = aiResult.RawJson
                    });
                }
            }
            else
            {
                int count = dto.Answers.Count(a => a.HasValue && a.Value >= 0.5f);
                riskLevel = count >= 8 ? "High" : count >= 3 ? "Moderate" : "Low";
                riskScorePercentage = count;
            }

            screening.TotalScore = riskScorePercentage;
            screening.RiskLevel = riskLevel;
            screening.Status = "Completed";
           

            await _context.SaveChangesAsync();

            return Ok(new
            {
                screeningID = dto.ScreeningId,
                childID = dto.ChildId,
                childAgeMonths = ageMonths,
                riskLevel,
                totalScore = riskScorePercentage,
                aiResult = aiResultFull,
                message = riskLevel.Contains("High")
                    ? "ننصح بزيارة متخصص فوراً"
                    : riskLevel.Contains("Moderate")
                    ? "يستحسن متابعة الطفل مع طبيب"
                    : "النتيجة في المدى الطبيعي"
            });
        }

        [HttpGet("child/{childId}/history")]
        public async Task<IActionResult> GetChildHistory(int childId)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            var child = await _context.Child
                .Where(c => c.ChildID == childId && c.ParentID == userId)
                .Select(c => new { c.ChildID, c.ChildName })
                .FirstOrDefaultAsync();

            if (child == null)
                return NotFound(new { message = "الطفل مش موجود أو مش ليك" });

            var history = await _context.Screening
                .Where(s => s.ChildID == childId && s.Status == "Completed")
                .Select(s => new
                {
                    s.ScreeningID,
                    s.ScreeningDate,
                    s.TotalScore,
                    s.RiskLevel,
                    s.Status,
                    s.ScreeningType
                })
                .OrderByDescending(s => s.ScreeningDate)
                .ToListAsync();

            var lastScreening = history.FirstOrDefault();

            return Ok(new
            {
                child = new
                {
                    child.ChildID,
                    child.ChildName
                },
                stats = new
                {
                    total = history.Count,
                    lastAssessmentDate = lastScreening?.ScreeningDate,
                    currentRiskLevel = lastScreening?.RiskLevel ?? "N/A"
                },
                history
            });
        }
    }
}
