using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        [Required]
        public required string Body { get; set; }
        public DateTime PublicationDate { get; set; }
        public int BookId { get; set; }
        public Book? Book { get; set; }
    }
}
