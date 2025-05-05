using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs
{
    public class CommentCreationDTO
    {
        [Required]
        public required string Body { get; set; }
    }
}
