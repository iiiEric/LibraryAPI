using LibraryAPI.Constants;
using LibraryAPI.DatabaseAccess.AuthorsRepository;
using LibraryAPI.Services;
using Microsoft.AspNetCore.OutputCaching;

namespace LibraryAPI.UseCases.Authors.Delete
{
    public class DeleteAuthorUseCase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILogger<DeleteAuthorUseCase> _logger;
        private readonly IOutputCacheStore _outputCacheStore;
        private readonly IConfiguration _configuration;
        private readonly IFileStorageService _fileStorageService;

        public DeleteAuthorUseCase(IAuthorRepository authorRepository, ILogger<DeleteAuthorUseCase> logger, IOutputCacheStore outputCacheStore,
            IConfiguration configuration, IFileStorageService fileStorageService)
        {
            _authorRepository = authorRepository;
            _logger = logger;
            _outputCacheStore = outputCacheStore;
            _configuration = configuration;
            _fileStorageService = fileStorageService;
        }

        public async Task<bool> Run(int authorId)
        {
            _logger.LogInformation("Attempting to delete author with ID {AuthorId}", authorId);

            var author = await _authorRepository.GetById(authorId);
            if (author is null)
            {
                _logger.LogWarning($"Author with ID {authorId} was not found.");
                return false;
            }

            await _authorRepository.Remove(author);
            _logger.LogWarning($"Author with ID {authorId} deleted successfully.");
            await _fileStorageService.Delete(author.ImageUrl, StorageContainers.Authors);
            return true;
        }
    }
}
