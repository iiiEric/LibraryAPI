using LibraryAPI.Entities;
using LibraryAPI.Validation;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs
{
    public class AuthorCreationDTO
    {
        [Required]
        [StringLength(30)]
        [FirstLetterCapital]
        public required string Name { get; set; }

        [Required]
        [StringLength(30)]
        [FirstLetterCapital]
        public required string Surname1 { get; set; }

        [StringLength(30)]
        [FirstLetterCapital]
        public string? Surname2 { get; set; }

        [StringLength(20)]
        public string? Identity { get; set; }

        public List<BookCreationDTO> Books { get; set; } = [];

    }
}
