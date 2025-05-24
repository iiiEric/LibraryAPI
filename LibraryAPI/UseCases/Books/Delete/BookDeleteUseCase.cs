using LibraryAPI.DatabaseAccess.BooksRepository;
using LibraryAPI.Utils;

namespace LibraryAPI.UseCases.Books.Delete
{
    public class BookDeleteUseCase : IBookDeleteUseCase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<BookDeleteUseCase> _logger;

        public BookDeleteUseCase(IBookRepository bookRepository, ILogger<BookDeleteUseCase> logger)
        {
            _bookRepository = bookRepository;
            _logger = logger;
        }

        public async Task<Result> Run(int bookId)
        {
            _logger.LogInformation("Attempting to delete book with ID {BookId}", bookId);

            var book = await _bookRepository.GetById(bookId);
            if (book is null)
            {
                _logger.LogWarning($"Book with ID {bookId} was not found.");
                return Result.NotFound();
            }

            await _bookRepository.Remove(book);
            _logger.LogInformation($"Book with ID {bookId} deleted successfully.");
            return Result.Success();
        }
    }
}
