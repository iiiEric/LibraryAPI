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
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Xml.Linq;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/books/{bookId:int}/[controller]")]
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
            this._context = context;
            this._mapper = mapper;
            this._logger = logger;
            this._usersServicies = usersServicies;
            this._outputCacheStore = outputCacheStore;
        }

        [HttpGet]
        [AllowAnonymous]
        [OutputCache(Tags = [_cache])]
        public async Task<ActionResult<List<CommentDTO>>> Get(int bookId)
        {
            _logger.LogInformation("Retrieving all comments of the book {BookId}", bookId);

            var existsBook = await _context.Books.AnyAsync(x => x.Id == bookId);
            if (!existsBook)
            {
                _logger.LogWarning("Book with ID {BookId} not found.", bookId);
                return NotFound();
            }

            var comments = await _context.Comments
                .Include(x => x.User)
                .Where(x => x.BookId == bookId)
                .OrderByDescending(x => x.PublicationDate)
                .ToListAsync();
            var commentsDTO = _mapper.Map<List<CommentDTO>>(comments);

            return Ok(commentsDTO);
        }

        [HttpGet("{id:guid}", Name = "GetComment")]
        [AllowAnonymous]
        [OutputCache(Tags = [_cache])]
        public async Task<ActionResult<CommentDTO>> Get([FromRoute] Guid id)
        {
            _logger.LogInformation("Retrieving comment with ID {CommentId}", id);

            var comment = await _context.Comments
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (comment is null)
            {
                _logger.LogWarning("Comment with ID {CommentId} not found.", id);
                return NotFound();
            }

            var commentDTO = _mapper.Map<CommentDTO>(comment);

            _logger.LogInformation("Comment with ID {CommentId} retrieved successfully.", id);
            return Ok(commentDTO);
        }

        [HttpPost]
        public async Task<ActionResult> Post(int bookId, [FromBody] CommentCreationDTO commentCreationDTO)
        {
            _logger.LogInformation("Creating comment for book ID {BookId}", bookId);

            var existsBook = await _context.Books.AnyAsync(x => x.Id == bookId);
            if (!existsBook)
            {
                _logger.LogWarning("Book with ID {BookId} not found.", bookId);
                ModelState.AddModelError(nameof(bookId), $"Book id {bookId} does not exist");
                return ValidationProblem();
            }

            var user = await _usersServicies.GetCurrentUser();
            //Unnecessary by having the [Authorize]
            //if (user is null)
            //{
            //    _logger.LogWarning("User not found.");
            //    ModelState.AddModelError(string.Empty, "User not found.");
            //    return ValidationProblem();
            //}

            var comment = _mapper.Map<Comment>(commentCreationDTO);
            comment.BookId = bookId;
            comment.PublicationDate = DateTime.UtcNow;
            comment.UserId = user!.Id;
            _context.Add(comment);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(_cache, default);

            var commentDTO = _mapper.Map<CommentDTO>(comment);

            _logger.LogInformation("Comment with ID {CommentId} created successfully.", comment.Id);
            return CreatedAtRoute("GetComment", new { id = comment.Id, bookId }, commentDTO);
        }

        [HttpPatch("{id:guid}")]
        public async Task<ActionResult> Patch([FromRoute] Guid id, int bookId, [FromBody] JsonPatchDocument<CommentPatchDTO> patchDocument)
        {
            _logger.LogInformation("Received PATCH request for comment with ID {CommentId}.", id);

            var existsBook = await _context.Books.AnyAsync(x => x.Id == bookId);
            if (!existsBook)
            {
                _logger.LogWarning("Book with ID {BookId} not found.", bookId);
                ModelState.AddModelError(nameof(bookId), $"Book id {bookId} does not exist");
                return ValidationProblem();
            }

            if (patchDocument is null)
            {
                _logger.LogWarning("PATCH request for comment with ID {CommentId} failed: patch document is null.", id);
                ModelState.AddModelError(nameof(patchDocument), "Patch document cannot be null.");
                return ValidationProblem();
            }

            var comment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == id);
            if (comment is null)
            {
                _logger.LogWarning("PATCH request failed: comment with ID {CommentId} not found.", id);
                ModelState.AddModelError(nameof(id), $"Comment with ID {id} not found.");
                return ValidationProblem();
            }

            var user = await _usersServicies.GetCurrentUser();
            //Unnecessary by having the [Authorize]
            //if (user is null)
            //{
            //    _logger.LogWarning("User not found.");
            //    ModelState.AddModelError(string.Empty, "User not found.");
            //    return ValidationProblem();
            //}

            if (user!.Id != comment.UserId)
            {
                _logger.LogWarning("User with ID {UserId} is not authorized to modify comment with ID {CommentId} of user {CommentUserId}.", user.Id, id, comment.UserId);
                return Forbid();
            }

            var commentPatchDTO = _mapper.Map<CommentPatchDTO>(comment);
            patchDocument.ApplyTo(commentPatchDTO, ModelState);

            var isValid = TryValidateModel(commentPatchDTO);
            if (!isValid)
            {
                _logger.LogWarning("PATCH request for comment with ID {CommentId} failed validation.", id);
                return ValidationProblem();
            }

            _mapper.Map(commentPatchDTO, comment);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(_cache, default);

            _logger.LogInformation("Comment with ID {CommentId} patched successfully.", id);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete([FromRoute] Guid id, int bookId)
        {
            _logger.LogInformation("Attempting to delete comment with ID {CommentId}", id);

            var existsBook = await _context.Books.AnyAsync(x => x.Id == bookId);
            if (!existsBook)
            {
                _logger.LogWarning("Book with ID {BookId} not found.", bookId);
                ModelState.AddModelError(nameof(bookId), $"Book id {bookId} does not exist");
                return ValidationProblem();
            }

            var user = await _usersServicies.GetCurrentUser();
            //Unnecessary by having the [Authorize]
            //if (user is null)
            //{
            //    _logger.LogWarning("User not found.");
            //    ModelState.AddModelError(string.Empty, "User not found.");
            //    return ValidationProblem();
            //}

            var comment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == id);
            if (comment is null)
            {
                _logger.LogWarning("Attempted to delete non-existing comment with ID {CommentId}", id);
                return NotFound();
            }

            if (user!.Id != comment.UserId)
            {
                _logger.LogWarning("User with ID {UserId} is not authorized to modify comment with ID {CommentId} of user {CommentUserId}.", user.Id, id, comment.UserId);
                return Forbid();
            }

            comment.IsDeleted = true;
            _context.Update(comment);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(_cache, default);

            _logger.LogInformation("Comment with ID {CommentId} deleted successfully.", id);
            return NoContent();
        }
    }
}
