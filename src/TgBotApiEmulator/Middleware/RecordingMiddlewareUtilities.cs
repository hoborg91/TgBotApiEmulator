using System.Text;
using System.Text.RegularExpressions;

namespace TgBotApiEmulator.Middleware;

internal interface IRecordingMiddlewareUtilities
{
    Task<(bool IsBotApiCall, string? path, string? body, string? botToken)> TryDeconstructAsync(HttpContext context, CancellationToken cancellationToken);
}

internal sealed partial class RecordingMiddlewareUtilities : IRecordingMiddlewareUtilities
{
    private readonly Regex _regex = BotTokenAndMethodRegex();

    public async Task<(
        bool IsBotApiCall,
        string? path,
        string? body,
        string? botToken)> TryDeconstructAsync(
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var match = _regex.Match(context.Request.Path);
        var path = match.Success
            ? match.Groups["methodPart"].Value
            : null;
        var body = match.Success
            ? await PeekRequestBody(context)
            : null;
        var botToken = match.Success
            ? match.Groups["botTokenPart"].Value[4..]
            : null;
        return (match.Success, path, body, botToken);
    }

    private static async Task<string?> PeekRequestBody(HttpContext context)
    {
        context.Request.EnableBuffering();

        // Leave the body open so the next middleware can read it.
        using var reader = new StreamReader(
            context.Request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();

        // Reset the request body stream position so the next middleware can read it
        context.Request.Body.Position = 0;

        return body;
    }

    [GeneratedRegex("^(?<botTokenPart>\\/bot[^\\/]+:[^\\/]+)(?<methodPart>\\/.+)$", RegexOptions.Compiled)]
    private static partial Regex BotTokenAndMethodRegex();
}
