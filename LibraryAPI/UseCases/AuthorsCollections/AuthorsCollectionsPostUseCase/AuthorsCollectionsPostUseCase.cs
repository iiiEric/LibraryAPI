using AutoMapper;
using LibraryAPI.DatabaseAccess.AuthorsCollectionsRepository;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;

namespace LibraryAPI.UseCases.AuthorsCollections.AuthorsCollectionsPostUseCase
{
    public class AuthorsCollectionsPostUseCase : IAuthorsCollectionsPostUseCase
    {
        private readonly IAuthorCollectionsRepository _authorCollectionsRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorsCollectionsPostUseCase> _logger;

        public AuthorsCollectionsPostUseCase(IAuthorCollectionsRepository authorCollectionsRepository, IMapper mapper, ILogger<AuthorsCollectionsPostUseCase> logger)
        {
            _authorCollectionsRepository = authorCollectionsRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<AuthorDTO>> Run(IEnumerable<AuthorCreationDTO> authorsCreationDTO)
        {
            var authors = _mapper.Map<IEnumerable<Author>>(authorsCreationDTO);

            foreach (var author in authors)
                _logger.LogInformation("Creating author with name '{Name}'", author.Name);

            await _authorCollectionsRepository.AddRange(authors);

            foreach (var author in authors)
                _logger.LogInformation("Author with ID {AuthorId} created successfully.", author.Id);

            var authorsDTO = _mapper.Map<IEnumerable<AuthorDTO>>(authors);
            return authorsDTO;
        }
    }
}
