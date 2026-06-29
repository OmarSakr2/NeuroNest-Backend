using System.ComponentModel.DataAnnotations;

namespace AustimAPI.DTOs
{
    
    public class ForgotPasswordDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

   
    public class ResetPasswordDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Code { get; set; }   

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
    }

    
    public class UpdateProfileDTO
    {
        [Required]
        public string FullName { get; set; }
    }

    
    public class ChangePasswordDTO
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
    }


    //public class UpdateChildDTO
    //{
    //    [Required]
    //    public string ChildName { get; set; }

    //    [Required]
    //    public DateTime DateOfBirth { get; set; }

    //    [Required]
    //    public string Gender { get; set; }
    //}

   
    public class GoogleSignInDTO
    {
        [Required]
        public string IdToken { get; set; }   
    }
}