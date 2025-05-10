using AutoMapper;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Admin")]
    public class AuthorsCollectionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorsController> _logger;

        public AuthorsCollectionController(ApplicationDbContext context, IMapper mapper, ILogger<AuthorsController> logger)
        {
            this._context = context;
            this._mapper = mapper;
            this._logger = logger;
        }

        [HttpGet("{ids}", Name = "GetAuthorsByIds")]
        [AllowAnonymous]
        public async Task<ActionResult<List<AuthorWithBooksDTO>>> Get([FromRoute] string ids)
        {
            _logger.LogInformation("Retrieving authors with IDs {AuthorsIds}", ids);

            var collectionIds = new List<int>();
            foreach (var id in ids.Split(","))
            {
                if (int.TryParse(id, out int idInt))
                    collectionIds.Add(idInt);
            }

            if (!collectionIds.Any())
            {
                _logger.LogWarning("No valid author IDs provided.");
                ModelState.AddModelError(nameof(ids), "No valid author IDs provided.");
                return ValidationProblem();
            }

            var authors = await _context.Authors
                 .Include(x => x.Books)
                 .ThenInclude(x => x.Book)
                 .Where(x => collectionIds.Contains(x.Id))
                 .ToListAsync();

            if (authors.Count != collectionIds.Count)
            {
                var authorsExistsIds = authors.Select(x => x.Id).ToList();
                var authorsNotExists = collectionIds.Except(authorsExistsIds);
                var authorsNotExistsString = string.Join(", ", authorsNotExists);
                _logger.LogWarning("Some author IDs not found: {AuthorsNotExistsIds}", authorsNotExistsString);
                ModelState.AddModelError(nameof(ids), $"Some author IDs not found: {authorsNotExistsString}");
                return ValidationProblem();
            }
           
            var authorsWithBooksDTO = _mapper.Map<List<AuthorWithBooksDTO>>(authors);

            _logger.LogInformation("Authors with IDs {AuthorsIds} retrieved successfully.", ids);
            return Ok(authorsWithBooksDTO);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] IEnumerable<AuthorCreationDTO> authorsCreationDTO)
        {
            var authors = _mapper.Map<IEnumerable<Author>>(authorsCreationDTO);

            foreach (var author in authors)
                _logger.LogInformation("Creating author with name '{Name}'", author.Name);

            _context.AddRange(authors);
            await _context.SaveChangesAsync();

            var authorsDTO = _mapper.Map<IEnumerable<AuthorDTO>>(authors);
            var ids = authors.Select(x => x.Id);
            var IdsString = string.Join(", ", ids);

            foreach (var author in authors)
                _logger.LogInformation("Author with ID {AuthorId} created successfully.", author.Id);

            return CreatedAtRoute("GetAuthorsByIds", new { ids = IdsString }, authorsDTO);
        }
    }
}
