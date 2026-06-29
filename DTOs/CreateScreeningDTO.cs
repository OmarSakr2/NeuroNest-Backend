namespace AustimAPI.DTOs
{
    public class CreateScreeningDTO
    {
        public int ChildID { get; set; }
 
        // ✅ جديد: "Questionnaire" أو "Video"
        public string ScreeningType { get; set; } = "Questionnaire";
    }
}
