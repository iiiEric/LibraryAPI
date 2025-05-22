using AutoMapper;
using LibraryAPI.DatabaseAccess.AuthorsRepository;
using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Authors.GetAll
{
    public class AuthorsGetAllUseCase : IAuthorsGetAllUseCase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorsGetAllUseCase> _logger;

        public AuthorsGetAllUseCase(IAuthorRepository authorRepository, IMapper mapper, ILogger<AuthorsGetAllUseCase> logger)
        {
            _authorRepository = authorRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<AuthorDTO>> Run(HttpContext httpContext, PaginationDTO paginationDTO)
        {
            _logger.LogInformation("Retrieving all authors.");
            var authors = await _authorRepository.GetAll(httpContext, paginationDTO);
            var authorsDTO = _mapper.Map<IEnumerable<AuthorDTO>>(authors);
            return authorsDTO;
        }
    }
}
