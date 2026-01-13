using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using TgBotApiEmulator.Application;

namespace TgBotApiEmulator.Controllers;

[ApiController]
[Route("assert")]
public class AssertController(
    IMyAssert myAssert) : Controller
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters = {
            new JsonStringEnumConverter(),
        },
    };

    [HttpPost("botShouldSendMessage")]
    public async Task<JsonResult> WaitForMessageFromBot(
        [FromBody] MessageAssert assert,
        CancellationToken cancellationToken)
    {
        var result = await myAssert.WaitForMessageFromBot(
            new(assert.SessionId),
            new(assert.AfterOffset),
            assert.ChatId,
            new System.Text.RegularExpressions.Regex(assert.MessageRegexPattern),
            TimeSpan.FromMilliseconds(assert.TimeoutMilliseconds),
            cancellationToken
        );

        return Json(result);
    }

    [HttpPost("botShouldSetMessageReaction")]
    public async Task<JsonResult> WaitForMessageReactionFromBot(
        [FromBody] MessageReactionAssert assert,
        CancellationToken cancellationToken)
    {
        var result = await myAssert.WaitForMessageReactionFromBot(
            new(assert.SessionId),
            new(assert.AfterOffset),
            assert.ChatId,
            assert.MessageId,
            assert.ReactionType,
            TimeSpan.FromMilliseconds(assert.TimeoutMilliseconds),
            cancellationToken
        );

        return Json(result);
    }

    public override JsonResult Json(object? data)
    {
        return base.Json(data, _jsonOptions);
    }
}

public abstract record Assert(
    long SessionId,
    int TimeoutMilliseconds);

public record MessageAssert(
    long SessionId,
    int AfterOffset,
    long ChatId,
    string MessageRegexPattern,
    int TimeoutMilliseconds) : Assert(SessionId, TimeoutMilliseconds);

public record MessageReactionAssert(
    long SessionId,
    int AfterOffset,
    long ChatId,
    int MessageId,
    string ReactionType,
    int TimeoutMilliseconds) : Assert(SessionId, TimeoutMilliseconds);
