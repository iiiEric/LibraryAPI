using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LibraryAPI.Utils
{
    public static class ResponseHelper
    {
        public static NotFoundObjectResult LogAndReturnNotFound(ILogger logger, string message, params object[] args)
        {
            var formattedMessage = FormatMessage(message, args);
            logger.LogWarning(formattedMessage);
            return new NotFoundObjectResult(new { message = formattedMessage });
        }

        public static BadRequestObjectResult LogAndReturnBadRequest(ILogger logger, string message, params object[] args)
        {
            var formattedMessage = FormatMessage(message, args);
            logger.LogWarning(formattedMessage);
            return new BadRequestObjectResult(new { message = formattedMessage });
        }

        public static ObjectResult LogAndReturnForbidden(ILogger logger, string message, params object[] args)
        {
            var formattedMessage = FormatMessage(message, args);
            logger.LogWarning(formattedMessage);
            return new ObjectResult(new { message = formattedMessage })
            {
                StatusCode = 403
            };
        }

        public static BadRequestObjectResult LogAndReturnValidationProblem(ILogger logger, string key, string message, ModelStateDictionary modelState, params object[] args)
        {
            var formattedMessage = FormatMessage(message, args);
            logger.LogWarning(formattedMessage);
            modelState.AddModelError(key, formattedMessage);
            return new BadRequestObjectResult(new ValidationProblemDetails(modelState));
        }

        public static CreatedAtRouteResult LogAndReturnCreatedAtRoute<T>(ILogger logger, string routeName, object routeValues, T value, string logMessage, params object[] args)
        {
            var formattedMessage = FormatMessage(logMessage, args);
            logger.LogInformation(formattedMessage);
            return new CreatedAtRouteResult(routeName, routeValues, value);
        }

        public static NoContentResult LogAndReturnNoContent(ILogger logger, string message, params object[] args)
        {
            var formattedMessage = FormatMessage(message, args);
            logger.LogInformation(formattedMessage);
            return new NoContentResult();
        }

        private static string FormatMessage(string message, object[] args)
        {
            return (args == null || args.Length == 0) ? message : string.Format(message, args);
        }
    }
}