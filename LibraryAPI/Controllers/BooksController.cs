using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BooksController> _logger;

        public BooksController(ApplicationDbContext context, ILogger<BooksController> logger)
        {
            this._context = context;
            this._logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<Book>> Get()
        {
            _logger.LogInformation("Retrieving all books.");
            return await _context.Books.ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookDto>> Get([FromRoute] int id)
        {
            _logger.LogInformation("Retrieving book with ID {BookId}", id);

            var book = await _context.Books
                .Include(x => x.Author)
                .Where(x => x.Id == id)
                .Select(x => new BookDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    AuthorName = x.Author != null ? x.Author.Name : null
                })
                .FirstOrDefaultAsync();

            if (book is null)
            {
                _logger.LogWarning("Book with ID {BookId} not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Book with ID {BookId} retrieved successfully.", id);
            return book;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Book book)
        {
            _logger.LogInformation("Creating book with title '{Title}' and author ID {AuthorId}", book.Title, book.AuthorId);

            var existsAuthor = await _context.Authors.AnyAsync(x => x.Id == book.AuthorId);
            if (!existsAuthor)
            {
                _logger.LogWarning("Failed to create book. Author with ID {AuthorId} does not exist.", book.AuthorId);
                ModelState.AddModelError(nameof(book.AuthorId), $"Author id {book.AuthorId} does not exist");
                return ValidationProblem();
            }

            _context.Add(book);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Book with ID {BookId} created successfully.", book.Id);
            return Created();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put([FromRoute] int id, [FromBody] Book book)
        {
            _logger.LogInformation("Updating book with route ID {RouteId} and body ID {BodyId}", id, book.Id);

            if (id != book.Id)
            {
                _logger.LogWarning("ID mismatch: route ID {RouteId} does not match body ID {BodyId}", id, book.Id);
                ModelState.AddModelError(nameof(book.Id), "The route ID and the body ID must match.");
                return ValidationProblem();
            }

            var existsBook = await _context.Books.AnyAsync(x => x.Id == id);
            if (!existsBook)
            {
                _logger.LogWarning("Attempted to update non-existing book with ID {BookId}", id);
                ModelState.AddModelError(nameof(book.Id), "The provided ID does not match any existing book.");
                return ValidationProblem();
            }

            var existsAuthor = await _context.Authors.AnyAsync(x => x.Id == book.AuthorId);
            if (!existsAuthor)
            {
                _logger.LogWarning("Invalid author ID {AuthorId} provided for book ID {BookId}", book.AuthorId, id);
                ModelState.AddModelError(nameof(book.AuthorId), "The provided ID does not match any existing author.");
                return ValidationProblem();
            }

            _context.Update(book);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Book with ID {BookId} updated successfully.", id);
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            _logger.LogInformation("Attempting to delete book with ID {BookId}", id);

            var recordsDeleted = await _context.Books.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (recordsDeleted == 0)
            {
                _logger.LogWarning("Attempted to delete non-existing book with ID {BookId}", id);
                return NotFound();
            }

            _logger.LogInformation("Book with ID {BookId} deleted successfully.", id);
            return Ok();
        }
    }
}
