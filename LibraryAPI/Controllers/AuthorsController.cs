using AutoMapper;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Admin")]
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
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AuthorDTO>>> Get([FromQuery] PaginationDTO paginationDTO)
        {
            _logger.LogInformation("Retrieving all authors.");
            var queryable = _context.Authors.AsQueryable();
            await HttpContext.InsertHeaderPaginationParameters(queryable);

            var authors = await queryable
                .OrderBy(x => x.Name)
                .Paginate(paginationDTO)
                .ToListAsync();
            var authorsDTO = _mapper.Map<IEnumerable<AuthorDTO>>(authors);
            return Ok(authorsDTO);
        }

        [HttpGet("{id:int}", Name = "GetAuthor")]
        [AllowAnonymous]
        [EndpointSummary("Retrieves an author by their unique identifier.")]
        [EndpointDescription("Include his/her books. If the author does not exist, returns 404")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuthorWithBooksDTO>> Get([FromRoute] [Description("Author Id")] int id)
        {
            _logger.LogInformation("Retrieving author with ID {AuthorId}", id);

            var author = await _context.Authors
                 .Include(x => x.Books)
                 .ThenInclude(x => x.Book)
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
            var exists = await _context.Authors.AnyAsync(x => x.Id == id);
            if (!exists)
            {
                _logger.LogWarning("Attempted to update non-existing author with ID {AuthorId}", id);
                ModelState.AddModelError(nameof(id), "The provided ID does not match any existing author.");
                return ValidationProblem();
            }

            var author = _mapper.Map<Author>(authorCreationDTO);
            author.Id = id;

            _context.Update(author);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Author with ID {AuthorId} updated successfully.", id);
            return NoContent();
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch([FromRoute] int id, [FromBody] JsonPatchDocument<AuthorPatchDTO> patchDocument)
        {
            _logger.LogInformation("Received PATCH request for author with ID {AuthorId}.", id);

            if (patchDocument is null)
            {
                _logger.LogWarning("PATCH request for author with ID {AuthorId} failed: patch document is null.", id);
                ModelState.AddModelError(nameof(patchDocument), "Patch document cannot be null.");
                return ValidationProblem();
            }

            var authorDB = await _context.Authors.FirstOrDefaultAsync(x => x.Id == id);
            if (authorDB is null)
            {
                _logger.LogWarning("PATCH request failed: author with ID {AuthorId} not found.", id);
                ModelState.AddModelError(nameof(id), $"Author with ID {id} not found.");
                return ValidationProblem();
            }

            var authorPatchDTO = _mapper.Map<AuthorPatchDTO>(authorDB);
            patchDocument.ApplyTo(authorPatchDTO, ModelState);

            var isValid = TryValidateModel(authorPatchDTO);
            if (!isValid)
            {
                _logger.LogWarning("PATCH request for author with ID {AuthorId} failed validation.", id);
                return ValidationProblem();
            }

            _mapper.Map(authorPatchDTO, authorDB);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Author with ID {AuthorId} patched successfully.", id);
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
