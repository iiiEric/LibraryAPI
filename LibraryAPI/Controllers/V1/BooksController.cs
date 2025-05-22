using LibraryAPI.Constants;
using LibraryAPI.DTOs;
using LibraryAPI.UseCases.Books.Delete;
using LibraryAPI.UseCases.Books.GetAll;
using LibraryAPI.UseCases.Books.GetById;
using LibraryAPI.UseCases.Books.Post;
using LibraryAPI.UseCases.Books.Put;
using LibraryAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.ComponentModel;

namespace LibraryAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Policy = "Admin")]
    public class BooksController : ControllerBase
    {
        private readonly IBooksGetAllUseCase _booksGetAllUseCase;
        private readonly IBookGetByIdUseCase _booksGetByIdUseCase;
        private readonly IBookPostUseCase _bookPostUseCase;
        private readonly IBookPutUseCase _bookPutUseCase;
        private readonly IBookDeleteUseCase _bookDeleteUseCase;

        public BooksController(IBooksGetAllUseCase booksGetAllUseCase, IBookGetByIdUseCase bookGetByIdUseCase, IBookPostUseCase bookPostUseCase,
            IBookPutUseCase bookPutUseCase, IBookDeleteUseCase bookDeleteUseCase)
        {
             _booksGetAllUseCase = booksGetAllUseCase;
            _booksGetByIdUseCase = bookGetByIdUseCase;
            _bookPostUseCase = bookPostUseCase;
            _bookPutUseCase = bookPutUseCase;
            _bookDeleteUseCase = bookDeleteUseCase;
        }

        [HttpGet(Name = "GetBooksV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [CacheTags.Books])]
        [EndpointSummary("Retrieves a paginated list of books.")]
        [ProducesResponseType(typeof(IEnumerable<BookDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookDTO>>> Get([FromQuery] PaginationDTO paginationDTO)
        {
            var booksDTO = await _booksGetAllUseCase.Run(HttpContext, paginationDTO);
            return Ok(booksDTO);
        }

        [HttpGet("{id:int}", Name = "GetBookV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [CacheTags.Books])]
        [EndpointSummary("Retrieves a single book with its authors by the specified book ID.")]
        [ProducesResponseType(typeof(BookWithAuthorsDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookWithAuthorsDTO>> Get([FromRoute][Description("Book Id")] int id)
        {
            var bookWithAuthorsDTO = await _booksGetByIdUseCase.Run(id);
            if (bookWithAuthorsDTO is null)
                return NotFound();
            return Ok(bookWithAuthorsDTO);
        }

        [HttpPost(Name = "CreateBookV1")]
        [ServiceFilter<ValidateBookFilter>]
        [EndpointSummary("Creates a new book from the provided data.")]
        [ProducesResponseType(typeof(BookDTO), StatusCodes.Status201Created)]
        public async Task<ActionResult> Post([FromBody] BookCreationDTO bookCreationDTO)
        {
            var bookDTO = await _bookPostUseCase.Run(bookCreationDTO);
            return CreatedAtRoute("GetBookV1", new { id = bookDTO.Id }, bookDTO);
        }

        [HttpPut("{id:int}", Name = "UpdateBookV1")]
        [ServiceFilter<ValidateBookFilter>]
        [EndpointSummary("Updates an existing book with the provided data.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Put([FromRoute] int id, [FromBody] BookCreationDTO bookCreationDTO)
        {
            bool updated = await _bookPutUseCase.Run(id, bookCreationDTO);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "DeleteBookV1")]
        [EndpointSummary("Deletes a book by the specified book ID.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            bool deleted = await _bookDeleteUseCase.Run(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}