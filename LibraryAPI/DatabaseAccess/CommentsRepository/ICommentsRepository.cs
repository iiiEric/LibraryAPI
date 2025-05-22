using LibraryAPI.Entities;

namespace LibraryAPI.DatabaseAccess.CommentsRepository
{
    public interface ICommentsRepository
    {
        Task<List<Comment>> GetAllByBookId(int bookId);
    }
}
