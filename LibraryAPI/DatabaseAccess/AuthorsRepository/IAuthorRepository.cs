using LibraryAPI.DTOs;
using LibraryAPI.Entities;

namespace LibraryAPI.DatabaseAccess.AuthorsRepository
{
    public interface IAuthorRepository
    {
        Task<bool> Add(Author author);

        Task<bool> Exists(int authorId);

        Task<IEnumerable<Author>> GetAll(HttpContext httpContext, PaginationDTO paginationDTO);

        Task<IEnumerable<Author>> GetByCriteria(AuthorFilterDTO authorFilterDTO);

        Task<Author?> GetById(int authorId);

        Task<string?> GetImageUrl(int authorId);

        Task<bool> Update(Author author);

        Task<bool> Remove(Author author);
    }
}
