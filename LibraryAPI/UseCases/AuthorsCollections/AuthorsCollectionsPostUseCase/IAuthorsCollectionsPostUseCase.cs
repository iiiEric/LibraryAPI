using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.AuthorsCollections.AuthorsCollectionsPostUseCase
{
    public interface IAuthorsCollectionsPostUseCase
    {
        public Task<IEnumerable<AuthorDTO>> Run(IEnumerable<AuthorCreationDTO> authorsCreationDTO);
    }
}
