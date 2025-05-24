using LibraryAPI.Constants;
using LibraryAPI.DTOs;
using LibraryAPI.UseCases.Comments.Delete;
using LibraryAPI.UseCases.Comments.GetByBookId;
using LibraryAPI.UseCases.Comments.GetById;
using LibraryAPI.UseCases.Comments.Patch;
using LibraryAPI.UseCases.Comments.Post;
using LibraryAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.ComponentModel;

namespace LibraryAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/books/{bookId:int}/[controller]")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentDeleteUseCase _commentDeleteUseCase;
        private readonly ICommentGetByBookIdUseCase _commentGetByBookIdUseCase;
        private readonly ICommentGetByIdUseCase _commentGetByIdUseCase;
        private readonly ICommentPatchUseCase _commentPatchUseCase;
        private readonly ICommentPostUseCase _commentPostUseCase;


        public CommentsController(ICommentDeleteUseCase commentDeleteUseCase, ICommentGetByBookIdUseCase commentGetByBookIdUseCase, ICommentGetByIdUseCase commentGetByIdUseCase,
            ICommentPatchUseCase commentPatchUseCase, ICommentPostUseCase commentPostUseCase)
        {
            _commentDeleteUseCase = commentDeleteUseCase;
            _commentGetByBookIdUseCase = commentGetByBookIdUseCase;
            _commentGetByIdUseCase = commentGetByIdUseCase;
            _commentPatchUseCase = commentPatchUseCase;
            _commentPostUseCase = commentPostUseCase;
        }

        [HttpGet(Name = "GetCommentsV1")]
        [AllowAnonymous]
        [OutputCache(Tags = new[] { CacheTags.Comments })]
        [EndpointSummary("Retrieves a list of comments for the specified book.")]
        [ProducesResponseType(typeof(List<CommentDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<CommentDTO>>> Get(int bookId)
        {
            var commentsDTO = await _commentGetByBookIdUseCase.Run(bookId);
            if (commentsDTO is null)
                return NotFound();
            return Ok(commentsDTO);
        }

        [HttpGet("{id:guid}", Name = "GetCommentV1")]
        [AllowAnonymous]
        [OutputCache(Tags = new[] { CacheTags.Comments })]
        [EndpointSummary("Retrieves a single comment by its specified ID.")]
        [ProducesResponseType(typeof(CommentDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentDTO>> Get([FromRoute][Description("Comment Id")] Guid id)
        {
            var commentDTO = await _commentGetByIdUseCase.Run(id);
            if (commentDTO is null)
                return NotFound();
            return Ok(commentDTO);
        }

        [HttpPost(Name = "CreateCommentV1")]
        [EndpointSummary("Creates a new comment for the specified book.")]
        [ProducesResponseType(typeof(CommentDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Post(int bookId, [FromBody] CommentCreationDTO commentCreationDTO)
        {
            var commentDTO = await _commentPostUseCase.Run(bookId, commentCreationDTO);
            if (commentDTO is null)
                return NotFound();
            return CreatedAtRoute("GetCommentV1", new { id = commentDTO.Id, bookId }, commentDTO);
        }

        [HttpPatch("{id:guid}", Name = "PatchCommentV1")]
        [EndpointSummary("Partially updates a comment by its ID for the specified book.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Patch([FromRoute] Guid id, int bookId, [FromBody] JsonPatchDocument<CommentPatchDTO> patchDocument)
        {
            var result = await _commentPatchUseCase.Run(id, bookId, patchDocument, ModelState);

            return result.Type switch
            {
                ResultType.Success => NoContent(),
                ResultType.NotFound => NotFound(),
                ResultType.Forbidden => Forbid(),
                ResultType.BadRequest => BadRequest(),
                ResultType.ValidationError => ValidationProblem(),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        [HttpDelete("{id:guid}", Name = "DeleteCommentV1")]
        [EndpointSummary("Deletes a comment by its ID for the specified book.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete([FromRoute] Guid id, int bookId)
        {
            var result = await _commentDeleteUseCase.Run(id, bookId, ModelState);

            return result.Type switch
            {
                ResultType.Success => NoContent(),
                ResultType.NotFound => NotFound(),
                ResultType.Forbidden => Forbid(),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }
    }
}