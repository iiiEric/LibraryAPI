using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Books.GetAll
{
    public interface IBooksGetAllUseCase
    {
        Task<IEnumerable<BookDTO>> Run(HttpContext httpContext, PaginationDTO paginationDTO);
    }
}
