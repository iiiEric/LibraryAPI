using Microsoft.AspNetCore.Identity;

namespace LibraryAPI.Entities
{
    public class User: IdentityUser
    {
        public DateTime DateOfBirth { get; set; }
    }
}
