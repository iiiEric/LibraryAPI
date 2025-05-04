namespace LibraryAPI.DTOs
{
    public class BookWithAuthorDTO: BookDTO
    {
        public int AuthorId { get; set; }

        public string? AuthorName { get; set; }
    }
}
