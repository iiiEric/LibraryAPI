using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs.Users
{
    public class ClaimUpdateDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}
