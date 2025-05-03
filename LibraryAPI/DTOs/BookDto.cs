using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }

        public required string Title { get; set; }

        public string? AuthorName { get; set; }
    }
}
