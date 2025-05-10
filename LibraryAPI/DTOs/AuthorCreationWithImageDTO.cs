namespace LibraryAPI.DTOs
{
    public class AuthorCreationWithImageDTO: AuthorCreationDTO
    {
        public IFormFile? Image { get; set; }
    }
}
