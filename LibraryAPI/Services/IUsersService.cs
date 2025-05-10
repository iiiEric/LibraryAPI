using LibraryAPI.Entities;
using Microsoft.AspNetCore.Identity;

namespace LibraryAPI.Services
{
    public interface IUsersService
    {
        Task<User?> GetCurrentUser();
    }
}