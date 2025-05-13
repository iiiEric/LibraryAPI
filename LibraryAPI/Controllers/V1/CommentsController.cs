using AutoMapper;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using static LibraryAPI.Utils.ResponseHelper;

namespace LibraryAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/books/{bookId:int}/[controller]")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<BooksController> _logger;
        private readonly IUsersService _usersServicies;
        private readonly IOutputCacheStore _outputCacheStore;
        private const string _cache = "CommentsCache";

        public CommentsController(ApplicationDbContext context, IMapper mapper, ILogger<BooksController> logger, IUsersService usersServicies, IOutputCacheStore outputCacheStore)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _usersServicies = usersServicies;
            _outputCacheStore = outputCacheStore;
        }

        [HttpGet(Name = "GetCommentsV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [_cache])]
        [EndpointSummary("Retrieves a list of comments for the specified book.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<CommentDTO>>> Get(int bookId)
        {
            var existsBook = await _context.Books.AnyAsync(x => x.Id == bookId);
            if (!existsBook)
                return LogAndReturnNotFound(_logger, "Book with ID {BookId} was not found.", bookId);

            var comments = await _context.Comments
                .Include(x => x.User)
                .Where(x => x.BookId == bookId)
                .OrderByDescending(x => x.PublicationDate)
                .ToListAsync();
            var commentsDTO = _mapper.Map<List<CommentDTO>>(comments);

            return Ok(commentsDTO);
        }

        [HttpGet("{id:guid}", Name = "GetCommentV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [_cache])]
        [EndpointSummary("Retrieves a single comment by its specified ID.")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentDTO>> Get([FromRoute][Description("Comment Id")] Guid id)
        {
            var comment = await _context.Comments
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (comment is null)
                return LogAndReturnNotFound(_logger, "Comment with ID {CommentId} was not found.", id);

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
                return LogAndReturnNotFound(_logger, "Book with ID {BookId} was not found.", bookId);

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
                return LogAndReturnNotFound(_logger, "Book with ID {BookId} was not found.", bookId);

            if (patchDocument is null)
                return LogAndReturnValidationProblem(_logger, nameof(patchDocument), "Patch document is null.", ModelState);

            var comment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == id);
            if (comment is null)
                return LogAndReturnNotFound(_logger, "Comment with ID {CommentId} was not found.", id);

            var user = await _usersServicies.GetCurrentUser();

            if (user!.Id != comment.UserId)
                return LogAndReturnForbidden(_logger, "User with ID {UserId} is not authorized to modify comment with ID {CommentId}.", user.Id, id);

            var commentPatchDTO = _mapper.Map<CommentPatchDTO>(comment);
            patchDocument.ApplyTo(commentPatchDTO, ModelState);

            var isValid = TryValidateModel(commentPatchDTO);
            if (!isValid)
                return LogAndReturnValidationProblem(_logger, nameof(id), $"Comment with ID {id} failed validation.", ModelState);

            _mapper.Map(commentPatchDTO, comment);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(_cache, default);

            return LogAndReturnNoContent(_logger, "Comment with ID {CommentId} patched successfully.", id);
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
                return LogAndReturnNotFound(_logger, "Book with ID {BookId} was not found.", bookId);

            var user = await _usersServicies.GetCurrentUser();
            var comment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == id);
            if (comment is null)
                return LogAndReturnNotFound(_logger, "Comment with ID {CommentId} was not found.", id);

            if (user!.Id != comment.UserId)
                return LogAndReturnForbidden(_logger, "User with ID {UserId} is not authorized to delete comment with ID {CommentId}.", user.Id, id);

            comment.IsDeleted = true;
            _context.Update(comment);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(_cache, default);

            return LogAndReturnNoContent(_logger, "Comment with ID {CommentId} deleted successfully.", id);
        }
    }
}