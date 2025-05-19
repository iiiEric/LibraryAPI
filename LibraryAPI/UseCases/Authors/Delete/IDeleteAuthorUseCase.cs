namespace LibraryAPI.UseCases.Authors.Delete
{
    public interface IDeleteAuthorUseCase
    {
        public Task<bool> Run(int authorId);
    }
}
