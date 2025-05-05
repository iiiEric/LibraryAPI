using LibraryAPI.Entities;

namespace LibraryAPI.DTOs
{
    public class BookWithAuthorsDTO: BookDTO
    {
        public List<AuthorDTO> Authors { get; set; } = [];
    }
}
