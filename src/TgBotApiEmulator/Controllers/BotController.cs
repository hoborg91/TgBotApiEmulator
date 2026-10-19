using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using TgBotApiEmulator.Application;
using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Utilities;

namespace TgBotApiEmulator.Controllers;

[ApiController]
[Route("")]
public sealed class BotController(
    IRecordingProxyFacadeWrite facadeWrite,
    ISessionFacadeRead facadeRead,
    IUpdateReader updateReader) : ControllerBase
{
    [HttpPost("{botPrefixedToken}/getMe")]
    public async Task<ApiResponse<User>> GetMeAsync(
#pragma warning disable IDE0060 // Remove unused parameter
        string botPrefixedToken)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        await Task.CompletedTask;

        return new ApiResponse<User>
        {
            Ok = true,
            Result = new User
            {
                Id = 1,
                IsBot = true,
                FirstName = "Emulated",
                Username = "emulated_bot"
            },
        };
    }

    [HttpPost("{botPrefixedToken}/deleteWebhook")]
    public async Task<ApiResponse<bool>> DeleteWebhookAsync(
#pragma warning disable IDE0060 // Remove unused parameter
        string botPrefixedToken,
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning disable IDE0060 // Remove unused parameter
        bool? drop_pending_updates)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        await Task.CompletedTask;

        return new ApiResponse<bool>
        {
            Ok = true,
            Result = true,
        };
    }

    private async Task<TgSessionId> GetSessionIdAsync(string botPrefixedToken, CancellationToken cancellationToken)
    {
        var botTokenHash = HashHelper.Hash(botPrefixedToken[3..]);
        var sessionId = (await facadeWrite.GetOrCreateSessionAsync(botTokenHash, cancellationToken)).Id;

        return sessionId;
    }

    [HttpPost("{botPrefixedToken}/getUpdates")]
    public async Task<ApiResponse<Update[]>> GetUpdatesAsync(
        string botPrefixedToken,
        Telegram.Bot.Requests.GetUpdatesRequest request,
        CancellationToken cancellationToken)
    {
        var sessionId = await GetSessionIdAsync(botPrefixedToken, cancellationToken);

        // The bot's offset may be either a Telegram update ID (after recording proxy mode)
        // or an Emulator queue position. Resolve it to the last record the bot has received.
        // When the bot passes no offset (for example, after a restart), continue from the
        // persisted queue cursor.
        var lastReceivedRecordId = await facadeWrite.GetLastReceivedUpdateRecordIdAsync(
            sessionId,
            cancellationToken);

        var startAfterRecordId = request.Offset is { } o && o > 0
            ? await facadeRead.FindLastReceivedUpdateRecordIdAsync(
                sessionId,
                new UpdateOffset(o),
                cancellationToken)
            : null;

        startAfterRecordId ??= lastReceivedRecordId;

        // The reader's border is inclusive, so the first candidate is the record
        // immediately after the last one the bot has already received.
        var effectiveOffset = new TgSessionRecordId((startAfterRecordId?.Value ?? 0) + 1);

        var timeoutMilliseconds = 0;
        if (request.Timeout.HasValue && request.Timeout.Value > 0)
        {
            timeoutMilliseconds = request.Timeout.Value * 900;
        }

        var (update, elapsed, offset) = await updateReader.WaitForUpdateAsync(
            sessionId,
            effectiveOffset,
            _ => true,
            TimeSpan.FromMilliseconds(timeoutMilliseconds),
            cancellationToken,
            excludeProducedByBot: true);

        Update[]? result = [];
        if (update != null)
        {
            result = [update];

            // Remember the queue position of the returned update so that after a restart
            // the bot does not receive all updates again when calling getUpdates
            // without an offset.
            await facadeWrite.SetLastReceivedUpdateRecordIdAsync(
                sessionId,
                new TgSessionRecordId(update.Id),
                cancellationToken);
        }

        return new ApiResponse<Update[]>
        {
            Ok = true,
            Result = result,
        };
    }

    [HttpPost("{botPrefixedToken}/sendMessage")]
    public async Task<ApiResponse<Message>> SendMessageAsync(
        string botPrefixedToken,
        Telegram.Bot.Requests.SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ChatId.Identifier == null)
        {
            throw new ArgumentException(
                "ChatId.Identifier cannot be null.",
                nameof(request));
        }

        var msg = new Message
        {
            Text = request.Text,
            From = new User
            {
                Id = 1001,
                Username = "bot",
                IsBot = true,
            },
            Chat = new Chat
            {
                Id = (long)request.ChatId.Identifier,
                Username = request.ChatId.Username,
                Type = Telegram.Bot.Types.Enums.ChatType.Group,
            },
        };

        var sessionId = await GetSessionIdAsync(botPrefixedToken, cancellationToken);

        await facadeWrite.RecordUpdateAsync(
            sessionId,
            new Update
            {
                Message = msg,
            },
            cancellationToken,
            isProducedByBot: true);

        return new ApiResponse<Message>
        {
            Ok = true,
            Result = msg,
        };
    }

    [HttpPost("{botPrefixedToken}/setMessageReaction")]
    public async Task<ApiResponse<bool>> SetMessageReaction(
        string botPrefixedToken,
        Telegram.Bot.Requests.SetMessageReactionRequest request,
        CancellationToken cancellationToken)
    {
        var reactionsString = request.Reaction == null
            ? string.Empty
            : string.Join(", ", request.Reaction.Select(r => r.ToString()));

        if (request.ChatId.Identifier == null)
        {
            throw new ArgumentException(
                "ChatId.Identifier cannot be null.",
                nameof(request));
        }

        var sessionId = await GetSessionIdAsync(botPrefixedToken, cancellationToken);

        var updateId = await facadeWrite.RecordUpdateAsync(
            sessionId,
            new Update
            {
                MessageReaction = new()
                {
                    MessageId = request.MessageId,
                    NewReaction = request.Reaction?.ToArray() ?? [],
                    User = new User
                    {
                        Id = 1001,
                        Username = "bot",
                        IsBot = true,
                    },
                    Chat = new Chat
                    {
                        Id = (long)request.ChatId.Identifier,
                        Username = request.ChatId.Username,
                        Type = Telegram.Bot.Types.Enums.ChatType.Group,
                    },
                }
            },
            cancellationToken,
            isProducedByBot: true);

        return new ApiResponse<bool>
        {
            Ok = true,
            Result = true,
        };
    }
}
