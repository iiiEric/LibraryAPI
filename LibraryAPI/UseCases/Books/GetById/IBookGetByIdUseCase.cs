using LibraryAPI.DTOs;
using LibraryAPI.Entities;

namespace LibraryAPI.UseCases.Books.GetById
{
    public interface IBookGetByIdUseCase
    {
       Task<BookWithAuthorsDTO?> Run(int bookId);
    }
}
