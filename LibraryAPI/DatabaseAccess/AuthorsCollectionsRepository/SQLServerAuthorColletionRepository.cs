using LibraryAPI.Data;
using LibraryAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.DatabaseAccess.AuthorsCollectionsRepository
{
    public class SQLServerAuthorColletionRepository : IAuthorCollectionsRepository
    {
        private readonly ApplicationDbContext _context;

        public SQLServerAuthorColletionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddRange(IEnumerable<Author> authors)
        {
            _context.AddRange(authors);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Author>?> GetAll(List<int> collectionIds)
        {
            var authors = await _context.Authors
               .Include(x => x.Books)
               .ThenInclude(x => x.Book)
               .Where(x => collectionIds.Contains(x.Id))
               .ToListAsync();
            return authors;
        }
    }
}
