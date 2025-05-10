using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs
{
    public class ClaimUpdateDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}
