using FairHire.API.Contract;
using FairHire.API.Options;
using System.Diagnostics;
using System.Text.Json;

namespace FairHire.API.Middleware;

/// <summary>
/// Глобальний "периметровий" middleware: базові перевірки для КОЖНОГО запиту
/// - дозволені методи
/// - обов'язкові заголовки/Content-Type (де доречно)
/// - грубий ліміт на Content-Length (без читання тіла)
/// - лог таймінгу + X-Request-Id у відповідь
/// </summary>

public sealed class GuardMiddleware(RequestDelegate next, 
    ILogger<GuardMiddleware> logger, RequestLimitsOptions limits)
{
    public async Task Invoke(HttpContext ctx)
    {
        var requestId = ctx.TraceIdentifier;
        using var _ = logger.BeginScope(new { requestId });

        // Додамо заголовок ДО старту відповіді, незалежно від того, хто нижче її почне
        ctx.Response.OnStarting(() =>
        {
            if (!ctx.Response.Headers.ContainsKey("X-Request-Id"))
                ctx.Response.Headers.Append("X-Request-Id", requestId);
            return Task.CompletedTask;
        });

        // 1) Дозволені методи
        if (!IsAllowedMethod(ctx.Request.Method, ctx.Request.Path))
        {
            await WriteProblem(ctx, StatusCodes.Status400BadRequest, "Method not allowed here");
            return;
        }

        // 2) Базові заголовки
        if (!HasRequiredHeaders(ctx.Request.Headers, ctx.Request.Path, ctx.Request.Method))
        {
            await WriteProblem(ctx, StatusCodes.Status400BadRequest, "Missing or invalid headers");
            return;
        }

        // 3) Content-Length
        if (!IsContentLengthOk(ctx.Request))
        {
            await WriteProblem(ctx, StatusCodes.Status400BadRequest, "Payload too large");
            return;
        }

        var sw = Stopwatch.StartNew();
        try
        {
            await next(ctx);
        }
        finally
        {
            sw.Stop();
            logger.LogInformation(
                "HTTP {method} {path} -> {status} in {elapsed} ms",
                ctx.Request.Method, ctx.Request.Path, ctx.Response.StatusCode, sw.ElapsedMilliseconds);
        }
    }

    // ------------------------------
    // Дозволені методи (без файлових спеціальних правил)
    // ------------------------------
    private static bool IsAllowedMethod(string method, PathString path)
    {
        var m = method.ToUpperInvariant();

        // службові запити інфраструктури допускаємо
        if (m is "OPTIONS" or "HEAD") return true;

        // приклади вузьких політик

        //if (path.ToString().Contains("/update")) return m is "PATCH";
        //if (path.ToString().Contains("/delete")) return m is "DELETE";
        // виправити!!

        // дефолтна політика для API: тільки GET/POST
        return m is "GET" or "POST" /* тимчасове рішення -> */ or "PATCH" or "DELETE";
    }

    // ------------------------------
    // Обов'язкові заголовки для методів із тілом (application/json)
    // ------------------------------
    private static bool HasRequiredHeaders(IHeaderDictionary headers, PathString path, string method)
    {
        // X-Request-Id не обов'язковий, але якщо є — базова валідація довжини
        if (headers.TryGetValue("X-Request-Id", out var incomingId))
        {
            var id = incomingId.ToString();
            if (id.Length is < 1 or > 128) return false;
        }

        // Перевіряємо Content-Type тільки для методів із тілом
        var isBodyMethod = method.Equals("POST", StringComparison.OrdinalIgnoreCase)
                        || method.Equals("PUT", StringComparison.OrdinalIgnoreCase)
                        || method.Equals("PATCH", StringComparison.OrdinalIgnoreCase);

        if (!isBodyMethod) return true;

        if (!headers.TryGetValue("Content-Type", out var ct)) return false; // немає типу

        var mediaType = NormalizeMediaType(ct);
        // для API очікуємо application/json
        return mediaType == "application/json";
    }

    // Витягнути базовий медіатайп без параметрів і знизити регістр
    private static string NormalizeMediaType(string contentType)
    {
        var semi = contentType.IndexOf(';');
        var core = semi >= 0 ? contentType[..semi] : contentType;
        return core.Trim().ToLowerInvariant();
    }

    private static string NormalizeMediaType(Microsoft.Extensions.Primitives.StringValues contentType)
        => NormalizeMediaType(contentType.ToString());

    // ------------------------------
    // Перевірка Content-Length проти конфігурованого порога
    // ------------------------------
    private bool IsContentLengthOk(HttpRequest req)
    {
        // лише для методів із тілом
        if (req.Method is not ("POST" or "PUT" or "PATCH")) return true;

        var limit = limits.ApiMaxBodyBytes;

        if (req.ContentLength.HasValue)
        {
            if (req.ContentLength.Value < 0) return false;
            if (req.ContentLength.Value > limit) return false;
        }

        // Якщо Content-Length немає (chunked) — залишаємо перевірку нижчим шарам / контролерам
        return true;
    }

    private static async Task WriteProblem(HttpContext ctx, int status, string title, string? detail = null)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json";

        var payload = new ProblemContract
        {
            Type = $"https://httpstatuses.io/{status}",
            Title = title,
            Status = status,
            Detail = detail,
            Instance = ctx.Request.Path,
            TraceId = ctx.TraceIdentifier,
            RequestId = ctx.Response.Headers["X-Request-Id"].ToString()
        };

        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}