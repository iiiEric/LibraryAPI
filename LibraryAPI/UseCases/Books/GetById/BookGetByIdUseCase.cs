using AutoMapper;
using LibraryAPI.DatabaseAccess.BooksRepository;
using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Books.GetById
{
    public class BookGetByIdUseCase : IBookGetByIdUseCase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BookGetByIdUseCase> _logger;

        public BookGetByIdUseCase(IBookRepository bookRepository, IMapper mapper, ILogger<BookGetByIdUseCase> logger)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BookWithAuthorsDTO?> Run(int bookId)
        {
            _logger.LogInformation("Retrieving book with ID {BookId}", bookId);
            var book = await _bookRepository.GetById(bookId);

            if (book is null)
            {
                _logger.LogWarning($"Book with ID {bookId} was not found.");
                return null;
            }
               
            var bookWithAuthorsDTO = _mapper.Map<BookWithAuthorsDTO>(book);
            return bookWithAuthorsDTO;
        }
    }
}
