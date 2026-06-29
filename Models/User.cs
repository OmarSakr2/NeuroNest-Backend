using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AustimAPI.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

   
        public bool IsVerified { get; set; } = false;

        [JsonIgnore]
        public ICollection<Child> Children { get; set; } = new List<Child>();
    }
}