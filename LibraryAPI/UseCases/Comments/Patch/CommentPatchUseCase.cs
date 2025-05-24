using AutoMapper;
using LibraryAPI.DatabaseAccess.BooksRepository;
using LibraryAPI.DatabaseAccess.CommentsRepository;
using LibraryAPI.DTOs;
using LibraryAPI.Services;
using LibraryAPI.Utils;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LibraryAPI.UseCases.Comments.Patch
{
    public class CommentPatchUseCase : ICommentPatchUseCase
    {
        private readonly ICommentsRepository _commentsRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CommentPatchUseCase> _logger;
        private readonly IBookRepository _bookRepository;
        private readonly IUsersService _usersServicies;

        public CommentPatchUseCase(ICommentsRepository commentsRepository, IMapper mapper, ILogger<CommentPatchUseCase> logger, IBookRepository bookRepository, IUsersService usersServicies)
        {
            _commentsRepository = commentsRepository;
            _mapper = mapper;
            _logger = logger;
            _bookRepository = bookRepository;
            _usersServicies = usersServicies;
        }

        public async Task<Result> Run(Guid commentId, int bookId, JsonPatchDocument<CommentPatchDTO> patchDocument, ModelStateDictionary modelState)
        {
            var existsBook = await _bookRepository.Exists(bookId);
            if (!existsBook)
            {
                _logger.LogWarning($"Book with ID {bookId} was not found.");
                return Result.NotFound();
            }

            if (patchDocument is null)
            {
                _logger.LogWarning("Patch document is null.");
                modelState.AddModelError(nameof(patchDocument), "Patch document is null.");
                return Result.BadRequest();
            }

            var comment = await _commentsRepository.GetById(commentId);
            if (comment is null)
            {
                _logger.LogWarning($"Comment with ID {commentId} was not found.");
                return Result.NotFound();
            }

            var user = await _usersServicies.GetCurrentUser();

            if (user!.Id != comment.UserId)
            {
                _logger.LogWarning($"User with ID {user.Id} is not authorized to modify comment with ID {commentId}.");
                modelState.AddModelError(nameof(user.Id), $"User with ID {user.Id} is not authorized to modify comment with ID {commentId}.");
                return Result.Forbidden();
            }

            var commentPatchDTO = _mapper.Map<CommentPatchDTO>(comment);
            patchDocument.ApplyTo(commentPatchDTO, modelState);

            if (!modelState.IsValid)
            {
                _logger.LogWarning("Validation failed.");
                return Result.ValidationError();
            }

            _mapper.Map(commentPatchDTO, comment);
            await _commentsRepository.Update(comment);
            return Result.Success();
        }
    }
}
