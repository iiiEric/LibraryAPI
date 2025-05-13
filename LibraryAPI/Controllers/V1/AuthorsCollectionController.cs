using AutoMapper;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using static LibraryAPI.Utils.ResponseHelper;

namespace LibraryAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Policy = "Admin")]
    public class AuthorsCollectionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorsController> _logger;

        public AuthorsCollectionController(ApplicationDbContext context, IMapper mapper, ILogger<AuthorsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("{ids}", Name = "GetAuthorsByIdsV1")]
        [AllowAnonymous]
        [EndpointSummary("Retrieves a list of authors along with their books based on the provided author IDs.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<AuthorWithBooksDTO>>> Get([FromRoute][Description("Author Ids")] string ids)
        {
            _logger.LogInformation("Retrieving authors with IDs {AuthorsIds}", ids);

            var collectionIds = new List<int>();
            foreach (var id in ids.Split(","))
            {
                if (int.TryParse(id, out int idInt))
                    collectionIds.Add(idInt);
            }

            if (!collectionIds.Any())
                return LogAndReturnValidationProblem(_logger, nameof(ids), "No valid author IDs provided.", ModelState);

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
                return LogAndReturnNotFound(_logger, "Some author IDs were not found: {0}", authorsNotExistsString);
            }

            var authorsWithBooksDTO = _mapper.Map<List<AuthorWithBooksDTO>>(authors);

            _logger.LogInformation("Authors with IDs {AuthorsIds} retrieved successfully.", ids);
            return Ok(authorsWithBooksDTO);
        }

        [HttpPost(Name = "CreateAuthorsV1")]
        [EndpointSummary("Creates multiple authors from the provided data.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status201Created)]
        public async Task<ActionResult> Post([FromBody] IEnumerable<AuthorCreationDTO> authorsCreationDTO)
        {
            var authors = _mapper.Map<IEnumerable<Author>>(authorsCreationDTO);

            foreach (var author in authors)
                _logger.LogInformation("Creating author with name '{Name}'", author.Name);

            _context.AddRange(authors);
            await _context.SaveChangesAsync();

            var authorsDTO = _mapper.Map<IEnumerable<AuthorDTO>>(authors);
            var ids = authors.Select(x => x.Id);
            var idsString = string.Join(", ", ids);

            foreach (var author in authors)
                _logger.LogInformation("Author with ID {AuthorId} created successfully.", author.Id);

            return LogAndReturnCreatedAtRoute(_logger, "GetAuthorsByIdsV1", new { ids = idsString }, authorsDTO, 
                "Created authors with IDs: {0}", idsString);
        }
    }
}