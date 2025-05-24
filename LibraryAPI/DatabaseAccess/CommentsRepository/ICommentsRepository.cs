using LibraryAPI.Entities;

namespace LibraryAPI.DatabaseAccess.CommentsRepository
{
    public interface ICommentsRepository
    {
        Task<bool> Add(Comment comment);

        Task<List<Comment>> GetAllByBookId(int bookId);

        Task<Comment?> GetById(Guid commentId);

        Task<bool> Update(Comment commentk);
    }
}
