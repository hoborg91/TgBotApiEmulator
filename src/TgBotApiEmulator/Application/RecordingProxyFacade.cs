using System.Linq;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Exceptions;
using TgBotApiEmulator.Storage.Contracts;
using TgBotApiEmulator.Storage.Contracts.Entities;
using TgBotApiEmulator.Utilities;

namespace TgBotApiEmulator.Application;

public record TgSession(
    TgSessionId Id,
    string NonUniqueName,
    DateTime DtmCreated,
    DateTime DtmLastUpdated,
    string? BotTokenHash,
    string? BotName,
    long? TgUserId,
    bool IsActive);

public interface ISessionFacadeRead
{
    Task<IReadOnlyCollection<Update>> GetOrderedUpdatesAsync(
        TgSessionId sessionId,
        CancellationToken cancellationToken,
        TgSessionRecordId? fromIncluding = null,
        string? updateType = null,
        bool excludeProducedByBot = false);

    Task<TgSessionRecordId?> FindLastReceivedUpdateRecordIdAsync(
        TgSessionId sessionId,
        UpdateOffset botOffset,
        CancellationToken cancellationToken);

    Task<TgSession> GetSessionAsync(
        TgSessionId sessionId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TgSession>> GetSessionsAsync(
        CancellationToken cancellationToken,
        string? nonUniqueName = null,
        string? botTokenHash = null,
        DateTimeOptionalInterval? dtmCreatedBetweenIncluding = null,
        DateTimeOptionalInterval? dtmLastUpdatedBetweenIncluding = null,
        SessionOrdering[]? orderBy = null);
}

public interface ISessionUpdateFacade
{
    Task<TgSession> GetOrCreateSessionAsync(
        string botTokenHash,
        CancellationToken cancellationToken);

    Task<TgSession> CreateSessionAsync(
        string botTokenHash,
        string nonUniqueName,
        string? botName,
        CancellationToken cancellationToken);

    Task<TgSession> GetSessionAsync(
        TgSessionId sessionId,
        CancellationToken cancellationToken);

    Task<UpdateOffset> RecordUpdateAsync(
        TgSessionId sessionId,
        Update update,
        CancellationToken cancellationToken,
        TgSessionRecordId? parentId = null,
        bool isProducedByBot = false);

    Task<TgSessionRecordId?> GetLastReceivedUpdateRecordIdAsync(
        TgSessionId sessionId,
        CancellationToken cancellationToken);

    Task SetLastReceivedUpdateRecordIdAsync(
        TgSessionId sessionId,
        TgSessionRecordId recordId,
        CancellationToken cancellationToken);

    Task<TgSession?> RenameSessionAsync(
        TgSessionId sessionId,
        string newName,
        CancellationToken cancellationToken);

    Task<bool> DeleteSessionAsync(
        TgSessionId sessionId,
        CancellationToken cancellationToken);

    Task SetBotCredentialsAsync(
        TgSessionId sessionId,
        CancellationToken cancellationToken,
        string? botName = null,
        long? tgUserId = null);

    Task SetCurrentSessionAsync(
        string botTokenHash,
        TgSessionId sessionId,
        CancellationToken cancellationToken);
}

public interface IProxyRecordingFacade
{
    Task RecordBotRequestAsync(
        TgSessionId sessionId,
        HttpRequest request,
        string tgBotApiMethod,
        string? body,
        CancellationToken cancellationToken);

