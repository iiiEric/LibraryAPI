using LibraryAPI.Utils;

namespace LibraryAPI.UseCases.Books.Delete
{
    public interface IBookDeleteUseCase
    {
        Task<Result> Run(int bookId);
    }
}
