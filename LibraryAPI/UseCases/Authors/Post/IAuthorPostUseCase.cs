using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Authors.Post
{
    public interface IAuthorPostUseCase
    {
        public Task<AuthorDTO> Run(AuthorCreationDTO authorCreationDTO);
    }
}
