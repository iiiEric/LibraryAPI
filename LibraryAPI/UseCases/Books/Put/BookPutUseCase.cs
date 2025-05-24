using AutoMapper;
using LibraryAPI.Constants;
using LibraryAPI.DatabaseAccess.BooksRepository;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Utils;

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

        public async Task<Result> Run(int bookId, BookCreationDTO bookCreationDTO)
        {
            _logger.LogInformation("Updating book with ID '{ID}'", bookId);

            var exists = await _bookRepository.Exists(bookId);
            if (!exists)
            {
                _logger.LogWarning($"Book with ID {bookId} was not found.");
                return Result.NotFound();
            }

            var book = _mapper.Map<Book>(bookCreationDTO);
            book.Id = bookId;
            AssignAuthorsOrder(book);

            await _bookRepository.Update(book);
            _logger.LogInformation($"Book with ID {book.Id} updated successfully.");
            return Result.Success();
        }

        private void AssignAuthorsOrder(Book book)
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
