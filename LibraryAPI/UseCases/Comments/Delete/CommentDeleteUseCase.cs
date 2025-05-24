using AutoMapper;
using LibraryAPI.DatabaseAccess.BooksRepository;
using LibraryAPI.DatabaseAccess.CommentsRepository;
using LibraryAPI.Services;
using LibraryAPI.Utils;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LibraryAPI.UseCases.Comments.Delete
{
    public class CommentDeleteUseCase : ICommentDeleteUseCase
    {
        private readonly ICommentsRepository _commentsRepository;
        private readonly ILogger<CommentDeleteUseCase> _logger;
        private readonly IBookRepository _bookRepository;
        private readonly IUsersService _usersServicies;

        public CommentDeleteUseCase(ICommentsRepository commentsRepository, ILogger<CommentDeleteUseCase> logger, IBookRepository bookRepository, IUsersService usersServicies)
        {
            _commentsRepository = commentsRepository;
            _logger = logger;
            _bookRepository = bookRepository;
            _usersServicies = usersServicies;
        }

        public async Task<Result> Run(Guid commentId, int bookId, ModelStateDictionary modelState)
        {
            var existsBook = await _bookRepository.Exists(bookId);
            if (!existsBook)
            {
                _logger.LogWarning($"Book with ID {bookId} was not found.");
                return Result.NotFound();
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
                _logger.LogWarning($"User with ID {user.Id} is not authorized to delete comment with ID {commentId}.");
                modelState.AddModelError(nameof(user.Id), $"User with ID {user.Id} is not authorized to delete comment with ID {commentId}.");
                return Result.Forbidden();
            }

            comment.IsDeleted = true;
            await _commentsRepository.Update(comment);
            return Result.Success();
        }
    }
}
