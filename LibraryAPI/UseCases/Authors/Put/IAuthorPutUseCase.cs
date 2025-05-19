using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Authors.Put
{
    public interface IAuthorPutUseCase
    {
        public Task<bool> Run(int authorId, AuthorCreationWithImageDTO authorCreationWithImageDTO);
    }
}
