using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Authors.GetByCriteria
{
    public interface IAuthorsGetByCriteriaUseCase
    {
        public Task<IEnumerable<object>> Run(AuthorFilterDTO authorFilterDTO);
    }
}
