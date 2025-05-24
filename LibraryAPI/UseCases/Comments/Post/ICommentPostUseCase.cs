using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Comments.Post
{
    public interface ICommentPostUseCase
    {
        Task<CommentDTO?> Run(int bookId, CommentCreationDTO commentCreationDTO);
    }
}
