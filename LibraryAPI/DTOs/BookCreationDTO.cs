using LibraryAPI.Entities;
using LibraryAPI.Validation;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs
{
    public class BookCreationDTO
    {
        [Required]
        [StringLength(150)]
        [FirstLetterCapital]
        public required string Title { get; set; }
        public List<int> AuthorsIds { get; set; } = [];
    }
}
