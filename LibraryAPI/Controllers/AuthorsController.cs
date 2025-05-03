using LibraryAPI.Data;
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
        public async Task<ActionResult<Author>> Get(int id)
        {
            var author = await context.Authors
                .Include(x => x.Books)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (author is null)
                return NotFound();

            return author;
        }

        [HttpPost]
        public async Task<ActionResult> Post(Author author)
        {
            context.Add(author);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, Author author)
        {
            if (id != author.Id)
                return BadRequest("IDs must match");

            var exists = await context.Authors.AnyAsync(x => x.Id == id);
            if (!exists)
                return BadRequest("Incorrect ID");

            context.Update(author);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var recordsDeleted = await context.Authors.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (recordsDeleted == 0)
                return NotFound();
            return Ok();
        }
    }
}
