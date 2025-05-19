using AutoMapper;
using LibraryAPI.DatabaseAccess.AuthorsRepository;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

        public async Task<bool?> Run(int authorId, JsonPatchDocument<AuthorPatchDTO> patchDocument, ModelStateDictionary modelState)
        {
            _logger.LogInformation("Received PATCH request for author with ID {AuthorId}.", authorId);

            if (patchDocument is null)
            {
                _logger.LogWarning("Patch document is null.");
                modelState.AddModelError(nameof(patchDocument), "Patch document is null.");
                return null;
            }

            var author = await _authorRepository.GetById(authorId);
            if (author is null)
            {
                _logger.LogWarning($"Author with ID {authorId} was not found.");
                return false;
            }

            var authorPatchDTO = _mapper.Map<AuthorPatchDTO>(author);
            patchDocument.ApplyTo(authorPatchDTO, modelState);

            if (!modelState.IsValid)
            {
                _logger.LogWarning("Validation failed.");
                return null;
            }

            _mapper.Map(authorPatchDTO, author);
            await _authorRepository.Update(author);

            _logger.LogInformation($"Author with ID {author.Id} updated successfully.");
            return true;
        }
    }
}
