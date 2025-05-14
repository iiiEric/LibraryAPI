using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LibraryAPI.Utils
{
    public static class ResponseHelper
    {
        public static NotFoundObjectResult LogAndReturnNotFound(ILogger logger, string message)
        {
            logger.LogWarning(message);
            return new NotFoundObjectResult(new { message });
        }

        public static BadRequestObjectResult LogAndReturnBadRequest(ILogger logger, string message)
        {
            logger.LogWarning(message);
            return new BadRequestObjectResult(new { message });
        }

        public static ObjectResult LogAndReturnForbidden(ILogger logger, string message)
        {
            logger.LogWarning(message);
            return new ObjectResult(new { message })
            {
                StatusCode = 403
            };
        }

        public static BadRequestObjectResult LogAndReturnValidationProblem(ILogger logger, string key, string message, ModelStateDictionary modelState)
        {
            logger.LogWarning(message);
            modelState.AddModelError(key, message);
            return new BadRequestObjectResult(new ValidationProblemDetails(modelState));
        }

        public static CreatedAtRouteResult LogAndReturnCreatedAtRoute<T>(ILogger logger, string routeName, object routeValues, T value, string message)
        {
            logger.LogInformation(message);
            return new CreatedAtRouteResult(routeName, routeValues, value);
        }

        public static NoContentResult LogAndReturnNoContent(ILogger logger, string message)
        {
            logger.LogInformation(message);
            return new NoContentResult();
        }
    }
}