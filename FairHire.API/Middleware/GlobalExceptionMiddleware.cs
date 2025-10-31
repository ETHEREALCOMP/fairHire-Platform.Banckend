namespace FairHire.API.Middleware;

using System.Text.Json;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (OperationCanceledException oce) when (ctx.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation(oce, "Request canceled by client. TraceId={TraceId}", ctx.TraceIdentifier);
            if (!ctx.Response.HasStarted)
                ctx.Response.StatusCode = StatusCodes.Status499ClientClosedRequest; // або 400/408 за політикою
        }
        catch (BadHttpRequestException bhe)
        {
            await WriteProblem(ctx, StatusCodes.Status400BadRequest, "Bad request", bhe);
        }
        catch (ApplicationException aex)
        {
            await WriteProblem(ctx, StatusCodes.Status400BadRequest, "Application error", aex);
        }
        catch (UnauthorizedAccessException uae)
        {
            await WriteProblem(ctx, StatusCodes.Status403Forbidden, "Forbidden", uae);
        }
        catch (Exception ex)
        {
            // Ключ: якщо відповідь вже почалась — тут, у catch, робимо rethrow; (легально)
            if (ctx.Response.HasStarted)
            {
                logger.LogWarning(ex, "Response already started. Cannot write error body. TraceId={TraceId}", ctx.TraceIdentifier);
                throw;
            }

            await WriteProblem(ctx, StatusCodes.Status500InternalServerError, "Internal Server Error", ex);
        }
    }

    private async Task WriteProblem(HttpContext ctx, int status, string title, Exception ex)
    {
        if (ctx.Response.HasStarted)
            return;

        ctx.Response.Clear();
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json";

        var payload = new
        {
            type = status >= 500 ? "about:blank" : $"https://httpstatuses.io/{status}",
            title,
            status,
            detail = null as string, // у проді без деталей; у Dev віддати ex.Message
            instance = ctx.Request.Path.Value,
            traceId = ctx.TraceIdentifier
        };

        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}


