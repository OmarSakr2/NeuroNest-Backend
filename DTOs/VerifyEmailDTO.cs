using System.ComponentModel.DataAnnotations;
namespace AustimAPI.DTOs
{ 
public class VerifyEmailDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Code { get; set; }
}
}