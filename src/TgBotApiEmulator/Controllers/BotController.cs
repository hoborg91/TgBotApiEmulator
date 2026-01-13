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
    IUpdateReader updateReader) : ControllerBase
{
    [HttpPost("{botTokenString}/getMe")]
    public async Task<ApiResponse<User>> GetMeAsync(
#pragma warning disable IDE0060 // Remove unused parameter
        string botTokenString)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        Console.WriteLine($"getMe...");
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

    [HttpPost("{botTokenString}/deleteWebhook")]
    public async Task<ApiResponse<bool>> DeleteWebhookAsync(
#pragma warning disable IDE0060 // Remove unused parameter
        string botTokenString,
#pragma warning restore IDE0060 // Remove unused parameter
        bool? drop_pending_updates)
    {
        Console.WriteLine($"deleteWebhook({drop_pending_updates})...");
        await Task.CompletedTask;

        return new ApiResponse<bool>
        {
            Ok = true,
            Result = true,
        };
    }

    private async Task<TgSessionId> GetSessionIdAsync(string botTokenString, CancellationToken cancellationToken)
    {
        var botTokenHash = HashHelper.Hash(botTokenString);
        var sessionId = (await facadeWrite.GetOrCreateSessionAsync(botTokenHash, cancellationToken)).Id;

        return sessionId;
    }

    [HttpPost("{botTokenString}/getUpdates")]
    public async Task<ApiResponse<Update[]>> GetUpdatesAsync(
        string botTokenString,
        Telegram.Bot.Requests.GetUpdatesRequest request,
        CancellationToken cancellationToken)
    {
        var updTypeStr = request.AllowedUpdates == null
            ? "null"
            : string.Join(", ", request.AllowedUpdates.Select(au => au.ToString()));
        Console.WriteLine($"getUpdates({request.Offset}, {request.Limit}, {request.Timeout}, {updTypeStr})...");

        Update[]? result = [];
        if (request.Offset >= 0)
        {
            var timeoutMilliseconds = 0;
            if (request.Timeout.HasValue && request.Timeout.Value > 0)
            {
                timeoutMilliseconds = request.Timeout.Value * 900;
            }

            var sessionId = await GetSessionIdAsync(botTokenString, cancellationToken);

            var (update, elapsed, offset) = await updateReader.WaitForUpdateAsync(
                sessionId,
                new Contracts.UpdateOffset(request.Offset.Value),
                _ => true,
                TimeSpan.FromMilliseconds(timeoutMilliseconds),
                cancellationToken);

            if (update != null)
            {
                result = [update];
            }
        }

        Console.WriteLine($"getUpdates returns, result: " + (result == null ? "null" : result.Length + " updates"));
        return new ApiResponse<Update[]>
        {
            Ok = true,
            Result = result,
        };
    }

    [HttpPost("{botTokenString}/sendMessage")]
    public async Task<ApiResponse<Message>> SendMessageAsync(
#pragma warning disable IDE0060 // Remove unused parameter
        string botTokenString,
#pragma warning restore IDE0060 // Remove unused parameter
        Telegram.Bot.Requests.SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"sendMessage({request.Text})");

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

        var sessionId = await GetSessionIdAsync(botTokenString, cancellationToken);

        await facadeWrite.RecordUpdateAsync(
            sessionId,
            new Update
            {
                Message = msg,
            },
            cancellationToken);

        return new ApiResponse<Message>
        {
            Ok = true,
            Result = msg,
        };
    }

    [HttpPost("{botTokenString}/setMessageReaction")]
    public async Task<ApiResponse<bool>> SetMessageReaction(
#pragma warning disable IDE0060 // Remove unused parameter
        string botTokenString,
#pragma warning restore IDE0060 // Remove unused parameter
        Telegram.Bot.Requests.SetMessageReactionRequest request,
        CancellationToken cancellationToken)
    {
        var reactionsString = request.Reaction == null
            ? string.Empty
            : string.Join(", ", request.Reaction.Select(r => r.ToString()));
        Console.WriteLine($"setMessageReaction({reactionsString})");

        if (request.ChatId.Identifier == null)
        {
            throw new ArgumentException(
                "ChatId.Identifier cannot be null.",
                nameof(request));
        }

        var sessionId = await GetSessionIdAsync(botTokenString, cancellationToken);

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
            cancellationToken);

        return new ApiResponse<bool>
        {
            Ok = true,
            Result = true,
        };
    }
}
