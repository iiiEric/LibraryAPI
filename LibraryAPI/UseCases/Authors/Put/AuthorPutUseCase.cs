using AutoMapper;
using LibraryAPI.Constants;
using LibraryAPI.DatabaseAccess.AuthorsRepository;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Services;
using Microsoft.AspNetCore.OutputCaching;

namespace LibraryAPI.UseCases.Authors.Put
{
    public class AuthorPutUseCase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorPutUseCase> _logger;
        private readonly IOutputCacheStore _outputCacheStore;
        private readonly IConfiguration _configuration;
        private readonly IFileStorageService _fileStorageService;

        public AuthorPutUseCase(IAuthorRepository authorRepository, IMapper mapper, ILogger<AuthorPutUseCase> logger, IOutputCacheStore outputCacheStore,
            IConfiguration configuration, IFileStorageService fileStorageService)
        {
            _authorRepository = authorRepository;
            _mapper = mapper;
            _logger = logger;
            _outputCacheStore = outputCacheStore;
            _configuration = configuration;
            _fileStorageService = fileStorageService;
        }

        public async Task<bool> Run(int authorId, AuthorCreationWithImageDTO authorCreationWithImageDTO)
        {
            _logger.LogInformation("Updating author with ID '{ID}'", authorId);

            var exists = await _authorRepository.Exists(authorId);
            if (!exists)
            {
                _logger.LogWarning($"Author with ID {authorId} was not found.");
                return false;
            }

            var author = _mapper.Map<Author>(authorCreationWithImageDTO);
            author.Id = authorId;

            if (authorCreationWithImageDTO.Image is not null)
            {
                var currentImage = await _authorRepository.GetImageUrl(authorId);

                var imageUrl = await _fileStorageService.Update(currentImage, StorageContainers.Authors, authorCreationWithImageDTO.Image);
                author.ImageUrl = imageUrl;
            }

            await _authorRepository.Update(author);
            _logger.LogInformation($"Author with ID {author.Id} updated successfully.");
            return true;
        }
    }
}
