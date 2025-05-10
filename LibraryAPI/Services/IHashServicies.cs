using LibraryAPI.DTOs;

namespace LibraryAPI.Services
{
    public interface IHashServicies
    {
        HashResultDTO Hash(string input);
        HashResultDTO Hash(string input, byte[] isalt);
    }
}