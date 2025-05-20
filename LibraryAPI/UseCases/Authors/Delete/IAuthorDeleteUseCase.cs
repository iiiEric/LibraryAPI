namespace LibraryAPI.UseCases.Authors.Delete
{
    public interface IAuthorDeleteUseCase
    {
        public Task<bool> Run(int authorId);
    }
}
