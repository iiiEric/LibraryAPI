using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs
{
    public class CommentDTO
    {
        public Guid Id { get; set; }
        public required string Body { get; set; }
        public DateTime PublicationDate { get; set; }
        public required string UserId { get; set; }
        public required string UserEmail { get; set; }
    }
}
