using LibraryAPI.Constants;
using LibraryAPI.DTOs;
using LibraryAPI.UseCases.Authors.Delete;
using LibraryAPI.UseCases.Authors.GetAll;
using LibraryAPI.UseCases.Authors.GetByCriteria;
using LibraryAPI.UseCases.Authors.GetById;
using LibraryAPI.UseCases.Authors.Patch;
using LibraryAPI.UseCases.Authors.Post;
using LibraryAPI.UseCases.Authors.PostWithImage;
using LibraryAPI.UseCases.Authors.Put;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OutputCaching;
using System.ComponentModel;

namespace LibraryAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Policy = "Admin")]
    public class AuthorsController : ControllerBase
    {
        private readonly AuthorsGetAllUseCase _authorsGetAllUseCase;
        private readonly AuthorsGetByCriteriaUseCase _authorsGetByCriteriaUseCase;
        private readonly AuthorGetByIdUseCase _authorsGetByIdUseCase;
        private readonly AuthorPostUseCase _authorPostUseCase;
        private readonly AuthorPostWithImageUseCase _authorPostWithImageUseCase;
        private readonly AuthorPutUseCase _authorPutUseCase;
        private readonly AuthorPatchUseCase _authorPatchUseCase;
        private readonly DeleteAuthorUseCase _deleteAuthorUseCase;

        public AuthorsController(AuthorsGetAllUseCase authorsGetAllUseCase, AuthorsGetByCriteriaUseCase authorsGetByCriteriaUseCase, AuthorGetByIdUseCase authorGetByIdUseCase,
            AuthorPostUseCase authorPostUseCase, AuthorPostWithImageUseCase authorPostWithImageUseCase, AuthorPutUseCase authorPutUseCase, AuthorPatchUseCase authorPatchUseCase,
            DeleteAuthorUseCase deleteAuthorUseCase)
        {
            _authorsGetAllUseCase = authorsGetAllUseCase;
            _authorsGetByCriteriaUseCase = authorsGetByCriteriaUseCase;
            _authorsGetByIdUseCase = authorGetByIdUseCase;
            _authorPostUseCase = authorPostUseCase;
            _authorPostWithImageUseCase = authorPostWithImageUseCase;
            _authorPutUseCase = authorPutUseCase;
            _authorPatchUseCase = authorPatchUseCase;
            _deleteAuthorUseCase = deleteAuthorUseCase;
        }

        [HttpGet(Name = "GetAuthorsV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [CacheTags.Authors])]
        [EndpointSummary("Retrieves a paginated list of authors.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AuthorDTO>>> Get([FromQuery] PaginationDTO paginationDTO)
        {
            var authorsDTO = await _authorsGetAllUseCase.Run(paginationDTO);
            return Ok(authorsDTO);
        }

        [HttpGet("filterV1", Name = "GetAuthorsByCriteriaV1")]
        [AllowAnonymous]
        [EndpointSummary("Retrieves a list of authors filtered by the specified criteria.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<object>>> GetByCriteria([FromQuery] AuthorFilterDTO authorFilterDTO)
        {
            var authorsObject = await _authorsGetByCriteriaUseCase.Run(authorFilterDTO);
            return Ok(authorsObject);
        }

        [HttpGet("{id:int}", Name = "GetAuthorV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [CacheTags.Authors])]
        [EndpointSummary("Retrieves a single author with their books by the specified author ID.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuthorWithBooksDTO>> Get([FromRoute][Description("Author Id")] int id)
        {
            var authorWithBooksDTO = await _authorsGetByIdUseCase.Run(id);
            if (authorWithBooksDTO is null)
                return NotFound();
            return Ok(authorWithBooksDTO);
        }

        [HttpPost(Name = "CreateAuthorV1")]
        [EndpointSummary("Creates a new author from the provided data.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status201Created)]
        public async Task<ActionResult> Post([FromBody] AuthorCreationDTO authorCreationDTO)
        {
            var authorDTO = await _authorPostUseCase.Run(authorCreationDTO);
            return CreatedAtRoute("GetAuthorV1", new { id = authorDTO.Id }, authorDTO);
        }

        [HttpPost("with-image", Name = "CreateAuthorWithImageV1")]
        [EndpointSummary("Creates a new author with an image from the provided data.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status201Created)]
        public async Task<ActionResult> PostWithImage([FromForm] AuthorCreationWithImageDTO authorCreationWithImageDTO)
        {
            var authorDTO = await _authorPostWithImageUseCase.Run(authorCreationWithImageDTO);
            return CreatedAtRoute("GetAuthorV1", new { id = authorDTO.Id }, authorDTO);
        }

        [HttpPut("{id:int}", Name = "UpdateAuthorV1")]
        [EndpointSummary("Updates an existing author with the provided data and image.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status204NoContent)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Put([FromRoute] int id, [FromForm] AuthorCreationWithImageDTO authorCreationWithImageDTO)
        {
            bool updated = await _authorPutUseCase.Run(id, authorCreationWithImageDTO);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        [HttpPatch("{id:int}", Name = "PatchAuthorV1")]
        [EndpointSummary("Partially updates an existing author with the provided patch document.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status204NoContent)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Patch([FromRoute] int id, [FromBody] JsonPatchDocument<AuthorPatchDTO> patchDocument)
        {
            bool? updated = await _authorPatchUseCase.Run(id, patchDocument, ModelState);
            if (updated is null)
                return BadRequest();
            if (!(bool)updated)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "DeleteAuthorV1")]
        [EndpointSummary("Deletes an author by the specified author ID.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status204NoContent)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            bool deleted = await _deleteAuthorUseCase.Run(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}