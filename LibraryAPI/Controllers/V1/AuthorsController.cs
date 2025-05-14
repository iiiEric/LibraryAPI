using AutoMapper;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Services;
using LibraryAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Linq.Dynamic.Core;
using static LibraryAPI.Utils.ResponseHelper;

namespace LibraryAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Policy = "Admin")]
    public class AuthorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorsController> _logger;
        private readonly IFileStorageService _fileStorageService;
        private readonly IOutputCacheStore _outputCacheStore;
        private const string _container = "Authors";
        private const string _cache = "AuthorsCache";

        public AuthorsController(ApplicationDbContext context, IMapper mapper, ILogger<AuthorsController> logger,
            IFileStorageService fileStorageService, IOutputCacheStore outputCacheStore)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _fileStorageService = fileStorageService;
            _outputCacheStore = outputCacheStore;
        }

        [HttpGet(Name = "GetAuthorsV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [_cache])]
        [EndpointSummary("Retrieves a paginated list of authors.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status200OK)]
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

        [HttpGet("filterV1", Name = "filterAuthorsV1")]
        [AllowAnonymous]
        [EndpointSummary("Retrieves a list of authors filtered by the specified criteria.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<object>>> Filter([FromQuery] AuthorFilterDTO authorFilterDTO)
        {
            _logger.LogInformation("Filtering authors.");

            var queryable = _context.Authors.AsQueryable();

            if (!string.IsNullOrEmpty(authorFilterDTO.Name))
                queryable = queryable.Where(x => x.Name.Contains(authorFilterDTO.Name));

            if (!string.IsNullOrEmpty(authorFilterDTO.Surname1))
                queryable = queryable.Where(x => x.Name.Contains(authorFilterDTO.Surname1));

            if (authorFilterDTO.HasImage.HasValue)
            {
                queryable = authorFilterDTO.HasImage.Value
                    ? queryable.Where(x => x.ImageUrl != null)
                    : queryable.Where(x => x.ImageUrl == null);
            }

            if (!string.IsNullOrEmpty(authorFilterDTO.BookTitle))
                queryable = queryable.Where(x => x.Books.Any(y => y.Book!.Title.Contains(authorFilterDTO.BookTitle)));

            if (authorFilterDTO.IncludeBooks)
                queryable = queryable.Include(x => x.Books).ThenInclude(x => x.Book);

            if (!string.IsNullOrEmpty(authorFilterDTO.SortBy))
            {
                var orderType = authorFilterDTO.SortAscending ? "ascending" : "descending";
                try
                {
                    queryable = queryable.OrderBy($"{authorFilterDTO.SortBy} {orderType}");
                }
                catch (Exception ex)
                {
                    queryable = queryable.OrderBy(x => x.Name);
                    _logger.LogError(ex, "Error ordering authors by {OrderType} on field {SortBy}", orderType, authorFilterDTO.SortBy);
                }
            }
            else
            {
                queryable = queryable.OrderBy(x => x.Name);
            }

            var authors = await queryable.Paginate(authorFilterDTO.PaginationDTO).ToListAsync();

            return authorFilterDTO.IncludeBooks
                ? Ok(_mapper.Map<IEnumerable<AuthorWithBooksDTO>>(authors))
                : Ok(_mapper.Map<IEnumerable<AuthorDTO>>(authors));
        }

        [HttpGet("{id:int}", Name = "GetAuthorV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [_cache])]
        [EndpointSummary("Retrieves a single author with their books by the specified author ID.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuthorWithBooksDTO>> Get([FromRoute][Description("Author Id")] int id)
        {
            _logger.LogInformation("Retrieving author with ID {AuthorId}", id);

            var author = await _context.Authors
                 .Include(x => x.Books)
                 .ThenInclude(x => x.Book)
                 .FirstOrDefaultAsync(x => x.Id == id);

            if (author is null)
                return LogAndReturnNotFound(_logger, $"Author with ID {id} was not found.");

            var authorWithBooksDTO = _mapper.Map<AuthorWithBooksDTO>(author);
            return Ok(authorWithBooksDTO);
        }

        [HttpPost(Name = "CreateAuthorV1")]
        [EndpointSummary("Creates a new author from the provided data.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status201Created)]
        public async Task<ActionResult> Post([FromBody] AuthorCreationDTO authorCreationDTO)
        {
            var author = _mapper.Map<Author>(authorCreationDTO);

            _logger.LogInformation("Creating author with name '{Name}'", author.Name);

            _context.Add(author);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(_cache, default);

            var authorDTO = _mapper.Map<AuthorDTO>(author);

            return LogAndReturnCreatedAtRoute(_logger, "GetAuthorV1", new { id = author.Id }, authorDTO,
                $"Author with ID {author.Id} created successfully.");
        }

        [HttpPost("with-image", Name = "CreateAuthorWithImageV1")]
        [EndpointSummary("Creates a new author with an image from the provided data.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status201Created)]
        public async Task<ActionResult> PostWithImage([FromForm] AuthorCreationWithImageDTO authorCreationWithImageDTO)
        {
            var author = _mapper.Map<Author>(authorCreationWithImageDTO);

            _logger.LogInformation("Creating author with name '{Name}'", author.Name);

            if (authorCreationWithImageDTO.Image is not null)
                author.ImageUrl = await _fileStorageService.Store(_container, authorCreationWithImageDTO.Image);

            _context.Add(author);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(_cache, default);

            var authorDTO = _mapper.Map<AuthorDTO>(author);

            return LogAndReturnCreatedAtRoute(_logger, "GetAuthorV1", new { id = author.Id }, authorDTO,
                $"Author with ID {author.Id} created successfully.");
        }

        [HttpPut("{id:int}", Name = "UpdateAuthorV1")]
        [EndpointSummary("Updates an existing author with the provided data and image.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status204NoContent)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Put([FromRoute] int id, [FromForm] AuthorCreationWithImageDTO authorCreationWithImageDTO)
        {
            var exists = await _context.Authors.AnyAsync(x => x.Id == id);
            if (!exists)
                return LogAndReturnNotFound(_logger, $"Author with ID {id} was not found.");

            var author = _mapper.Map<Author>(authorCreationWithImageDTO);
            author.Id = id;

            if (authorCreationWithImageDTO.Image is not null)
            {
                var currentImage = await _context.Authors
                    .Where(x => x.Id == id)
                    .Select(x => x.ImageUrl)
                    .FirstOrDefaultAsync();

                var imageUrl = await _fileStorageService.Update(currentImage, _container, authorCreationWithImageDTO.Image);
                author.ImageUrl = imageUrl;
            }

            _context.Update(author);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(_cache, default);

            return LogAndReturnNoContent(_logger, $"Author with ID {id} updated successfully.");
        }

        [HttpPatch("{id:int}", Name = "PatchAuthorV1")]
        [EndpointSummary("Partially updates an existing author with the provided patch document.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status204NoContent)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Patch([FromRoute] int id, [FromBody] JsonPatchDocument<AuthorPatchDTO> patchDocument)
        {
            _logger.LogInformation("Received PATCH request for author with ID {AuthorId}.", id);

            if (patchDocument is null)
            {
                return LogAndReturnValidationProblem(_logger, "PatchDocument", "Patch document is null.", ModelState);
            }

            var author = await _context.Authors.FirstOrDefaultAsync(x => x.Id == id);
            if (author is null)
                return LogAndReturnNotFound(_logger, $"Author with ID {id} was not found.");

            var authorPatchDTO = _mapper.Map<AuthorPatchDTO>(author);
            patchDocument.ApplyTo(authorPatchDTO, ModelState);

            var isValid = TryValidateModel(authorPatchDTO);
            if (!isValid)
                return LogAndReturnValidationProblem(_logger, "AuthorPatchDTO", "Validation failed.", ModelState);
            

            _mapper.Map(authorPatchDTO, author);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(_cache, default);

            return LogAndReturnNoContent(_logger, $"Author with ID {id} patched successfully.");
        }

        [HttpDelete("{id:int}", Name = "DeleteAuthorV1")]
        [EndpointSummary("Deletes an author by the specified author ID.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status204NoContent)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            _logger.LogInformation("Attempting to delete author with ID {AuthorId}", id);

            var author = await _context.Authors.FirstOrDefaultAsync(x => x.Id == id);
            if (author is null)
                return LogAndReturnNotFound(_logger, $"Author with ID {id} was not found.");

            _context.Remove(author);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(_cache, default);
            await _fileStorageService.Delete(author.ImageUrl, _container);

            return LogAndReturnNoContent(_logger, $"Author with ID {id} deleted successfully.");
        }
    }
}