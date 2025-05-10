namespace LibraryAPI.DTOs
{
    public class HashResultDTO
    {
        public required string Hash { get; set; }
        public required byte[] Salt { get; set; }
    }
}
