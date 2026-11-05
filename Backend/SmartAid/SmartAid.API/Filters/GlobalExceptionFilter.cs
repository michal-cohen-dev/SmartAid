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

        _logger.LogError(
            ex,
            "Unhandled exception at {Path}",
            context.HttpContext.Request.Path);

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

        context.Result = new ObjectResult(
            new ProblemDetails
            {
                Status = status,
                Title = message
            })
        {
            StatusCode = status
        };

        context.ExceptionHandled = true;
    }
}