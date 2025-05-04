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
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthorsController> _logger;

        public AuthorsController(ApplicationDbContext _context, ILogger<AuthorsController> _logger)
        {
            this._context = _context;
            this._logger = _logger;
        }

        [HttpGet]
        public async Task<IEnumerable<Author>> Get()
        {
            _logger.LogInformation("Retrieving all authors.");
            return await _context.Authors.ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AuthorDto>> Get([FromRoute] int id)
        {
            _logger.LogInformation("Retrieving author with ID {AuthorId}", id);

            var author = await _context.Authors
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
            {
                _logger.LogWarning("Author with ID {AuthorId} not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Author with ID {AuthorId} retrieved successfully.", id);
            return Ok(author);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Author author)
        {
            _logger.LogInformation("Creating author with name '{Name}'", author.Name);

            _context.Add(author);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Author with ID {AuthorId} created successfully.", author.Id);
            return Created();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put([FromRoute] int id, [FromBody] Author author)
        {
            _logger.LogInformation("Updating author with route ID {RouteId} and body ID {BodyId}", id, author.Id);

            if (id != author.Id)
            {
                _logger.LogWarning("Mismatch between route ID {RouteId} and body ID {BodyId}", id, author.Id);
                ModelState.AddModelError(nameof(author.Id), "The route ID and the body ID must match.");
                return ValidationProblem();
            }

            var exists = await _context.Authors.AnyAsync(x => x.Id == id);
            if (!exists)
            {
                _logger.LogWarning("Attempted to update non-existing author with ID {AuthorId}", id);
                ModelState.AddModelError(nameof(author.Id), "The provided ID does not match any existing author.");
                return ValidationProblem();
            }

            _context.Update(author);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Author with ID {AuthorId} updated successfully.", id);
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            _logger.LogInformation("Attempting to delete author with ID {AuthorId}", id);

            var recordsDeleted = await _context.Authors.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (recordsDeleted == 0)
            {
                _logger.LogWarning("Attempted to delete non-existing author with ID {AuthorId}", id);
                return NotFound();
            }

            _logger.LogInformation("Author with ID {AuthorId} deleted successfully.", id);
            return Ok();
        }
    }
}
