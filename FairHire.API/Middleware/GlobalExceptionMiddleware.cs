namespace FairHire.API.Middleware;

using FairHire.API.Problems;
using FairHire.Application.Exceptions;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, 
    IHostEnvironment env, Problem problem)
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
                ctx.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
        }
        catch (BadHttpRequestException bhe)
        {
            await problem.WriteProblem(ctx, StatusCodes.Status400BadRequest, "Bad request", bhe);
        }
        catch (DomainValidationException dex)
        {
            await problem.WriteProblem(ctx, StatusCodes.Status400BadRequest, "DomainValidation", dex);
        }
        catch (NotFoundException nex)
        {
            await problem.WriteProblem(ctx, StatusCodes.Status404NotFound, "NotFound", nex);
        }
        catch (ForbiddenException fex)
        {
            await problem.WriteProblem(ctx, StatusCodes.Status403Forbidden, "Forbidden", fex);
        }
        catch (UnauthorizedAccessException uae)
        {
            await problem.WriteProblem(ctx, StatusCodes.Status401Unauthorized, "Unauthorized", uae);
        }
        catch (Exception ex)
        {
            // Ключ: якщо відповідь вже почалась — тут, у catch, робимо rethrow; (легально
            logger.LogWarning(ex, "Response already started. Cannot write error body. TraceId={TraceId}", ctx.TraceIdentifier);
            await problem.WriteProblem(ctx, StatusCodes.Status500InternalServerError, "Internal Server Error", ex);
        }
    }
}


