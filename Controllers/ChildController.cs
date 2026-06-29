using AustimAPI.DTOs;
using AustimAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AustimAPI.Controllers
{
    [Authorize]
    [Route("api/child")]
    [ApiController]
    public class ChildController : ControllerBase
    {
        private readonly apiDBContext _context;
        public ChildController(apiDBContext context) { _context = context; }

        // GET /api/child — كل أطفال المستخدم
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);
            return Ok(await _context.Child
                .Where(c => c.ParentID == userId)
                .Select(c => new
                {
                    c.ChildID,
                    c.ChildName,
                    c.DateOfBirth,
                    c.Gender,
                    c.HasJaundice,
                    c.FamilyASD,
                    c.ParentID
                })
                .ToListAsync());
        }

        // ✅ مطلب 9 — GET /api/child/{id}/name — جيب اسم الطفل بس
        [HttpGet("{id}/name")]
        public async Task<IActionResult> GetChildName(int id)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);
            var child = await _context.Child
                .Where(c => c.ChildID == id && c.ParentID == userId)
                .Select(c => new { c.ChildID, c.ChildName })
                .FirstOrDefaultAsync();

            if (child == null)
                return NotFound(new { message = "الطفل مش موجود أو مش ليك" });

            return Ok(child);
        }

        // POST /api/child — إضافة طفل
        [HttpPost]
        public async Task<IActionResult> AddChild([FromBody] AddChildDTO dto)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);
            var child = new Child
            {
                ChildName = dto.ChildName,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                HasJaundice = dto.HasJaundice,
                FamilyASD = dto.FamilyASD,
                ParentID = userId
            };
            _context.Child.Add(child);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                child.ChildID,
                child.ChildName,
                child.DateOfBirth,
                child.Gender,
                child.HasJaundice,
                child.FamilyASD,
                child.ParentID
            });
        }

        // PUT /api/child/{id} — تعديل بيانات طفل
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChild(int id, [FromBody] UpdateChildDTO dto)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);
            var child = await _context.Child
                .FirstOrDefaultAsync(c => c.ChildID == id && c.ParentID == userId);
            if (child == null)
                return NotFound(new { message = "الطفل مش موجود أو مش ليك" });

            child.ChildName = dto.ChildName;
            child.DateOfBirth = dto.DateOfBirth;
            child.Gender = dto.Gender;
            child.HasJaundice = dto.HasJaundice;
            child.FamilyASD = dto.FamilyASD;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم التحديث",
                child.ChildID,
                child.ChildName,
                child.DateOfBirth,
                child.Gender,
                child.HasJaundice,
                child.FamilyASD
            });
        }

        // DELETE /api/child/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChild(int id)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);
            var child = await _context.Child
                .FirstOrDefaultAsync(c => c.ChildID == id && c.ParentID == userId);
            if (child == null)
                return NotFound(new { message = "الطفل مش موجود أو مش ليك" });

            _context.Child.Remove(child);
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم حذف بيانات الطفل" });
        }
    }
}