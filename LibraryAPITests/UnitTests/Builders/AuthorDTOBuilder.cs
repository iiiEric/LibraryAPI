using LibraryAPI.DTOs;

namespace LibraryAPITests.UnitTests.Builders
{
    public class AuthorDTOBuilder
    {
        private int _id = 1;
        private string _fullName = "DefaultName";
        private string? _imageUrl;

        public AuthorDTOBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public AuthorDTOBuilder WithFullName(string fullName)
        {
            _fullName = fullName;
            return this;
        }

        public AuthorDTOBuilder WithImageUrl(string? imageUrl)
        {
            _imageUrl = imageUrl;
            return this;
        }

        public AuthorDTO Build()
        {
            return new AuthorDTO
            {
                Id = _id,
                FullName = _fullName!,
                ImageUrl = _imageUrl
            };
        }
    }
}
