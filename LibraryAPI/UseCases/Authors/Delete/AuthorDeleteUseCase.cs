using LibraryAPI.Constants;
using LibraryAPI.DatabaseAccess.AuthorsRepository;
using LibraryAPI.Services;
using LibraryAPI.Utils;

namespace LibraryAPI.UseCases.Authors.Delete
{
    public class AuthorDeleteUseCase : IAuthorDeleteUseCase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILogger<AuthorDeleteUseCase> _logger;
        private readonly IFileStorageService _fileStorageService;

        public AuthorDeleteUseCase(IAuthorRepository authorRepository, ILogger<AuthorDeleteUseCase> logger, IFileStorageService fileStorageService)
        {
            _authorRepository = authorRepository;
            _logger = logger;
            _fileStorageService = fileStorageService;
        }

        public async Task<Result> Run(int authorId)
        {
            _logger.LogInformation("Attempting to delete author with ID {AuthorId}", authorId);

            var author = await _authorRepository.GetById(authorId);
            if (author is null)
            {
                _logger.LogWarning($"Author with ID {authorId} was not found.");
                return Result.NotFound();
            }

            await _authorRepository.Remove(author);
            await _fileStorageService.Delete(author.ImageUrl, StorageContainers.Authors);

            _logger.LogInformation($"Author with ID {authorId} deleted successfully.");
            return Result.Success();
        }
    }
}
