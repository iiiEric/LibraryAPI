using LibraryAPI.Constants;
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

        public async Task<bool> Add(Comment comment)
        {
            _context.Add(comment);
            return await SaveAndEvictCacheAsync();
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

        public async Task<Comment?> GetById(Guid commentId)
        {
            var comment = await _context.Comments
               .Include(x => x.User)
               .FirstOrDefaultAsync(x => x.Id == commentId);
            return comment;
        }

        public async Task<bool> Update(Comment comment)
        {
            _context.Update(comment);
            return await SaveAndEvictCacheAsync();
        }

        private async Task<bool> SaveAndEvictCacheAsync()
        {
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(CacheTags.Comments, default);
            return true;
        }
    }
}
