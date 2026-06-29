using AustimAPI.DTOs;
using AustimAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AustimAPI.Controllers
{
    [Authorize]
    [Route("api/ai")]
    [ApiController]
    public class AIResultController : ControllerBase
    {
        private readonly apiDBContext _context;

        public AIResultController(apiDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            return Ok(await _context.AIResult
                .Where(r => r.Screening.Child.ParentID == userId)
                .Select(r => new
                {
                    r.ResultID,
                    r.ScreeningID,
                    r.VideoPath,
                    r.RiskScorePercentage,
                    r.OverallConfidence,
                    r.AI_JSON_Data
                })
                .ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAIResultDTO dto)
        {
            var screeningExists = await _context.Screening
                .AnyAsync(s => s.ScreeningID == dto.ScreeningID);

            if (!screeningExists)
                return NotFound("الجلسة مش موجودة");

            var result = new AIResult
            {
                ScreeningID = dto.ScreeningID,
                VideoPath = dto.VideoPath,
                RiskScorePercentage = dto.RiskScorePercentage,
                OverallConfidence = dto.OverallConfidence,
                AI_JSON_Data = dto.AI_JSON_Data
            };

            _context.AIResult.Add(result);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                result.ResultID,
                result.ScreeningID,
                result.RiskScorePercentage,
                result.OverallConfidence
            });
        }
    }
}