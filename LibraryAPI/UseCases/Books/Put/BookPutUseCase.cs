using AutoMapper;
using LibraryAPI.Constants;
using LibraryAPI.DatabaseAccess.BooksRepository;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;

namespace LibraryAPI.UseCases.Books.Put
{
    public class BookPutUseCase : IBookPutUseCase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BookPutUseCase> _logger;

        public BookPutUseCase(IBookRepository bookRepository, IMapper mapper, ILogger<BookPutUseCase> logger)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<bool> Run(int bookId, BookCreationDTO bookCreationDTO)
        {
            _logger.LogInformation("Updating book with ID '{ID}'", bookId);

            var exists = await _bookRepository.Exists(bookId);
            if (!exists)
            {
                _logger.LogWarning($"Book with ID {bookId} was not found.");
                return false;
            }

            var book = _mapper.Map<Book>(bookCreationDTO);
            book.Id = bookId;
            assignAuthorsOrder(book);

            await _bookRepository.Update(book);
            _logger.LogInformation($"Book with ID {book.Id} updated successfully.");
            return true;
        }

        private void assignAuthorsOrder(Book book)
        {
            if (book.Authors is not null)
            {
                for (int i = 0; i < book.Authors.Count; i++)
                {
                    book.Authors[i].Order = i;
                }
            }
        }
    }
}
