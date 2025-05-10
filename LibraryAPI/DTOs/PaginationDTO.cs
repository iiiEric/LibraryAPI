namespace LibraryAPI.DTOs
{
    public record PaginationDTO(int Page = 1, int RecordsPerPage = 10)
    {
        private const int MaxRecordsPerPage = 50;

        //init instead of set => must not be modified once initialised
        public int Page { get; init; } = Math.Max(1, Page); //avoid pages less than 0
        public int RecordsPerPage { get; init; } = Math.Clamp(RecordsPerPage, 1, MaxRecordsPerPage); //avoid records per page less than 0 and more than MaxRecordsPerPage
    }
}
