using System.ComponentModel.DataAnnotations;

namespace AustimAPI.DTOs
{
    public class SubmitAnswersDTO
    {
        [Required]
        public int ScreeningId { get; set; }

        [Required]
        public int ChildId { get; set; }

        [Required]
        public List<float?> Answers { get; set; } = new();
    }
}