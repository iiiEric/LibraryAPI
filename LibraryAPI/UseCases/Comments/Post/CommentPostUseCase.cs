using AutoMapper;
using LibraryAPI.DatabaseAccess.BooksRepository;
using LibraryAPI.DatabaseAccess.CommentsRepository;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Services;

namespace LibraryAPI.UseCases.Comments.Post
{
    public class CommentPostUseCase : ICommentPostUseCase
    {
        private readonly ICommentsRepository _commentsRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CommentPostUseCase> _logger;
        private readonly IBookRepository _bookRepository;
        private readonly IUsersService _usersServicies;

        public CommentPostUseCase(ICommentsRepository commentsRepository, IMapper mapper, ILogger<CommentPostUseCase> logger, IBookRepository bookRepository, IUsersService usersServicies)
        {
            _commentsRepository = commentsRepository;
            _mapper = mapper;
            _logger = logger;
            _bookRepository = bookRepository;
            _usersServicies = usersServicies;
        }

        public async Task<CommentDTO?> Run(int bookId, CommentCreationDTO commentCreationDTO)
        {
            var existsBook = await _bookRepository.Exists(bookId);
            if (!existsBook)
            {
                _logger.LogWarning($"Book with ID {bookId} was not found.");
                return null;
            }

            var user = await _usersServicies.GetCurrentUser();
            var comment = _mapper.Map<Comment>(commentCreationDTO);
            comment.BookId = bookId;
            comment.PublicationDate = DateTime.UtcNow;
            comment.UserId = user!.Id;

            await _commentsRepository.Add(comment);
            var commentDTO = _mapper.Map<CommentDTO>(comment);
            return commentDTO;
        }
    }
}
