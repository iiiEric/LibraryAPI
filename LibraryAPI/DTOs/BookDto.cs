using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs
{
    public class BookDTO
    {
        public int Id { get; set; }

        public required string Title { get; set; }
    }
}
