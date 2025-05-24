using AutoMapper;
using LibraryAPI.DatabaseAccess.AuthorsRepository;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using LibraryAPI.Utils;

namespace LibraryAPI.UseCases.Authors.Patch
{
    public class AuthorPatchUseCase : IAuthorPatchUseCase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorPatchUseCase> _logger;

        public AuthorPatchUseCase(IAuthorRepository authorRepository, IMapper mapper, ILogger<AuthorPatchUseCase> logger)
        {
            _authorRepository = authorRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result> Run(int authorId, JsonPatchDocument<AuthorPatchDTO> patchDocument, ModelStateDictionary modelState)
        {
            _logger.LogInformation("Received PATCH request for author with ID {AuthorId}.", authorId);

            if (patchDocument is null)
            {
                _logger.LogWarning("Patch document is null.");
                modelState.AddModelError(nameof(patchDocument), "Patch document is null.");
                return Result.BadRequest();
            }

            var author = await _authorRepository.GetById(authorId);
            if (author is null)
            {
                _logger.LogWarning($"Author with ID {authorId} was not found.");
                return Result.NotFound();
            }

            var dto = _mapper.Map<AuthorPatchDTO>(author);
            patchDocument.ApplyTo(dto, modelState);

            if (!modelState.IsValid)
            {
                _logger.LogWarning("Patch document failed validation.");
                return Result.ValidationError();
            }

            _mapper.Map(dto, author);
            await _authorRepository.Update(author);

            _logger.LogInformation($"Author with ID {author.Id} patched successfully.");
            return Result.Success();
        }
    }
}
