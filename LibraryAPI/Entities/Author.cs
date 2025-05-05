using LibraryAPI.Validation;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Entities
{
    public class Author
    {
        public int Id { get; set; }

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

        public List<AuthorBook> Books { get; set; } = [];
    }
}
