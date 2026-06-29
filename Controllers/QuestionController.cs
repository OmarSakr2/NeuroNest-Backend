using AustimAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AustimAPI.Controllers
{
    [Route("api/question")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly apiDBContext _context;

        public QuestionController(apiDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
            => Ok(await _context.Question.ToListAsync());
    }
}
