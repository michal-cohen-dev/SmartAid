
using BLL.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SmartAid.API.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(
        ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var ex = context.Exception;
        var traceId = context.HttpContext.TraceIdentifier;

        _logger.LogError(
            ex,
            "Unhandled exception at {Path}. TraceId: {TraceId}",
            context.HttpContext.Request.Path,
            traceId);

        int status = ex switch
        {
            NotFoundException => 404,
            ConflictException => 409,
            BusinessRuleException => 400,
            InvalidCredentialsException => 401,
            ArgumentException => 400,
            _ => 500
        };

        string message = status == 500
            ? "An unexpected error occurred."
            : ex.Message;

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = message
        };

        if (status == 500)
        {
            problemDetails.Extensions["traceId"] = traceId;
        }

        context.Result = new ObjectResult(problemDetails)
        {
            StatusCode = status
        };

        context.ExceptionHandled = true;
    }
}
