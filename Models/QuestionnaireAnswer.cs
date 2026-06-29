using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AustimAPI.Models
{
    public class QuestionnaireAnswer
    {
        [Key]
        public int AnswerID { get; set; }
        public int ScreeningID { get; set; }
        public int QuestionID { get; set; }

        // ✅ مطلب 2 — تخزين قيم عشرية
        // 1.0 = نعم كامل
        // 0.66 = نعم أحياناً
        // 0.33 = نادراً
        // 0.0 = لا
        // null = لم يجب
        public float? AnswerValue { get; set; }

        [JsonIgnore]
        public Screening Screening { get; set; }
        [JsonIgnore]
        public Question Question { get; set; }
    }
}
