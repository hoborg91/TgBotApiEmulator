using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using TgBotApiEmulator.Application;
using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Utilities;

namespace TgBotApiEmulator.Controllers;

[ApiController]
[Route("assert")]
public class AssertController(
    IMyAssert myAssert,
    IRecordingProxyFacadeWrite facade) : Controller
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
        var botTokenHash = HashHelper.Hash(assert.BotToken);
        var sessionId = await ResolveSessionIdAsync(assert.SessionId, botTokenHash, cancellationToken);

        var result = await myAssert.WaitForMessageFromBot(
            sessionId,
            new TgSessionRecordId(assert.AfterOffset),
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
        var botTokenHash = HashHelper.Hash(assert.BotToken);
        var sessionId = await ResolveSessionIdAsync(assert.SessionId, botTokenHash, cancellationToken);

        var result = await myAssert.WaitForMessageReactionFromBot(
            sessionId,
            new TgSessionRecordId(assert.AfterOffset),
            assert.ChatId,
            assert.MessageId,
            assert.ReactionType,
            TimeSpan.FromMilliseconds(assert.TimeoutMilliseconds),
            cancellationToken
        );

        return Json(result);
    }

    private async Task<TgSessionId> ResolveSessionIdAsync(
        long? sessionId,
        string botTokenHash,
        CancellationToken cancellationToken)
    {
        if (sessionId is { } id)
        {
            return (await facade.GetSessionAsync(new TgSessionId(id), cancellationToken)).Id;
        }

        return (await facade.GetOrCreateSessionAsync(botTokenHash, cancellationToken)).Id;
    }

    public override JsonResult Json(object? data)
    {
        return base.Json(data, _jsonOptions);
    }
}

public abstract record Assert(
    string BotToken,
    int TimeoutMilliseconds,
    long? SessionId = null);

public record MessageAssert(
    string BotToken,
    int AfterOffset,
    long ChatId,
    string MessageRegexPattern,
    int TimeoutMilliseconds,
    long? SessionId = null) : Assert(BotToken, TimeoutMilliseconds, SessionId);

public record MessageReactionAssert(
    string BotToken,
    int AfterOffset,
    long ChatId,
    int MessageId,
    string ReactionType,
    int TimeoutMilliseconds,
    long? SessionId = null) : Assert(BotToken, TimeoutMilliseconds, SessionId);
