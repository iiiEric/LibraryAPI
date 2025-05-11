using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace LibraryAPITests.Utils
{
    public class FakeUserFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim("email", "example@gmail.com")
            }, "test"));

            await next();
        }
    }
}
