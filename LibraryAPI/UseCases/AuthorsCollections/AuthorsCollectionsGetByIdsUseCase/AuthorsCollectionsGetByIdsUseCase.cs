using AutoMapper;
using LibraryAPI.DatabaseAccess.AuthorsCollectionsRepository;
using LibraryAPI.DatabaseAccess.AuthorsRepository;
using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.AuthorsCollections.AuthorsGetByIdsUseCase
{
    public class AuthorsCollectionsGetByIdsUseCase : IAuthorsCollectionsGetByIdsUseCase
    {
        private readonly IAuthorCollectionsRepository _authorCollectionsRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorsCollectionsGetByIdsUseCase> _logger;

        public AuthorsCollectionsGetByIdsUseCase(IAuthorCollectionsRepository authorCollectionsRepository, IMapper mapper, ILogger<AuthorsCollectionsGetByIdsUseCase> logger)
        {
            _authorCollectionsRepository = authorCollectionsRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<AuthorWithBooksDTO>?> Run(string ids)
        {
            _logger.LogInformation("Retrieving authors with IDs {AuthorsIds}", ids);

            var collectionIds = ids
                .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .ToList();

            if (!collectionIds.Any())
            {
                _logger.LogWarning("No valid author IDs provided.");
                return null;
            }

            var authors = await _authorCollectionsRepository.GetAll(collectionIds);

            if (authors!.Count() != collectionIds.Count)
            {
                var existingIds = authors!.Select(x => x.Id);
                var missingIds = collectionIds.Except(existingIds);
                var missingIdsString = string.Join(", ", missingIds);
                _logger.LogWarning($"Some author IDs were not found: {missingIdsString}");
                return null;
            }

            var authorsDTO = _mapper.Map<List<AuthorWithBooksDTO>>(authors);

            _logger.LogInformation("Authors with IDs {AuthorsIds} retrieved successfully.", ids);
            return authorsDTO;
        }
    }

}
