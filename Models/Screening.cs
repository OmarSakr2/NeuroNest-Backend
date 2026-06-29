using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AustimAPI.Models
{
    public class Screening
    {
        [Key]
        public int ScreeningID { get; set; }

        public int ChildID { get; set; }

        public DateTime ScreeningDate { get; set; } = DateTime.UtcNow;

        public float? TotalScore { get; set; }

        public string RiskLevel { get; set; } = "NotCalculated";

        public string Status { get; set; } = "Pending";

        public string ScreeningType { get; set; } = "Questions";

        [JsonIgnore]
        public Child Child { get; set; }

        [JsonIgnore]
        public ICollection<QuestionnaireAnswer> Answers { get; set; }
            = new List<QuestionnaireAnswer>();

        [JsonIgnore]
        public AIResult? AIResult { get; set; }
    }
}