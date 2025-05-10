namespace LibraryAPI.DTOs
{
    public class AuthorFilterDTO
    {
        public int Page { get; set; } = 1;
        public int RecordsPerPage { get; set; } = 10;
        public PaginationDTO PaginationDTO
        {
            get
            {
                return new PaginationDTO(Page, RecordsPerPage);
            }
        }
        public string? Name { get; set; }
        public string? Surname1 { get; set; }
        public bool? HasImage { get; set; }
        public string? BookTitle { get; set; }
        public bool IncludeBooks { get; set; }
        public string? SortBy { get; set; }
        public bool SortAscending { get; set; } = true;
    }
}
