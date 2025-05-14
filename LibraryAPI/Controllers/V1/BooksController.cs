using AutoMapper;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using static LibraryAPI.Utils.ResponseHelper;

namespace LibraryAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Policy = "Admin")]
    public class BooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<BooksController> _logger;
        private readonly IOutputCacheStore _outputCacheStore;
        private const string _cache = "BooksCache";

        public BooksController(ApplicationDbContext context, IMapper mapper, ILogger<BooksController> logger,
            IOutputCacheStore outputCacheStore)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _outputCacheStore = outputCacheStore;
        }

        [HttpGet(Name = "GetBooksV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [_cache])]
        [EndpointSummary("Retrieves a paginated list of books.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookDTO>>> Get([FromQuery] PaginationDTO paginationDTO)
        {
            var queryable = _context.Books.AsQueryable();
            await HttpContext.InsertHeaderPaginationParameters(queryable);

            var books = await queryable
                .OrderBy(x => x.Title)
                .Paginate(paginationDTO)
                .ToListAsync();
            var booksDTO = _mapper.Map<IEnumerable<BookDTO>>(books);

            return Ok(booksDTO);
        }

        [HttpGet("{id:int}", Name = "GetBookV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [_cache])]
        [EndpointSummary("Retrieves a single book with its authors by the specified book ID.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookWithAuthorsDTO>> Get([FromRoute][Description("Book Id")] int id)
        {
            var book = await _context.Books
                .Include(x => x.Authors)
                .ThenInclude(x => x.Author)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (book is null)
                return LogAndReturnNotFound(_logger, $"Book with ID {id} was not found.");

            var bookWithAuthorDTO = _mapper.Map<BookWithAuthorsDTO>(book);

            return Ok(bookWithAuthorDTO);
        }

        [HttpPost(Name = "CreateBookV1")]
        [ServiceFilter<ValidateBookFilter>]
        [EndpointSummary("Creates a new book from the provided data.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status201Created)]
        public async Task<ActionResult> Post([FromBody] BookCreationDTO bookCreationDTO)
        {
            var book = _mapper.Map<Book>(bookCreationDTO);
            assignAuthorsOrder(book);

            _context.Add(book);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(_cache, default);

            var bookDTO = _mapper.Map<BookDTO>(book);

            return LogAndReturnCreatedAtRoute(_logger, "GetBookV1", new { id = book.Id }, bookDTO,
                $"Book with ID {book.Id} created successfully.");
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

        [HttpPut("{id:int}", Name = "UpdateBookV1")]
        [ServiceFilter<ValidateBookFilter>]
        [EndpointSummary("Updates an existing book with the provided data.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status204NoContent)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Put([FromRoute] int id, [FromBody] BookCreationDTO bookCreationDTO)
        {
            var book = await _context.Books
                .Include(x => x.Authors)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (book is null)
                return LogAndReturnNotFound(_logger, $"Book with ID {id} was not found.");

            book = _mapper.Map(bookCreationDTO, book);
            assignAuthorsOrder(book);

            _context.Update(book);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(_cache, default);

            return LogAndReturnNoContent(_logger, $"Book with ID {id} updated successfully.");
        }

        [HttpDelete("{id:int}", Name = "DeleteBookV1")]
        [EndpointSummary("Deletes a book by the specified book ID.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status204NoContent)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            var recordsDeleted = await _context.Books.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (recordsDeleted == 0)
                return LogAndReturnNotFound(_logger, $"Book with ID {id} was not found.");

            await _outputCacheStore.EvictByTagAsync(_cache, default);

            return LogAndReturnNoContent(_logger, $"Book with ID {id} deleted successfully.");
        }
    }
}