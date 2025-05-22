using AutoMapper;
using LibraryAPI.DatabaseAccess.BooksRepository;
using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Books.GetAll
{
    public class BooksGetAllUseCase : IBooksGetAllUseCase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BooksGetAllUseCase> _logger;

        public BooksGetAllUseCase(IBookRepository bookRepository, IMapper mapper, ILogger<BooksGetAllUseCase> logger)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<BookDTO>> Run(HttpContext httpContext, PaginationDTO paginationDTO)
        {
            _logger.LogInformation("Retrieving all books.");
            var books = await _bookRepository.GetAll(httpContext, paginationDTO);
            var booksDTO = _mapper.Map<IEnumerable<BookDTO>>(books);
            return booksDTO;
        }
    }
}
