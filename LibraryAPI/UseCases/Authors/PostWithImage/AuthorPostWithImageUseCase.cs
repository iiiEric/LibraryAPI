using AutoMapper;
using LibraryAPI.Constants;
using LibraryAPI.DatabaseAccess.AuthorsRepository;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Services;
using Microsoft.AspNetCore.OutputCaching;

namespace LibraryAPI.UseCases.Authors.PostWithImage
{
    public class AuthorPostWithImageUseCase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorPostWithImageUseCase> _logger;
        private readonly IOutputCacheStore _outputCacheStore;
        private readonly IConfiguration _configuration;
        private readonly IFileStorageService _fileStorageService;

        public AuthorPostWithImageUseCase(IAuthorRepository authorRepository, IMapper mapper, ILogger<AuthorPostWithImageUseCase> logger, IOutputCacheStore outputCacheStore, 
            IConfiguration configuration, IFileStorageService fileStorageService)
        {
            _authorRepository = authorRepository;
            _mapper = mapper;
            _logger = logger;
            _outputCacheStore = outputCacheStore;
            _configuration = configuration;
            _fileStorageService = fileStorageService;
        }

        public async Task<AuthorDTO> Run(AuthorCreationWithImageDTO authorCreationWithImageDTO)
        {
            _logger.LogInformation("Creating author with name '{Name}'", authorCreationWithImageDTO.Name);

            var author = _mapper.Map<Author>(authorCreationWithImageDTO);

            if (authorCreationWithImageDTO.Image is not null)
                author.ImageUrl = await _fileStorageService.Store(StorageContainers.Authors, authorCreationWithImageDTO.Image);

            await _authorRepository.Add(author);
            _logger.LogInformation($"Author with ID {author.Id} created successfully.");

            var authorDTO = _mapper.Map<AuthorDTO>(author);

            return authorDTO;
        }
    }
}
