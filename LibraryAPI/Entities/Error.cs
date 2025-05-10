namespace LibraryAPI.Entities
{
    public class Error
    {
        public Guid Id { get; set; }
        public required string ErrorMessage { get; set; }
        public string? StrackTrace { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
