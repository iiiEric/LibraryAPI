using LibraryAPI.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController: ControllerBase
    {
        [HttpGet]
        public IEnumerable<Author> GET()
        {
            return new List<Author> {
                new Author
                {
                    Id = 1,
                    Name = "J.K. Rowling"
                },
                new Author
                {
                    Id = 2,
                    Name = "J.R.R. Tolkien"
                }
            };
        }
    }
}
