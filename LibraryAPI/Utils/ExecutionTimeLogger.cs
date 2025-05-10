using LibraryAPI.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace LibraryAPI.Utils
{
    public class ExecutionTimeLogger : IAsyncActionFilter
    {
        private readonly ILogger<ExecutionTimeLogger> _logger;

        public ExecutionTimeLogger(ILogger<ExecutionTimeLogger> logger)
        {
            this._logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var stopWatch = Stopwatch.StartNew();
            _logger.LogInformation($"Execution started for {context.ActionDescriptor.DisplayName}");
            await next();

            stopWatch.Stop();
            _logger.LogInformation($"Execution finished for {context.ActionDescriptor.DisplayName} in {stopWatch.ElapsedMilliseconds} ms");
        }
    }
}
