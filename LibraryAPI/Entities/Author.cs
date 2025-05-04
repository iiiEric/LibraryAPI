using LibraryAPI.Validation;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Entities
{
    public class Author
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        [FirstLetterCapital]
        public required string Name { get; set; }
        public List<Book> Books { get; set; } = new List<Book>();
    }
}
