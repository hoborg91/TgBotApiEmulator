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
        var sessionId = (await facade.GetOrCreateSessionAsync(botTokenHash, cancellationToken)).Id;

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
        var sessionId = (await facade.GetOrCreateSessionAsync(botTokenHash, cancellationToken)).Id;

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

    public override JsonResult Json(object? data)
    {
        return base.Json(data, _jsonOptions);
    }
}

public abstract record Assert(
    string BotToken,
    int TimeoutMilliseconds);

public record MessageAssert(
    string BotToken,
    int AfterOffset,
    long ChatId,
    string MessageRegexPattern,
    int TimeoutMilliseconds) : Assert(BotToken, TimeoutMilliseconds);

public record MessageReactionAssert(
    string BotToken,
    int AfterOffset,
    long ChatId,
    int MessageId,
    string ReactionType,
    int TimeoutMilliseconds) : Assert(BotToken, TimeoutMilliseconds);
