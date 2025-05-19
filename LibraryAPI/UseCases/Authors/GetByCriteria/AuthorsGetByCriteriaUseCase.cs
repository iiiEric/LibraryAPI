using AutoMapper;
using LibraryAPI.DatabaseAccess.AuthorsRepository;
using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Authors.GetByCriteria
{
    public class AuthorsGetByCriteriaUseCase : IAuthorsGetByCriteriaUseCase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorsGetByCriteriaUseCase> _logger;

        public AuthorsGetByCriteriaUseCase(IAuthorRepository authorRepository, IMapper mapper, ILogger<AuthorsGetByCriteriaUseCase> logger)
        {
            _authorRepository = authorRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<object>> Run(AuthorFilterDTO authorFilterDTO)
        {
            _logger.LogInformation("Filtering authors.");
            var authors = await _authorRepository.GetByCriteria(authorFilterDTO);

            return authorFilterDTO.IncludeBooks
              ? _mapper.Map<IEnumerable<AuthorWithBooksDTO>>(authors)
              : _mapper.Map<IEnumerable<AuthorDTO>>(authors);
        }
    }
}
