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
        private readonly ApplicationDbContext context;

        public BooksController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<Book>> Get()
        {
            return await context.Books.ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookDto>> Get([FromRoute] int id)
        {
            var book = await context.Books
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
                return NotFound();

            return book;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Book book)
        {
            var existsAuthor = await context.Authors.AnyAsync(x => x.Id == book.AuthorId);
            if (!existsAuthor)
            {
                //return BadRequest($"Author id {book.AuthorId} does not exist");
                ModelState.AddModelError(nameof(book.AuthorId), $"Author id {book.AuthorId} does not exist");
                return ValidationProblem();
            }
                
            context.Add(book);
            await context.SaveChangesAsync();
            return Created();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put([FromRoute] int id, [FromBody] Book book)
        {
            if (id != book.Id)
            {
                ModelState.AddModelError(nameof(book.Id), "The route ID and the body ID must match.");
                return ValidationProblem();
            }

            var existsBook = await context.Books.AnyAsync(x => x.Id == id);
            if (!existsBook)
            {
                ModelState.AddModelError(nameof(book.Id), "The provided ID does not match any existing book.");
                return ValidationProblem();
            }

            var existsAuthor = await context.Authors.AnyAsync(x => x.Id == book.AuthorId);
            if (!existsAuthor)
            {
                ModelState.AddModelError(nameof(book.AuthorId), "The provided ID does not match any existing author.");
                return ValidationProblem();
            }

            context.Update(book);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            var recordsDeleted = await context.Books.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (recordsDeleted == 0)
                return NotFound();
            return Ok();
        }
    }
}
