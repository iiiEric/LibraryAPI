using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Comments.GetById
{
    public interface ICommentGetByIdUseCase
    {
        Task<CommentDTO?> Run(Guid commentId);
    }
}
