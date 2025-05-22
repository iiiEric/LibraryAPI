using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Books.Put
{
    public interface IBookPutUseCase
    {
        public Task<bool> Run(int bookId, BookCreationDTO bookCreationDTO);
    }
}
