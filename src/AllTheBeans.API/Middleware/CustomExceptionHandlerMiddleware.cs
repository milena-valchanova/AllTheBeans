using EntityFramework.Exceptions.Common;
using System.Net;

namespace AllTheBeans.API.Middleware;

public class CustomExceptionHandlerMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var response = context.Response;

            var (status, message) = GetResponse(ex);
            response.StatusCode = (int)status;
            await response.WriteAsync(message);
        }
    }

    private static (HttpStatusCode Status, string Message) GetResponse(Exception ex)
    {
        var code = ex switch
        {
            UniqueConstraintException => HttpStatusCode.Conflict,
            KeyNotFoundException => HttpStatusCode.NotFound,
            _ => HttpStatusCode.InternalServerError
        };
        return (code, ex.Message);
    }
}
