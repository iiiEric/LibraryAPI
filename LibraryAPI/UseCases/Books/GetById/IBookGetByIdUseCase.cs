using LibraryAPI.DTOs;
using LibraryAPI.Entities;

namespace LibraryAPI.UseCases.Books.GetById
{
    public interface IBookGetByIdUseCase
    {
        public Task<BookWithAuthorsDTO?> Run(int bookId);
    }
}
