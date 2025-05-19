using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Authors.PostWithImage
{
    public interface IAuthorPostWithImageUseCase
    {
        public Task<AuthorDTO> Run(AuthorCreationWithImageDTO authorCreationWithImageDTO);
    }
}
