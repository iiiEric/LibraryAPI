using LibraryAPI.Utils;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LibraryAPI.UseCases.Comments.Delete
{
    public interface ICommentDeleteUseCase
    {
        Task<Result> Run(Guid commentId, int bookId, ModelStateDictionary modelState);
    }
}
