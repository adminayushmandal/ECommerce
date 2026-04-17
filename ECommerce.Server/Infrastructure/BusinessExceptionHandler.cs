using Application.Common.Exceptions;
using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Server.Infrastructure;

public sealed class BusinessExceptionHandler(ILogger<BusinessExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BusinessLogicException && exception is not DomainException)
        {
            return false;
        }

        var statusCode = ResolveStatusCode(exception);

        logger.LogWarning(exception, "Handled business exception with status code {StatusCode}.", statusCode);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = exception.GetType().Name,
            Detail = exception.Message,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        httpContext.Response.StatusCode = statusCode;
        await Results.Problem(problemDetails).ExecuteAsync(httpContext);

        return true;
    }

    private static int ResolveStatusCode(Exception exception)
    {
        return exception switch
        {
            BusinessLogicException businessLogicException => businessLogicException.StatusCode,
            InsufficientInventoryException => StatusCodes.Status409Conflict,
            InvalidInventoryReservationQuantityException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };
    }
}
