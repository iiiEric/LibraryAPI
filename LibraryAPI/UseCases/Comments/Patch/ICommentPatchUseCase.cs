using LibraryAPI.DTOs;
using LibraryAPI.Utils;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LibraryAPI.UseCases.Comments.Patch
{
    public interface ICommentPatchUseCase
    {
        Task<Result> Run(Guid commentId, int bookId, JsonPatchDocument<CommentPatchDTO> patchDocument, ModelStateDictionary modelState);
    }
}
