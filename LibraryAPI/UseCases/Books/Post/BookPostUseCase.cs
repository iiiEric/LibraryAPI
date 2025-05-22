using AutoMapper;
using LibraryAPI.DatabaseAccess.BooksRepository;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using Microsoft.AspNetCore.OutputCaching;

namespace LibraryAPI.UseCases.Books.Post
{
    public class BookPostUseCase : IBookPostUseCase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BookPostUseCase> _logger;

        public BookPostUseCase(IBookRepository bookRepository, IMapper mapper, ILogger<BookPostUseCase> logger)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BookDTO> Run(BookCreationDTO bookCreationDTO)
        {
            _logger.LogInformation("Creating book with title '{title}'", bookCreationDTO.Title);

            var book = _mapper.Map<Book>(bookCreationDTO);
            assignAuthorsOrder(book);

            await _bookRepository.Add(book);
            _logger.LogInformation($"Book with ID {book.Id} created successfully.");

            var bookDTO = _mapper.Map<BookDTO>(book);

            return bookDTO;
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