    Task RecordTgServerResponseAsync(
        TgSessionId sessionId,
        HttpStatusCode statusCode,
        string content,
        string tgBotApiMethod,
        CancellationToken cancellationToken);
}

internal sealed class RecordingProxyFacade(
    ITgBotSessionStorage storage,
    ISettingsStorage settingsStorage,
    IOptions<JsonOptions> jsonOptions,
    IUpdateBroadcaster updateBroadcaster) : ISessionFacadeRead, ISessionUpdateFacade, IProxyRecordingFacade
{
    private readonly IUpdateBroadcaster _updateBroadcaster = updateBroadcaster;
    public async Task<TgSession> GetOrCreateSessionAsync(
        string botTokenHash,
        CancellationToken cancellationToken)
    {
        var currentSessionId = settingsStorage.FindCurrentTgSessionAsync(botTokenHash);

        if (currentSessionId is { } id)
        {
            return await GetSessionAsync(id, cancellationToken);
        }

        var nextIndex = await storage.CountTgSessionsAsync(
            cancellationToken,
            botTokenHash: botTokenHash);

        return await CreateSessionAsync(botTokenHash, $"Session {nextIndex + 1}", null, cancellationToken);
    }

    public async Task<TgSession> CreateSessionAsync(
        string botTokenHash,
        string nonUniqueName,
        string? botName,
        CancellationToken cancellationToken)
    {
        var nextIndex = await storage.CountTgSessionsAsync(
            cancellationToken,
            botTokenHash: botTokenHash);

        var session = storage.CreateTgSession(
            nonUniqueName)
            .Then(s => StorageToApplication(s, null));

        await settingsStorage.SetCurrentTgSessionAsync(
            botTokenHash,
            session.Id,
            cancellationToken);

        return session;
    }

    public async Task<IReadOnlyCollection<Update>> GetOrderedUpdatesAsync(
        TgSessionId sessionId,
        CancellationToken cancellationToken,
        TgSessionRecordId? fromIncluding = null,
        string? updateType = null,
        bool excludeProducedByBot = false)
    {
        var orderedRecords = await storage.GetOrderedUpdatesAsync(
            sessionId,
            cancellationToken,
            fromIncluding,
            updateType,
            excludeProducedByBot);
        var settings = settingsStorage.FindSettingsBySessionId(sessionId);
        var result = orderedRecords
            .Select(r => StorageToApplication(r, settings))
            .ToList();

        return result;
    }

    public Task<TgSessionRecordId?> FindLastReceivedUpdateRecordIdAsync(
        TgSessionId sessionId,
        UpdateOffset botOffset,
        CancellationToken cancellationToken)
        => storage.FindLastReceivedUpdateRecordIdAsync(sessionId, botOffset, cancellationToken);

    public async Task<IReadOnlyCollection<TgSession>> GetSessionsAsync(
        CancellationToken cancellationToken,
        string? nonUniqueName = null,
        string? botTokenHash = null,
        DateTimeOptionalInterval? dtmCreatedBetweenIncluding = null,
        DateTimeOptionalInterval? dtmLastUpdatedBetweenIncluding = null,
        SessionOrdering[]? orderBy = null)
    {
        var result = await storage.GetTgSessionsAsync(
            null,
            cancellationToken,
            nonUniqueName,
            botTokenHash,
            dtmCreatedBetweenIncluding,
            dtmLastUpdatedBetweenIncluding,
            orderBy);

        return [.. result.Select(s => StorageToApplication(s, settingsStorage.FindSettingsBySessionId(s.Id)))];
    }

    public async Task<TgSession> GetSessionAsync(
        TgSessionId sessionId,
        CancellationToken cancellationToken)
    {
        var sessions = await storage.GetTgSessionsAsync(
            sessionId,
            cancellationToken);

        var settings = settingsStorage.FindSettingsBySessionId(sessionId);
        var session = sessions.Select(s => StorageToApplication(s, settings)).Single();
        var isActive = settings is not null && settings.CurrentTgSessionId == sessionId;

        return session with { IsActive = isActive };
    }

    public async Task RecordBotRequestAsync(
        TgSessionId sessionId,
        HttpRequest request,
        string tgBotApiMethod,
        string? body,
        CancellationToken cancellationToken)
    {
        await storage.RecordBotRequestAsync(
            sessionId,
            DateTime.UtcNow, // TODO Use TimeProvider instead of DateTime static properties.
            request.Method,
            tgBotApiMethod,
            request.QueryString.ToString(),
            request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            body,
            cancellationToken);

        if (tgBotApiMethod == "sendMessage" && !string.IsNullOrWhiteSpace(body))
        {
            await RecordSendMessageAsync(sessionId, body, cancellationToken);
        }
        else if (tgBotApiMethod == "setMessageReaction" && !string.IsNullOrWhiteSpace(body))
        {
            await RecordSetMessageReactionAsync(sessionId, body, cancellationToken);
        }
    }

    private async Task RecordSendMessageAsync(
        TgSessionId sessionId,
        string body,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = JsonSerializer.Deserialize<SendMessageRequest>(
                body,
                jsonOptions.Value.JsonSerializerOptions);

            if (request is null)
            {
                return;
            }

            var message = new Message
            {
                Chat = new Chat
                {
                    Id = GetChatIdValue(request.ChatId),
                    Username = request.ChatId.Username,
                    Type = ChatType.Private,
                },
                Text = request.Text,
                From = new User { Id = 1001, Username = "bot", IsBot = true },
            };

            await RecordUpdateAsync(
                sessionId,
                new Update { Message = message },
                cancellationToken,
                isProducedByBot: true);
        }
        catch (JsonException)
        {
            // The bot request record is already saved;
            // child records are best-effort.
        }
    }

