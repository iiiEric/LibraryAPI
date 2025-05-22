using AutoMapper;
using LibraryAPI.DatabaseAccess.BooksRepository;
using LibraryAPI.DatabaseAccess.CommentsRepository;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.UseCases.Books.Delete;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.UseCases.Comments.GetByBookId
{
    public class CommentGetByBookIdUseCase : ICommentGetByBookIdUseCase
    {
        private readonly ICommentsRepository _commentsRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BookDeleteUseCase> _logger;
        private readonly IBookRepository _bookRepository;

        public CommentGetByBookIdUseCase(ICommentsRepository commentsRepository, IMapper mapper, ILogger<BookDeleteUseCase> logger, IBookRepository bookRepository)
        {
            _commentsRepository = commentsRepository;
            _mapper = mapper;
            _logger = logger;
            _bookRepository = bookRepository;
        }

        public async Task<List<CommentDTO>?> Run(int bookId)
        {
            var exists = await _bookRepository.Exists(bookId);
            if (!exists)
            {
                _logger.LogWarning($"Book with ID {bookId} was not found.");
                return null;
            }

            var comments = _commentsRepository.GetAllByBookId(bookId);
            var commentsDTO = _mapper.Map<List<CommentDTO>>(comments);
            return commentsDTO;
        }
    }
}
