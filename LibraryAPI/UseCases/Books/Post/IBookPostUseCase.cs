using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Books.Post
{
    public interface IBookPostUseCase
    {
        Task<BookDTO> Run(BookCreationDTO bookCreationDTO);
    }
}
