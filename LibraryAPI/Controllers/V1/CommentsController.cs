using AutoMapper;
using LibraryAPI.Constants;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.UseCases.Comments.Delete;
using LibraryAPI.UseCases.Comments.GetByBookId;
using LibraryAPI.UseCases.Comments.GetById;
using LibraryAPI.UseCases.Comments.Patch;
using LibraryAPI.UseCases.Comments.Post;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
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
        [OutputCache(Tags = [CacheTags.Comments])]
        [EndpointSummary("Retrieves a list of comments for the specified book.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)] //Corregir ProducesResponseType!!!!!!
        public async Task<ActionResult<List<CommentDTO>>> Get(int bookId)
        {
            var commentsDTO = await _commentGetByBookIdUseCase.Run(bookId);
            if (commentsDTO is null)
                return NotFound();
            return Ok(commentsDTO);
        }

        [HttpGet("{id:guid}", Name = "GetCommentV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [CacheTags.Comments])]
        [EndpointSummary("Retrieves a single comment by its specified ID.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentDTO>> Get([FromRoute][Description("Comment Id")] Guid id)
        {
            var comment = await _context.Comments
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (comment is null)
                return LogAndReturnNotFound(_logger, $"Comment with ID {id} was not found.");

            var commentDTO = _mapper.Map<CommentDTO>(comment);
            return Ok(commentDTO);
        }

        [HttpPost(Name = "CreateCommentV1")]
        [EndpointSummary("Creates a new comment for the specified book.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status201Created)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Post(int bookId, [FromBody] CommentCreationDTO commentCreationDTO)
        {
            var existsBook = await _context.Books.AnyAsync(x => x.Id == bookId);
            if (!existsBook)
                return LogAndReturnNotFound(_logger, $"Book with ID {bookId} was not found.");

            var user = await _usersServicies.GetCurrentUser();
            var comment = _mapper.Map<Comment>(commentCreationDTO);
            comment.BookId = bookId;
            comment.PublicationDate = DateTime.UtcNow;
            comment.UserId = user!.Id;

            _context.Add(comment);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(_cache, default);

            var commentDTO = _mapper.Map<CommentDTO>(comment);

            return CreatedAtRoute("GetCommentV1", new { id = comment.Id, bookId }, commentDTO);
        }

        [HttpPatch("{id:guid}", Name = "PatchCommentV1")]
        [EndpointSummary("Partially updates a comment by its ID for the specified book.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status204NoContent)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Patch([FromRoute] Guid id, int bookId, [FromBody] JsonPatchDocument<CommentPatchDTO> patchDocument)
        {
            var existsBook = await _context.Books.AnyAsync(x => x.Id == bookId);
            if (!existsBook)
                return LogAndReturnNotFound(_logger, $"Book with ID {bookId} was not found.");

            if (patchDocument is null)
                return LogAndReturnValidationProblem(_logger, nameof(patchDocument), "Patch document is null.", ModelState);

            var comment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == id);
            if (comment is null)
                return LogAndReturnNotFound(_logger, $"Comment with ID {id} was not found.");

            var user = await _usersServicies.GetCurrentUser();

            if (user!.Id != comment.UserId)
                return LogAndReturnForbidden(_logger, $"User with ID {user.Id} is not authorized to modify comment with ID {id}.");

            var commentPatchDTO = _mapper.Map<CommentPatchDTO>(comment);
            patchDocument.ApplyTo(commentPatchDTO, ModelState);

            var isValid = TryValidateModel(commentPatchDTO);
            if (!isValid)
                return LogAndReturnValidationProblem(_logger, nameof(id), $"Comment with ID {id} failed validation.", ModelState);

            _mapper.Map(commentPatchDTO, comment);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(_cache, default);

            return LogAndReturnNoContent(_logger, $"Comment with ID {id} patched successfully.");
        }

        [HttpDelete("{id:guid}", Name = "DeleteCommentV1")]
        [EndpointSummary("Deletes a comment by its ID for the specified book.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status204NoContent)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete([FromRoute] Guid id, int bookId)
        {
            var existsBook = await _context.Books.AnyAsync(x => x.Id == bookId);
            if (!existsBook)
                return LogAndReturnNotFound(_logger, $"Book with ID {bookId} was not found.");

            var user = await _usersServicies.GetCurrentUser();
            var comment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == id);
            if (comment is null)
                return LogAndReturnNotFound(_logger, $"Comment with ID {id} was not found.");

            if (user!.Id != comment.UserId)
                return LogAndReturnForbidden(_logger, $"User with ID {user.Id} is not authorized to delete comment with ID {id}.");

            comment.IsDeleted = true;
            _context.Update(comment);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(_cache, default);

            return LogAndReturnNoContent(_logger, $"Comment with ID {id} deleted successfully.");
        }
    }
}