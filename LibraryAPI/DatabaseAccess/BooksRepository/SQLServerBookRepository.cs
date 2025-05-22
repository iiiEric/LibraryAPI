using LibraryAPI.Constants;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Utils;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.DatabaseAccess.BooksRepository
{
    public class SQLServerBookRepository: IBookRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IOutputCacheStore _outputCacheStore;

        public SQLServerBookRepository(ApplicationDbContext context, IOutputCacheStore outputCacheStore)
        {
            _context = context;
            _outputCacheStore = outputCacheStore;
        }

        public async Task<bool> Add(Book book)
        {
            _context.Add(book);
            return await SaveAndEvictCacheAsync();
        }

        public async Task<bool> Exists(int bookId)
        {
            var exists = await _context.Books.AnyAsync(x => x.Id == bookId);
            return exists;
        }

        public async Task<IEnumerable<Book>> GetAll(HttpContext httpContext, PaginationDTO paginationDTO)
        {
            var queryable = _context.Books.AsQueryable();
            await httpContext.InsertHeaderPaginationParameters(queryable);

            var books = await queryable
                .OrderBy(x => x.Title)
                .Paginate(paginationDTO)
                .ToListAsync();

            return books;
        }

        public async Task<Book?> GetById(int bookId)
        {
            var book = await _context.Books
                .Include(x => x.Authors)
                .ThenInclude(x => x.Author)
                .FirstOrDefaultAsync(x => x.Id == bookId);

            return book;
        }

        public async Task<bool> Update(Book book)
        {
            _context.Update(book);
            return await SaveAndEvictCacheAsync();
        }

        public async Task<bool> Remove(Book book)
        {
            _context.Remove(book);
            return await SaveAndEvictCacheAsync();
        }

        private async Task<bool> SaveAndEvictCacheAsync()
        {
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(CacheTags.Books, default);
            return true;
        }
    }
}
