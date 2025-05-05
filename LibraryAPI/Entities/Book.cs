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
        public List<AuthorBook> Authors { get; set; } = [];
        public List<Comment> Comments { get; set; } = [];
    }
}
