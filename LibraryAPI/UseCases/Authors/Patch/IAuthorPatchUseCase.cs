using LibraryAPI.DTOs;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LibraryAPI.UseCases.Authors.Patch
{
    public interface IAuthorPatchUseCase
    {
        public Task<bool?> Run(int authorId, JsonPatchDocument<AuthorPatchDTO> patchDocument, ModelStateDictionary modelState);
    }
}
