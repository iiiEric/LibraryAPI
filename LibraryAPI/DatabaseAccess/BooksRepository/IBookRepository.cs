using LibraryAPI.DTOs;
using LibraryAPI.Entities;

namespace LibraryAPI.DatabaseAccess.BooksRepository
{
    public interface IBookRepository
    {
        Task<bool> Add(Book book);

        Task<bool> Exists(int bookId);

        Task<IEnumerable<Book>> GetAll(HttpContext httpContext, PaginationDTO paginationDTO);

        Task<Book?> GetById(int BookId);

        Task<bool> Update(Book Book);

        Task<bool> Remove(Book Book);
    }
}
