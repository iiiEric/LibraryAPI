using AutoMapper;
using LibraryAPI.DatabaseAccess.AuthorsRepository;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using Microsoft.AspNetCore.OutputCaching;

namespace LibraryAPI.UseCases.Authors.Post
{
    public class AuthorPostUseCase : IAuthorPostUseCase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorPostUseCase> _logger;
        private readonly IOutputCacheStore _outputCacheStore;
        private readonly IConfiguration _configuration;

        public AuthorPostUseCase(IAuthorRepository authorRepository, IMapper mapper, ILogger<AuthorPostUseCase> logger, IOutputCacheStore outputCacheStore, IConfiguration configuration)
        {
            _authorRepository = authorRepository;
            _mapper = mapper;
            _logger = logger;
            _outputCacheStore = outputCacheStore;
            _configuration = configuration;
        }

        public async Task<AuthorDTO> Run(AuthorCreationDTO authorCreationDTO)
        {
            _logger.LogInformation("Creating author with name '{Name}'", authorCreationDTO.Name);

            var author = _mapper.Map<Author>(authorCreationDTO);

            await _authorRepository.Add(author);
            _logger.LogInformation($"Author with ID {author.Id} created successfully.");

            var authorDTO = _mapper.Map<AuthorDTO>(author);

            return authorDTO;
        }
    }
}
