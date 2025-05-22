using LibraryAPI.Data;
using LibraryAPI.Entities;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.DatabaseAccess.CommentsRepository
{
    public class SQLServerCommentRepository : ICommentsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IOutputCacheStore _outputCacheStore;

        public SQLServerCommentRepository(ApplicationDbContext context, IOutputCacheStore outputCacheStore)
        {
            _context = context;
            _outputCacheStore = outputCacheStore;
        }

        public async Task<List<Comment>> GetAllByBookId(int bookId)
        {
            var comments = await _context.Comments
               .Include(x => x.User)
               .Where(x => x.BookId == bookId)
               .OrderByDescending(x => x.PublicationDate)
               .ToListAsync();
            return comments;
        }
    }
}
