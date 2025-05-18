using AutoMapper;
using LibraryAPI.DatabaseAccess.AuthorsRepository;
using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Authors.GetById
{
    public class AuthorGetByIdUseCase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorGetByIdUseCase> _logger;

        public AuthorGetByIdUseCase(IAuthorRepository authorRepository, IMapper mapper, ILogger<AuthorGetByIdUseCase> logger)
        {
            _authorRepository = authorRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AuthorWithBooksDTO?> Run(int authorId)
        {
            _logger.LogInformation("Retrieving author with ID {AuthorId}", authorId);
            var author = await _authorRepository.GetById(authorId);

            if (author is null)
            {
                _logger.LogWarning($"Author with ID {authorId} was not found.");
                return null;
            }
               
            var authorWithBooksDTO = _mapper.Map<AuthorWithBooksDTO>(author);
            return authorWithBooksDTO;
        }
    }
}
