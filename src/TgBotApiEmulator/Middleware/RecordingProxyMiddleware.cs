using System.Text;
using TgBotApiEmulator.Application;
using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Utilities;

namespace TgBotApiEmulator.Middleware;

internal sealed class RecordingProxyMiddleware(
    IRecordingMiddlewareUtilities utilities,
    IRecordingProxyFacadeWrite facade,
    IOperatingModeFacade operatingModeFacade,
    IRetranslatedRequestManager abortManager) : IMiddleware
{
    public async Task InvokeAsync(
        HttpContext context,
        RequestDelegate next)
    {
        var cancellationToken = context.RequestAborted;

        var (isBotApiCall, path, body, botToken) = await utilities.TryDeconstructAsync(context, cancellationToken);

        if (!isBotApiCall)
        {
            await next(context);
            return;
        }

        var operatingMode = await operatingModeFacade.GetOperatingModeAsync(botToken!, cancellationToken);

        if (operatingMode == OperatingMode.Emulator)
        {
            await next(context);
            return;
        }

        var botTokenHash = HashHelper.Hash(botToken!);

        using var registration = abortManager.RegisterRequest(botTokenHash);

        var tgBotApiMethod = GetTgBotApiMethod(path);

        var sessionId = (await facade.GetOrCreateSessionAsync(botTokenHash, cancellationToken)).Id;

        await facade.RecordBotRequestAsync(
            sessionId,
            context.Request,
            tgBotApiMethod,
            body,
            cancellationToken);

        var httpRequestMessage = PrepareRetranslatedRequestMessage(
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString.ToString(),
            context.Request.Headers,
            body);

        var httpClient = context.RequestServices
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient();

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted, registration.Token);
        var result = await httpClient.SendAsync(httpRequestMessage, linkedCts.Token);
        var content = await result.Content.ReadAsStringAsync(cancellationToken);

        await facade.RecordTgServerResponseAsync(
            sessionId,
            result.StatusCode,
            content,
            tgBotApiMethod,
            cancellationToken);

        await context.Response.WriteAsync(content, cancellationToken);
    }

    private static string GetTgBotApiMethod(string? path)
    {
        var tgBotApiMethod = path == null
            ? string.Empty
            : (path.StartsWith('/')
                ? path.TrimStart('/')
                : path);

        if (tgBotApiMethod.Contains('/'))
        {
            throw new NotSupportedException($"Path {tgBotApiMethod} is not supported.");
        }

        return tgBotApiMethod;
    }

    private static HttpRequestMessage PrepareRetranslatedRequestMessage(
        string httpMethod,
        string path,
        string queryString,
        IHeaderDictionary headers,
        string? body)
    {
        var method = httpMethod switch
        {
            "GET" => HttpMethod.Get,
            "POST" => HttpMethod.Post,
            _ => throw new NotSupportedException($"HTTP method (verb) {httpMethod} is not supported."),
        };

        var httpRequestMessage = new HttpRequestMessage(
            method,
            $"https://api.telegram.org{path}{queryString}");

        foreach (var h in headers
            .Where(h => !h.Key.StartsWith("Content") && h.Key != "Host"))
        {
            httpRequestMessage.Headers.Add(h.Key, (string?)h.Value);
        }

        if (body != null)
        {
            var contentTypeMediaType = (string?)null;
            var contentTypeEndoding = Encoding.UTF8;
            var otherContentHeaders = new Dictionary<string, string?>();

            foreach (var h in headers.Where(h => h.Key.StartsWith("Content")))
            {
                if (h.Key == "Content-Type")
                {
                    var parts = h.Value.ToString().Split(";")
                        .Select(p => p.Trim())
                        .ToList();

                    contentTypeMediaType = parts[0];

                    if (parts.Count > 1)
                    {
                        if (parts[1] != "charset=utf-8")
                        {
                            throw new NotSupportedException($"The Content-Type header part \"{h.Value[1]}\" is not supported.");
                        }
                    }
                }
                else
                {
                    otherContentHeaders.Add(h.Key, h.Value);
                }
            }

            if (contentTypeMediaType == null)
            {
                httpRequestMessage.Content = new StringContent(body, Encoding.UTF8);
            }
            else
            {
                httpRequestMessage.Content = new StringContent(body, contentTypeEndoding, contentTypeMediaType);
            }

            foreach (var h in otherContentHeaders)
            {
                httpRequestMessage.Content.Headers.Add(h.Key, h.Value);
            }
        }

        return httpRequestMessage;
    }
}
