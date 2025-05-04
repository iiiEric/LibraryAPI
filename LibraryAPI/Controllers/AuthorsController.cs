using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorsController> _logger;

        public AuthorsController(ApplicationDbContext context, IMapper mapper, ILogger<AuthorsController> logger)
        {
            this._context = context;
            this._mapper = mapper;
            this._logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorDTO>>> Get()
        {
            _logger.LogInformation("Retrieving all authors.");
            var authors = await _context.Authors.ToListAsync();
            var authorsDTO = _mapper.Map<IEnumerable<AuthorDTO>>(authors);
            return Ok(authorsDTO);
        }

        [HttpGet("{id:int}", Name = "GetAuthor")]
        public async Task<ActionResult<AuthorWithBooksDTO>> Get([FromRoute] int id)
        {
            _logger.LogInformation("Retrieving author with ID {AuthorId}", id);

            var author = await _context.Authors
                 .Include(x => x.Books)
                 .FirstOrDefaultAsync(x => x.Id == id);

            if (author is null)
            {
                _logger.LogWarning("Author with ID {AuthorId} not found.", id);
                return NotFound();
            }

            var authorWithBooksDTO = _mapper.Map<AuthorWithBooksDTO>(author);

            _logger.LogInformation("Author with ID {AuthorId} retrieved successfully.", id);
            return Ok(authorWithBooksDTO);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] AuthorCreationDTO authorCreationDTO)
        {
            var author = _mapper.Map<Author>(authorCreationDTO);

            _logger.LogInformation("Creating author with name '{Name}'", author.Name);

            _context.Add(author);
            await _context.SaveChangesAsync();

            var authorDTO = _mapper.Map<AuthorDTO>(author);

            _logger.LogInformation("Author with ID {AuthorId} created successfully.", author.Id);
            return CreatedAtRoute("GetAuthor", new { id = author.Id }, authorDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put([FromRoute] int id, [FromBody] AuthorCreationDTO authorCreationDTO)
        {
            var author = _mapper.Map<Author>(authorCreationDTO);
            author.Id = id;

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
            return NoContent();
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
            return NoContent();
        }
    }
}
