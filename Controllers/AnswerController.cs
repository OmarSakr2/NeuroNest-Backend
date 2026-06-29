using AustimAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AustimAPI.Controllers
{
    [Authorize]
    [Route("api/answer")]
    [ApiController]
    public class AnswerController : ControllerBase
    {
        private readonly apiDBContext _context;

        public AnswerController(apiDBContext context)
        {
            _context = context;
        }

     
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            return Ok(await _context.QuestionnaireAnswer
                .Where(a => a.Screening.Child.ParentID == userId)
                .Select(a => new {
                    a.AnswerID,
                    a.ScreeningID,
                    a.QuestionID,
                    a.AnswerValue
                })
                .ToListAsync());
        }
    }
}
