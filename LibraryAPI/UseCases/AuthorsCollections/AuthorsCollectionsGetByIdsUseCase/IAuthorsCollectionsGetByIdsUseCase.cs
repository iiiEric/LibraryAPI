using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.AuthorsCollections.AuthorsGetByIdsUseCase
{
    public interface IAuthorsCollectionsGetByIdsUseCase
    {
        public Task<List<AuthorWithBooksDTO>> Run(string ids);
    }
}
