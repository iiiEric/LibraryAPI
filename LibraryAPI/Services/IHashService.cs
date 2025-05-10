using LibraryAPI.DTOs;

namespace LibraryAPI.Services
{
    public interface IHashService
    {
        HashResultDTO Hash(string input);
        HashResultDTO Hash(string input, byte[] isalt);
    }
}