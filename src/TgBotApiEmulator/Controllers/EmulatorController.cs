using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using TgBotApiEmulator.Application;
using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Utilities;

namespace TgBotApiEmulator.Controllers;

[ApiController]
[Route("emulate")]
public sealed class EmulatorController(
    IRecordingProxyFacadeWrite facade,
    IUpdateNotifier updateNotifier) : ControllerBase
{
    [HttpPost("message")]
    public async Task<MessageResult> MessageAsync(
        MessageRequest request,
        CancellationToken cancellationToken)
    {
        var botTokenHash = HashHelper.Hash(request.BotToken);
        var sessionId = await ResolveSessionIdAsync(request.SessionId, botTokenHash, cancellationToken);

        var updateId = await facade.RecordUpdateAsync(
            sessionId,
            new Update
            {
                Message = new Message
                {
                    Text = request.Text,
                    From = new User
                    {
                        Id = 11,
                        Username = "testUser",
                    },
                    Chat = new Chat
                    {
                        Id = request.ChatId,
                        Type = Telegram.Bot.Types.Enums.ChatType.Group,
                    },
                },
            },
            cancellationToken);

        updateNotifier.Notify(sessionId);

        return new(updateId.Value);
    }

    [HttpPost("setMessageReaction")]
    public async Task SetMessageReactionAsync(
        MessageReactionRequest request,
        CancellationToken cancellationToken)
    {
        var botTokenHash = HashHelper.Hash(request.BotToken);
        var sessionId = await ResolveSessionIdAsync(request.SessionId, botTokenHash, cancellationToken);

        var reaction = request.Reaction is { } emoji
            ? new ReactionType[] { new ReactionTypeEmoji { Emoji = emoji } }
            : [];

        await facade.RecordUpdateAsync(
            sessionId,
            new Update
            {
                MessageReaction = new()
                {
                    MessageId = (int)request.MessageId,
                    NewReaction = reaction,
                    User = new User
                    {
                        Id = 11,
                        Username = "testUser",
                    },
                    Chat = new Chat
                    {
                        Id = request.ChatId,
                        Type = Telegram.Bot.Types.Enums.ChatType.Group,
                    },
                },
            },
            cancellationToken);

        updateNotifier.Notify(sessionId);
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
}

public record MessageRequest(string BotToken, long ChatId, string Text, long? SessionId = null);

public record MessageReactionRequest(string BotToken, long ChatId, long MessageId, string Reaction, long? SessionId = null);

public record MessageResult(long Offset);
