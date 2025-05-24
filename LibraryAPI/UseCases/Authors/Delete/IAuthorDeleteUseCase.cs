using LibraryAPI.Utils;

namespace LibraryAPI.UseCases.Authors.Delete
{
    public interface IAuthorDeleteUseCase
    {
        public Task<Result> Run(int authorId);
    }
}
