using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AustimAPI.DTOs
{
    public class UploadVideoDTO
    {
        [Required]
        public int ChildID { get; set; }

        [Required]
        public IFormFile Video { get; set; }
    }
}
