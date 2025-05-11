using LibraryAPI.Entities;

namespace LibraryAPITests.UnitTests.Builders
{
    public class AuthorBuilder
    {
        private int _id = 0;
        private string _name = "DefaultName";
        private string _surname1 = "DefaultSurname";
        private string? _surname2 = null;
        private string? _identity = null;
        private string? _imageUrl = null;
        private List<AuthorBook> _books = [];

        public AuthorBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public AuthorBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public AuthorBuilder WithSurname1(string surname1)
        {
            _surname1 = surname1;
            return this;
        }

        public AuthorBuilder WithSurname2(string? surname2)
        {
            _surname2 = surname2;
            return this;
        }

        public AuthorBuilder WithIdentity(string? identity)
        {
            _identity = identity;
            return this;
        }

        public AuthorBuilder WithImageUrl(string? imageUrl)
        {
            _imageUrl = imageUrl;
            return this;
        }

        public AuthorBuilder WithBooks(List<AuthorBook> books)
        {
            _books = books;
            return this;
        }

        public Author Build()
        {
            return new Author
            {
                Id = _id,
                Name = _name,
                Surname1 = _surname1,
                Surname2 = _surname2,
                Identity = _identity,
                ImageUrl = _imageUrl,
                Books = _books
            };
        }
    }
}
