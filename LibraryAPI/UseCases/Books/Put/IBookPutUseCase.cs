using LibraryAPI.DTOs;
using LibraryAPI.Utils;

namespace LibraryAPI.UseCases.Books.Put
{
    public interface IBookPutUseCase
    {
        Task<Result> Run(int bookId, BookCreationDTO bookCreationDTO);
    }
}
