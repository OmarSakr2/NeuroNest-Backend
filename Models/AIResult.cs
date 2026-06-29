using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AustimAPI.Models
{
    public class AIResult
    {
        [Key]
        public int ResultID { get; set; }
        public int ScreeningID { get; set; }

        //  مطلب 7 — المسار الفعلي على السيرفر
        public string? VideoPath { get; set; }

        // ـ URL اللي Flutter بتستخدمه
        public string? VideoUrl { get; set; }

        public float? RiskScorePercentage { get; set; }
        public float? OverallConfidence { get; set; }
        public string? AI_JSON_Data { get; set; }

        [JsonIgnore]
        public Screening Screening { get; set; }
    }
}
