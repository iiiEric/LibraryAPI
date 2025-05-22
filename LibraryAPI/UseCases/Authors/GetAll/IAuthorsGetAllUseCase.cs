using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Authors.GetAll
{
    public interface IAuthorsGetAllUseCase
    {
        Task<IEnumerable<AuthorDTO>> Run(HttpContext httpContext, PaginationDTO paginationDTO);
    }
}
