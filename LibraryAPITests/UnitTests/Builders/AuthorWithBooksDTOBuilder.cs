using LibraryAPI.DTOs;

namespace LibraryAPITests.UnitTests.Builders
{
    public class AuthorWithBooksDTOBuilder
    {
        private int _id = 1;
        private string _fullName = "Default Name";
        private string? _imageUrl;
        private List<BookDTO> _books = [];

        public AuthorWithBooksDTOBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public AuthorWithBooksDTOBuilder WithFullName(string fullName)
        {
            _fullName = fullName;
            return this;
        }

        public AuthorWithBooksDTOBuilder WithImageUrl(string? imageUrl)
        {
            _imageUrl = imageUrl;
            return this;
        }

        public AuthorWithBooksDTOBuilder WithBooks(List<BookDTO>? books)
        {
            _books = books ?? new List<BookDTO>();
            return this;
        }

        public AuthorWithBooksDTO Build()
        {
            return new AuthorWithBooksDTO
            {
                Id = _id,
                FullName = _fullName,
                ImageUrl = _imageUrl,
                Books = _books
            };
        }
    }

}
