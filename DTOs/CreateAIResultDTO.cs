using System.ComponentModel.DataAnnotations;

namespace AustimAPI.DTOs
{
    public class CreateAIResultDTO
    {
        public int ScreeningID { get; set; }
        public string VideoPath { get; set; }
        public string? VideoUrl { get; set; }

        public float? RiskScorePercentage { get; set; }
        public float? OverallConfidence { get; set; }
        public string AI_JSON_Data { get; set; }
    }
}
