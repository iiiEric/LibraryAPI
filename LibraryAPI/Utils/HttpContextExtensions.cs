using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Utils
{
    public static class HttpContextExtensions
    {
        public async static Task InsertHeaderPaginationParameters<T>(this HttpContext httpContext, IQueryable<T> queryable)
        {
            if (httpContext is null)
                throw new ArgumentNullException(nameof(httpContext));

            double count = await queryable.CountAsync();
            httpContext.Response.Headers.Append("total-number-of-records", count.ToString());
        }
    }
}
