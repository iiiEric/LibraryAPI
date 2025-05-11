using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryAPITests.UnitTests.Builders
{
    public class AuthorCreationDTOBuilder
    {
        private string _name = "DefaultName";
        private string _surname1 = "DefaultSurname";
        private string? _surname2 = null;
        private string? _identity = null;
        private List<BookCreationDTO> _books = [];

        public AuthorCreationDTOBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public AuthorCreationDTOBuilder WithSurname1(string surname1)
        {
            _surname1 = surname1;
            return this;
        }

        public AuthorCreationDTOBuilder WithSurname2(string? surname2)
        {
            _surname2 = surname2;
            return this;
        }

        public AuthorCreationDTOBuilder WithIdentity(string? identity)
        {
            _identity = identity;
            return this;
        }

        public AuthorCreationDTOBuilder WithBooks(List<BookCreationDTO> books)
        {
            _books = books;
            return this;
        }


        public AuthorCreationDTO Build()
        {
            return new AuthorCreationDTO
            {
                Name = _name,
                Surname1 = _surname1,
                Surname2 = _surname2,
                Identity = _identity,
                Books = _books
            };
        }
    }
}
