using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs
{
    public class CommentDTO
    {
        public Guid Id { get; set; }
        public required string Body { get; set; }
        public DateTime PublicationDate { get; set; }
    }
}
