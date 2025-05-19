using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Authors.GetById
{
    public interface IAuthorGetByIdUseCase
    {
        public Task<AuthorWithBooksDTO?> Run(int authorId);
    }
}
