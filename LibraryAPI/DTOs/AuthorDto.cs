using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs
{
    public class AuthorDto
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public List<string> BookTitles { get; set; } = new List<string>();
    }
}