    private async Task RecordSetMessageReactionAsync(
        TgSessionId sessionId,
        string body,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = JsonSerializer.Deserialize<SetMessageReactionRequest>(
                body,
                jsonOptions.Value.JsonSerializerOptions);

            if (request is null)
            {
                return;
            }

            var reactions = request.Reaction?
                .OfType<ReactionTypeEmoji>()
                .Select(r => new ReactionTypeEmoji { Emoji = r.Emoji })
                .ToArray() ?? [];

            var update = new Update
            {
                MessageReaction = new()
            };
            update.MessageReaction!.MessageId = request.MessageId;
            update.MessageReaction!.NewReaction = reactions;
            update.MessageReaction!.User = new User { Id = 1001, Username = "bot", IsBot = true };
            update.MessageReaction!.Chat = new Chat
            {
                Id = GetChatIdValue(request.ChatId),
                Username = request.ChatId.Username,
                Type = ChatType.Private,
            };

            await RecordUpdateAsync(
                sessionId,
                update,
                cancellationToken,
                isProducedByBot: true);
        }
        catch (JsonException)
        {
            // The bot request record is already saved;
            // child records are best-effort.
        }
    }

    private static long GetChatIdValue(ChatId chatId)
    {
        return chatId.Identifier is long id ? id : 0;
    }

    public async Task RecordTgServerResponseAsync(
        TgSessionId sessionId,
        HttpStatusCode statusCode,
        string content,
        string tgBotApiMethod,
        CancellationToken cancellationToken)
    {
        var parentId = await storage.RecordTgServerResponseAsync(
            sessionId,
            DateTime.UtcNow,
            statusCode.ToString(),
            content,
            cancellationToken);

        try
        {
            if (tgBotApiMethod == "getUpdates")
            {
                var updates = JsonSerializer.Deserialize<ApiResponse<Update[]>>(
                    content,
                    jsonOptions.Value.JsonSerializerOptions)?.Result;

                if (updates != null)
                {
                    foreach (var update in updates)
                    {
                        await RecordUpdateAsync(sessionId, update, cancellationToken, parentId);
                    }
                }
            }
            else if (tgBotApiMethod == "sendMessage")
            {
                var response = JsonSerializer.Deserialize<ApiResponse<Message>>(
                    content,
                    jsonOptions.Value.JsonSerializerOptions)?.Result;

                if (response is not null)
                {
                    var botName = response.From?.Username
                        ?? response.From?.FirstName;
                    var tgUserId = response.From?.Id;

                    if (!string.IsNullOrWhiteSpace(botName) || tgUserId is { })
                    {
                        await SetBotCredentialsAsync(
                            sessionId,
                            cancellationToken,
                            botName?.Trim(),
                            tgUserId);
                    }
                }
            }
        }
        catch (JsonException)
        {
            // The parent record with the raw content is already saved;
            // child records are best-effort.
        }
    }

    public async Task<UpdateOffset> RecordUpdateAsync(
        TgSessionId sessionId,
        Update update,
        CancellationToken cancellationToken,
        TgSessionRecordId? parentId = null,
        bool isProducedByBot = false)
    {
        var content = JsonSerializer.Serialize(update, jsonOptions.Value.JsonSerializerOptions);

        // A positive ID means the update was recorded from a real Telegram response
        // (its real Telegram update ID must be preserved). Zero or negative means the
        // update was created locally and has no Telegram update ID.
        UpdateOffset? updateId = update.Id > 0
            ? new UpdateOffset(update.Id)
            : null;

        var offset = await storage.RecordUpdateAsync(
            sessionId,
            DateTime.UtcNow,
            content,
            update.Type.ToString(),
            updateId,
            parentId,
            isProducedByBot,
            cancellationToken);

        _ = _updateBroadcaster.BroadcastAsync(sessionId);

        return offset;
    }

    public Task<TgSessionRecordId?> GetLastReceivedUpdateRecordIdAsync(
        TgSessionId sessionId,
        CancellationToken cancellationToken)
        => storage.GetLastReceivedUpdateRecordIdAsync(sessionId, cancellationToken);

    public Task SetLastReceivedUpdateRecordIdAsync(
        TgSessionId sessionId,
        TgSessionRecordId recordId,
        CancellationToken cancellationToken)
        => storage.SetLastReceivedUpdateRecordIdAsync(sessionId, recordId, cancellationToken);

    private TgSessionId GetCurrentSessionId(string botTokenHash)
    {
        return settingsStorage.FindCurrentTgSessionAsync(botTokenHash)
            ?? throw new Exception("No current session.");
    }

    private Update StorageToApplication(TgSessionUpdateRecord updateRecord, TgBotSettings? settings = null)
    {
        var update = JsonSerializer.Deserialize<Update>(
            updateRecord.EscapedContent,
            jsonOptions.Value.JsonSerializerOptions)
            ?? throw new Exception($"Cannot parse the update.");

        var queueId = (int)updateRecord.Id.Value;
        update.Id = queueId;

        if (settings is not null && settings.TgUserId is { } tgUserId)
        {
            PatchBotUser(update.Message?.From, tgUserId, settings.BotName);
            PatchBotUser(update.MessageReaction?.User, tgUserId, settings.BotName);
        }

        return update;
    }

    private static void PatchBotUser(User? user, long tgUserId, string? botName)
    {
        if (user is null || user.Id != 1001 || user.Username != "bot")
        {
            return;
        }

        user.Id = tgUserId;
        if (botName is not null)
        {
            user.Username = botName;
        }
    }

    private static TgSession StorageToApplication(Storage.Contracts.Entities.TgSession session, TgBotSettings? settings = null) => new(
        session.Id,
        session.NonUniqueName,
        session.DtmCreated,
        session.DtmLastUpdated,
        settings?.BotTokenHash,
        settings?.BotName,
        settings?.TgUserId,
        settings is not null && settings.CurrentTgSessionId == session.Id
    );

    public async Task<TgSession?> RenameSessionAsync(
        TgSessionId sessionId,
        string newName,
        CancellationToken cancellationToken)
    {
        var session = await storage.RenameTgSessionAsync(sessionId, newName, cancellationToken);
        return session is null ? null : StorageToApplication(session);
    }

    public async Task<bool> DeleteSessionAsync(
        TgSessionId sessionId,
        CancellationToken cancellationToken)
    {
        return await storage.DeleteTgSessionAsync(sessionId, cancellationToken);
    }

    public async Task SetBotCredentialsAsync(
        TgSessionId sessionId,
        CancellationToken cancellationToken,
        string? botName = null,
        long? tgUserId = null)
    {
        await storage.UpdateBotCredentialsAsync(sessionId, cancellationToken, botName, tgUserId);
    }

    public async Task SetCurrentSessionAsync(
        string botTokenHash,
        TgSessionId sessionId,
        CancellationToken cancellationToken)
    {
        await settingsStorage.SetCurrentTgSessionAsync(botTokenHash, sessionId, cancellationToken);
    }
}
