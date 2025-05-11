using AutoMapper;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

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
        //private readonly ITimeLimitedDataProtector _timeLimitedDataProtector;
        private readonly IOutputCacheStore _outputCacheStore;
        private const string _cache = "BooksCache";

        public BooksController(ApplicationDbContext context, IMapper mapper, ILogger<BooksController> logger/*, IDataProtectionProvider dataProtectionProvider*/,
            IOutputCacheStore outputCacheStore)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            //this._timeLimitedDataProtector = dataProtectionProvider.CreateProtector("BooksController").ToTimeLimitedDataProtector();
            _outputCacheStore = outputCacheStore;
        }

        //[HttpGet("collection/get-tokenV1")]
        //public ActionResult GetTokenForBookCollection()
        //{
        //    var plainText = Guid.NewGuid().ToString();
        //    var token = _timeLimitedDataProtector.Protect(plainText, lifetime: TimeSpan.FromDays(1));
        //    var url = Url.RouteUrl("GetBookCollectionUsingToken", new { token }, "https");
        //    return Ok(new { url });
        //}

        //[HttpGet("collection/{token}V1", Name = "GetBookCollectionUsingTokenV1")]
        //[AllowAnonymous]
        //public async Task<ActionResult<IEnumerable<BookDTO>>> GetCollectionUsingToken(string token)
        //{
        //    try
        //    {
        //        _timeLimitedDataProtector.Unprotect(token);
        //    }
        //    catch (Exception)
        //    {
        //        _logger.LogWarning("Token has expired.");
        //        ModelState.AddModelError(nameof(token), "Token has expired.");
        //        return ValidationProblem();
        //    }

        //    _logger.LogInformation("Retrieving all books.");

        //    var books = await _context.Books.ToListAsync(); ;
        //    var booksDTO = _mapper.Map<IEnumerable<BookDTO>>(books);

        //    return Ok(booksDTO);
        //}

        [HttpGet(Name = "GetBooksV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [_cache])]
        public async Task<ActionResult<IEnumerable<BookDTO>>> Get([FromQuery] PaginationDTO paginationDTO)
        {
            _logger.LogInformation("Retrieving all books.");

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
        public async Task<ActionResult<BookWithAuthorsDTO>> Get([FromRoute] int id)
        {
            _logger.LogInformation("Retrieving book with ID {BookId}", id);

            var book = await _context.Books
                .Include(x => x.Authors)
                .ThenInclude(x => x.Author)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (book is null)
            {
                _logger.LogWarning("Book with ID {BookId} not found.", id);
                return NotFound();
            }

            var bookWithAuthorDTO = _mapper.Map<BookWithAuthorsDTO>(book);

            _logger.LogInformation("Book with ID {BookId} retrieved successfully.", id);
            return Ok(bookWithAuthorDTO);
        }

        [HttpPost(Name = "CreateBookV1")]
        [ServiceFilter<ValidateBookFilter>]
        public async Task<ActionResult> Post([FromBody] BookCreationDTO bookCreationDTO)
        {
            var book = _mapper.Map<Book>(bookCreationDTO);
            assignAuthorsOrder(book);

            _logger.LogInformation("Creating book with title '{Title}' and author IDs {AuthorIds}", book.Title, string.Join(", ", bookCreationDTO.AuthorsIds));

            _context.Add(book);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(_cache, default);

            var bookDTO = _mapper.Map<BookDTO>(book);

            _logger.LogInformation("Book with ID {BookId} created successfully.", book.Id);
            return CreatedAtRoute("GetBookV1", new { id = book.Id }, bookDTO);
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
        public async Task<ActionResult> Put([FromRoute] int id, [FromBody] BookCreationDTO bookCreationDTO)
        {
            var bookDB = await _context.Books
                .Include(x => x.Authors)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (bookDB is null)
            {
                _logger.LogWarning("Attempted to update non-existing book with ID {BookId}", id);
                ModelState.AddModelError(nameof(id), "The provided ID does not match any existing book.");
                return ValidationProblem();
            }

            bookDB = _mapper.Map(bookCreationDTO, bookDB);
            assignAuthorsOrder(bookDB);

            _context.Update(bookDB);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(_cache, default);

            _logger.LogInformation("Book with ID {BookId} updated successfully.", id);
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "DeleteBookV1")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            _logger.LogInformation("Attempting to delete book with ID {BookId}", id);

            var recordsDeleted = await _context.Books.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (recordsDeleted == 0)
            {
                _logger.LogWarning("Attempted to delete non-existing book with ID {BookId}", id);
                return NotFound();
            }
            else
                await _outputCacheStore.EvictByTagAsync(_cache, default);

            _logger.LogInformation("Book with ID {BookId} deleted successfully.", id);
            return NoContent();
        }
    }
}
