using LibraryAPI.Constants;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Utils;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace LibraryAPI.DatabaseAccess.AuthorsRepository
{
    public class SQLServerAuthorRepository : IAuthorRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SQLServerAuthorRepository> _logger;
        private readonly IConfiguration _configuration;
        private readonly IOutputCacheStore _outputCacheStore;
        private const string AuthorsCacheKey = "CacheSettings:AuthorsCache";

        public SQLServerAuthorRepository(ApplicationDbContext context, ILogger<SQLServerAuthorRepository> logger, IConfiguration configuration, IOutputCacheStore outputCacheStore)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _outputCacheStore = outputCacheStore;
        }

        public async Task<bool> Add(Author author)
        {
            _context.Add(author);
            return await SaveAndEvictCacheAsync();
        }

        public async Task<bool> Exists(int authorId)
        {
            var exists = await _context.Authors.AnyAsync(x => x.Id == authorId);
            return exists;
        }

        public async Task<IEnumerable<Author>> GetAll(PaginationDTO paginationDTO)
        {
            var queryable = _context.Authors.AsQueryable();

            var authors = await queryable
                .OrderBy(x => x.Name)
                .Paginate(paginationDTO)
                .ToListAsync();

            return authors;
        }

        public async Task<IEnumerable<Author>> GetByCriteria(AuthorFilterDTO authorFilterDTO)
        {
            var queryable = _context.Authors.AsQueryable();

            if (!string.IsNullOrEmpty(authorFilterDTO.Name))
                queryable = queryable.Where(x => x.Name.Contains(authorFilterDTO.Name));

            if (!string.IsNullOrEmpty(authorFilterDTO.Surname1))
                queryable = queryable.Where(x => x.Name.Contains(authorFilterDTO.Surname1));

            if (authorFilterDTO.HasImage.HasValue)
            {
                queryable = authorFilterDTO.HasImage.Value
                    ? queryable.Where(x => x.ImageUrl != null)
                    : queryable.Where(x => x.ImageUrl == null);
            }

            if (!string.IsNullOrEmpty(authorFilterDTO.BookTitle))
                queryable = queryable.Where(x => x.Books.Any(y => y.Book!.Title.Contains(authorFilterDTO.BookTitle)));

            if (authorFilterDTO.IncludeBooks)
                queryable = queryable.Include(x => x.Books).ThenInclude(x => x.Book);

            if (!string.IsNullOrEmpty(authorFilterDTO.SortBy))
            {
                var orderType = authorFilterDTO.SortAscending ? "ascending" : "descending";
                try
                {
                    queryable = queryable.OrderBy($"{authorFilterDTO.SortBy} {orderType}");
                }
                catch (Exception ex)
                {
                    queryable = queryable.OrderBy(x => x.Name);
                    _logger.LogError(ex, "Error ordering authors by {OrderType} on field {SortBy}", orderType, authorFilterDTO.SortBy);
                }
            }
            else
            {
                queryable = queryable.OrderBy(x => x.Name);
            }

            var authors = await queryable.Paginate(authorFilterDTO.PaginationDTO).ToListAsync();
            return authors;
        }

        public async Task<Author?> GetById(int authorId)
        {
            var author = await _context.Authors
                 .Include(x => x.Books)
                 .ThenInclude(x => x.Book)
                 .FirstOrDefaultAsync(x => x.Id == authorId);

            return author;
        }

        public async Task<string?> GetImageUrl(int authorId)
        {
            var currentImage = await _context.Authors
                     .Where(x => x.Id == authorId)
                     .Select(x => x.ImageUrl)
                     .FirstOrDefaultAsync();
            return currentImage;
        }

        public async Task<bool> Update(Author author)
        {
            _context.Update(author);
            return await SaveAndEvictCacheAsync();
        }

        public async Task<bool> Remove(Author author)
        {
            _context.Remove(author);
            return await SaveAndEvictCacheAsync();
        }

        private async Task<bool> SaveAndEvictCacheAsync()
        {
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(CacheTags.Authors, default);
            return true;
        }
    }
}
