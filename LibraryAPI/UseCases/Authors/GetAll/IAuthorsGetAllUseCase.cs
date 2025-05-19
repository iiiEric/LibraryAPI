using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Authors.GetAll
{
    public interface IAuthorsGetAllUseCase
    {
        Task<IEnumerable<AuthorDTO>> Run(PaginationDTO paginationDTO);
    }
}
