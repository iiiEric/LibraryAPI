using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.UseCases.AuthorsCollections.AuthorsCollectionsPostUseCase;
using LibraryAPI.UseCases.AuthorsCollections.AuthorsGetByIdsUseCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
namespace LibraryAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Policy = "Admin")]
    public class AuthorsCollectionController : ControllerBase
    {
        private readonly IAuthorsCollectionsGetByIdsUseCase _authorsGetByIdsUseCase;
        private readonly IAuthorsCollectionsPostUseCase _authorsCollectionsPostUseCase;

        public AuthorsCollectionController(IAuthorsCollectionsGetByIdsUseCase authorsGetByIdsUseCase, IAuthorsCollectionsPostUseCase authorsCollectionsPostUseCase)
        {
            _authorsGetByIdsUseCase = authorsGetByIdsUseCase;
            _authorsCollectionsPostUseCase = authorsCollectionsPostUseCase;
        }

        [HttpGet("{ids}", Name = "GetAuthorsByIdsV1")]
        [AllowAnonymous]
        [EndpointSummary("Retrieves a list of authors along with their books based on the provided author IDs.")]
        [ProducesResponseType(typeof(List<AuthorWithBooksDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<AuthorWithBooksDTO>>> Get([FromRoute][Description("Author Ids")] string ids)
        {
            var authorWithBooksDTO = await _authorsGetByIdsUseCase.Run(ids);
            if (authorWithBooksDTO is null)
                return BadRequest();
            return Ok(authorWithBooksDTO);
        }

        [HttpPost(Name = "CreateAuthorsV1")]
        [EndpointSummary("Creates multiple authors from the provided data.")]
        [ProducesResponseType(typeof(IEnumerable<AuthorWithBooksDTO>), StatusCodes.Status201Created)]
        public async Task<ActionResult> Post([FromBody] IEnumerable<AuthorCreationDTO> authorsCreationDTO)
        {
            var authorsDTO = await _authorsCollectionsPostUseCase.Run(authorsCreationDTO);
            var ids = authorsDTO.Select(x => x.Id);
            var idsString = string.Join(", ", ids);
            return CreatedAtRoute("GetAuthorsByIdsV1", new { ids = idsString }, authorsDTO);
        }
    }
}