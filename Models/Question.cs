using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AustimAPI.Models
{
    public class Question
    {
        [Key]
        public int QuestionID { get; set; }
        public int QuestionNumber { get; set; }
        public string QuestionText_AR { get; set; }
        public string QuestionText_EN { get; set; }
        public bool RiskIfNo { get; set; }
        public bool IsActive { get; set; } = true;

        [JsonIgnore] public ICollection<QuestionnaireAnswer> Answers { get; set; } = new List<QuestionnaireAnswer>();
    }
}