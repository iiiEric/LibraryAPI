using LibraryAPI.DTOs;
using LibraryAPI.Utils;

namespace LibraryAPI.UseCases.Authors.Put
{
    public interface IAuthorPutUseCase
    {
        public Task<Result> Run(int authorId, AuthorCreationWithImageDTO authorCreationWithImageDTO);
    }
}
