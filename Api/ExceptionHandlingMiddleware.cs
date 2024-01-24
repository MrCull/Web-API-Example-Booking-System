using Domain.Exceptions;
using Infrastructure.Repository;

namespace Api;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (MovieException notFoundException)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync(notFoundException.Message);
        }
        catch (TheaterException domainException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(domainException.Message);
        }
        catch (EntityNotExistsException entityNotExistsException)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync(entityNotExistsException.Message);
        }
        catch (Exception exception)
        {
            ILogger<ExceptionHandlingMiddleware> logger = context.RequestServices.GetRequiredService<ILogger<ExceptionHandlingMiddleware>>();
            logger.LogError(exception, "An error occurred while processing the request.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("An internal server error occurred.");
        }
    }
}

