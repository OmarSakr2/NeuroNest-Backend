using System.ComponentModel.DataAnnotations;

namespace AustimAPI.DTOs
{
    public class AddChildDTO
    {
        [Required]
        public string ChildName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string Gender { get; set; }

        // ✅ جديد
        public bool HasJaundice { get; set; } = false;
        public bool FamilyASD { get; set; } = false;
    }
}