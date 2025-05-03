using LibraryAPI.Data;
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
        public async Task<ActionResult<Book>> Get(int id)
        {
            var book = await context.Books
                   .Include(x => x.Author)
                   .FirstOrDefaultAsync(x => x.Id == id);
            if (book is null)
                return NotFound();

            return book;
        }

        [HttpPost]
        public async Task<ActionResult> Post(Book book)
        {
            var existsAuthor = await context.Authors.AnyAsync(x => x.Id == book.AuthorId);
            if (!existsAuthor)
                return BadRequest($"Author id {book.AuthorId} does not exist");

            context.Add(book);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, Book book)
        {
            if (id != book.Id)
                return BadRequest("Book IDs must match");

            var existsBook = await context.Books.AnyAsync(x => x.Id == id);
            if (!existsBook)
                return BadRequest("Incorrect book ID");

            var existsAuthor = await context.Authors.AnyAsync(x => x.Id == book.AuthorId);
            if (!existsAuthor)
                return BadRequest($"Author ID {book.AuthorId} does not exist");

            context.Update(book);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var recordsDeleted = await context.Books.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (recordsDeleted == 0)
                return NotFound();
            return Ok();
        }
    }
}
