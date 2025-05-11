using LibraryAPI.Controllers;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Utils
{
    public class ValidateBookFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ValidateBookFilter> _logger;

        public ValidateBookFilter(ApplicationDbContext context, ILogger<ValidateBookFilter> logger)
        {
            this._context = context;
            this._logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext AEcontext, ActionExecutionDelegate next)
        {
            if (!AEcontext.ActionArguments.TryGetValue("bookCreationDTO", out var value) || value is not BookCreationDTO bookCreationDTO)
            {
                AEcontext.ModelState.AddModelError(string.Empty, "Invalid model.");
                AEcontext.Result = AEcontext.ModelState.BuildProblemDetails();
                return;
            }

            if (bookCreationDTO.AuthorsIds is null || bookCreationDTO.AuthorsIds.Count == 0)
            {
                _logger.LogWarning("Failed to create book. No authors provided.");
                AEcontext.ModelState.AddModelError(nameof(bookCreationDTO.AuthorsIds), "At least one author ID is required.");
                AEcontext.Result = AEcontext.ModelState.BuildProblemDetails();
                return;
            }

            var existsAuthorsIds = await _context.Authors
                .Where(x => bookCreationDTO.AuthorsIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            if (existsAuthorsIds.Count != bookCreationDTO.AuthorsIds.Count)
            {
                var authorsNotExists = bookCreationDTO.AuthorsIds.Except(existsAuthorsIds);
                var authorsNotExistsString = string.Join(", ", authorsNotExists);
                _logger.LogWarning("Failed to create book. Some author IDs do not exist: {AuthorsNotExistsIds}", authorsNotExistsString);
                AEcontext.ModelState.AddModelError(nameof(bookCreationDTO.AuthorsIds), $"Some author IDs do not exist: {authorsNotExistsString}");
                AEcontext.Result = AEcontext.ModelState.BuildProblemDetails();
                return;
            }

            await next();
        }
    }
}
