namespace LibraryAPI.UseCases.Books.Delete
{
    public interface IBookDeleteUseCase
    {
        public Task<bool> Run(int bookId);
    }
}
