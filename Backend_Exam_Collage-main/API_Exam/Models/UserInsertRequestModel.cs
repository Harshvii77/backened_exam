using System.ComponentModel.DataAnnotations;

namespace API_Exam.Models
{
    public class UserInsertRequestModel
    {
        [Required]
        public string name { get; set; }

        [Required]
        [EmailAddress]
        public string email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string password { get; set; }

        [Required]
        public string role { get; set; }
    }
}
