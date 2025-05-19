using LibraryAPI.DTOs;

namespace LibraryAPITests.UnitTests.Builders
{
    public class AuthorFilterDTOBuilder
    {
        private int _page = 1;
        private int _recordsPerPage = 10;
        private string? _name;
        private string? _surname1;
        private bool? _hasImage;
        private string? _bookTitle;
        private bool _includeBooks = false;
        private string? _sortBy;
        private bool _sortAscending = true;

        public AuthorFilterDTOBuilder WithPage(int page)
        {
            _page = page;
            return this;
        }

        public AuthorFilterDTOBuilder WithRecordsPerPage(int records)
        {
            _recordsPerPage = records;
            return this;
        }

        public AuthorFilterDTOBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public AuthorFilterDTOBuilder WithSurname1(string surname)
        {
            _surname1 = surname;
            return this;
        }

        public AuthorFilterDTOBuilder WithHasImage(bool? hasImage)
        {
            _hasImage = hasImage;
            return this;
        }

        public AuthorFilterDTOBuilder WithBookTitle(string title)
        {
            _bookTitle = title;
            return this;
        }

        public AuthorFilterDTOBuilder IncludeBooks(bool include = true)
        {
            _includeBooks = include;
            return this;
        }

        public AuthorFilterDTOBuilder SortBy(string sortBy, bool ascending = true)
        {
            _sortBy = sortBy;
            _sortAscending = ascending;
            return this;
        }

        public AuthorFilterDTO Build()
        {
            return new AuthorFilterDTO
            {
                Page = _page,
                RecordsPerPage = _recordsPerPage,
                Name = _name,
                Surname1 = _surname1,
                HasImage = _hasImage,
                BookTitle = _bookTitle,
                IncludeBooks = _includeBooks,
                SortBy = _sortBy,
                SortAscending = _sortAscending
            };
        }
    }
}
