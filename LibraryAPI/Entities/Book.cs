using LibraryAPI.Validation;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Entities
{
    public class Book
    {
        public int Id { get; set; }
        [Required]
        [StringLength(150)]
        [FirstLetterCapital]
        public required string Title { get; set; }
        public int AuthorId { get; set; }
        public Author? Author { get; set; }
        public List<Comment> Comments { get; set; } = [];
    }
}
