using LibraryAPI.DTOs;

namespace LibraryAPITests.UnitTests.Builders
{
    public class AuthorCreationWithImageDTOBuilder
    {
        private string _name = "DefaultName";
        private string _surname1 = "DefaultSurname1";
        private string? _surname2 = null;
        private string? _identity = null;
        private List<BookCreationDTO> _books = new();
        private IFormFile? _image = null;

        public AuthorCreationWithImageDTOBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public AuthorCreationWithImageDTOBuilder WithSurname1(string surname1)
        {
            _surname1 = surname1;
            return this;
        }

        public AuthorCreationWithImageDTOBuilder WithSurname2(string? surname2)
        {
            _surname2 = surname2;
            return this;
        }

        public AuthorCreationWithImageDTOBuilder WithIdentity(string? identity)
        {
            _identity = identity;
            return this;
        }

        public AuthorCreationWithImageDTOBuilder WithBooks(List<BookCreationDTO> books)
        {
            _books = books;
            return this;
        }

        public AuthorCreationWithImageDTOBuilder AddBook(BookCreationDTO book)
        {
            _books.Add(book);
            return this;
        }

        public AuthorCreationWithImageDTOBuilder WithImage(IFormFile? image)
        {
            _image = image;
            return this;
        }

        public AuthorCreationWithImageDTO Build()
        {
            return new AuthorCreationWithImageDTO
            {
                Name = _name,
                Surname1 = _surname1,
                Surname2 = _surname2,
                Identity = _identity,
                Books = _books,
                Image = _image
            };
        }
    }
}
