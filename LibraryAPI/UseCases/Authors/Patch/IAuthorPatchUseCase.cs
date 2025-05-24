using LibraryAPI.DTOs;
using LibraryAPI.Utils;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LibraryAPI.UseCases.Authors.Patch
{
    public interface IAuthorPatchUseCase
    {
        public Task<Result> Run(int authorId, JsonPatchDocument<AuthorPatchDTO> patchDocument, ModelStateDictionary modelState);
    }
}
