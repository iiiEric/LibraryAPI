using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController: ControllerBase
    {
        private readonly ApplicationDbContext context;

        public AuthorsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<Author>> Get()
        {
            return await context.Authors.ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AuthorDto>> Get([FromRoute] int id)
        {
            var author = await context.Authors
                 .Include(x => x.Books)
                 .Where(x => x.Id == id)
                 .Select(x => new AuthorDto
                 {
                     Id = x.Id,
                     Name = x.Name,
                     BookTitles = x.Books.Select(book => book.Title).ToList()
                 })
                 .FirstOrDefaultAsync();

            if (author is null)
                return NotFound();

            return Ok(author);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Author author)
        {
            context.Add(author);
            await context.SaveChangesAsync();
            return Created();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put([FromRoute] int id, [FromBody] Author author)
        {
            if (id != author.Id)
            {
                ModelState.AddModelError(nameof(author.Id), "The route ID and the body ID must match.");
                return ValidationProblem();
            }

            var exists = await context.Authors.AnyAsync(x => x.Id == id);
            if (!exists)
            {
                ModelState.AddModelError(nameof(author.Id), "The provided ID does not match any existing author.");
                return ValidationProblem();
            }

            context.Update(author);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            var recordsDeleted = await context.Authors.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (recordsDeleted == 0)
                return NotFound();
            return Ok();
        }
    }
}
