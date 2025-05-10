using LibraryAPI.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace LibraryAPI.Services
{
    public class UsersService : IUsersService
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersService(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            this._userManager = userManager;
            this._httpContextAccessor = httpContextAccessor;
        }

        public async Task<User?> GetCurrentUser()
        {
            var emailClaim = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
            if (emailClaim is null)
                return null;

            return await _userManager.FindByEmailAsync(emailClaim.Value);
        }
    }
}
