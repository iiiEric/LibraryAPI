using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Books.Post
{
    public interface IBookPostUseCase
    {
        public Task<BookDTO> Run(BookCreationDTO bookCreationDTO);
    }
}
