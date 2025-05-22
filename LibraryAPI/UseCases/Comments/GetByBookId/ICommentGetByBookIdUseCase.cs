using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Comments.GetByBookId
{
    public interface ICommentGetByBookIdUseCase
    {
        Task<List<CommentDTO>?> Run(int bookId);
    }
}
