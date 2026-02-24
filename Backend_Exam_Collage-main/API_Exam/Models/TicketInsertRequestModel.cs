using System.ComponentModel.DataAnnotations;

namespace API_Exam.Models
{
    public class TicketInsertRequestModel
    {
        [Required]
        [MinLength(5, ErrorMessage = "Title must be at least 5 characters long.")]
        public string Title { get; set; }

        [Required]
        [MinLength(10, ErrorMessage = "Description must be at least 10 characters long.")]
        public string Description { get; set; }

        [Required]
        public string Priority { get; set; }
    }

    public class CommentRequestModel
    {
        [Required]
        [MinLength(1, ErrorMessage = "Comment cannot be empty.")]
        public string Comment { get; set; } = null!;
    }
}
