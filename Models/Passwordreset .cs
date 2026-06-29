using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AustimAPI.Models
{

    public class PasswordReset
    {
        [Key]
        public int Id { get; set; }

        // ربط بالمستخدم
        public int UserID { get; set; }

        // الكود العشوائي (6 أرقام مثلاً)
        [Required]
        public string Code { get; set; }

        // وقت إنشاء الكود
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // الكود بيتنتهي بعد 15 دقيقة
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(15);

        // هل اتستخدم الكود ده قبل كده؟
        public bool IsUsed { get; set; } = false;

        [ForeignKey("UserID")]
        [JsonIgnore]
        public User User { get; set; }
    }
}
