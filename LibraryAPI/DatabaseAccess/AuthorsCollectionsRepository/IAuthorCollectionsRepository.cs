using LibraryAPI.Entities;

namespace LibraryAPI.DatabaseAccess.AuthorsCollectionsRepository
{
    public interface IAuthorCollectionsRepository
    {
        Task<IEnumerable<Author>?> GetAll(List<int> collectionIds);

        Task<bool> AddRange(IEnumerable<Author> authors);
    }
}
