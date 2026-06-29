using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AustimAPI.Models
{
    public class Child
    {
        [Key]
        public int ChildID { get; set; }

        public int ParentID { get; set; }
        public string ChildName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }

        // for the AI model questions
        public bool HasJaundice { get; set; } = false;
        public bool FamilyASD { get; set; } = false;

        [ForeignKey("ParentID")]
        [JsonIgnore]
        public User? User { get; set; }

        [JsonIgnore]
        public ICollection<Screening> Screenings { get; set; } = new List<Screening>();
    }
}