using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
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

public interface IRecordingProxyFacadeWrite
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

    Task SetBotNameAsync(
        TgSessionId sessionId,
        string botName,
        CancellationToken cancellationToken);

    Task SetCurrentSessionAsync(
        string botTokenHash,
        TgSessionId sessionId,
        CancellationToken cancellationToken);
}

internal sealed class RecordingProxyFacade(
    ITgBotSessionStorage storage,
    ISettingsStorage settingsStorage,
    IOptions<JsonOptions> jsonOptions) : ISessionFacadeRead, IRecordingProxyFacadeWrite
{
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
            nonUniqueName,
            botTokenHash,
            botName)
            .Then(s => StorageToApplication(s));

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
        var result = orderedRecords
            .Select(StorageToApplication)
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

        var activeSessionIds = new HashSet<TgSessionId>();
        foreach (var session in result)
        {
            if (session.BotTokenHash is { } hash)
            {
                var current = settingsStorage.FindCurrentTgSessionAsync(hash);
                if (current is { } id && id == session.Id)
                {
                    activeSessionIds.Add(session.Id);
                }
            }
        }

        return [.. result.Select(s => StorageToApplication(s, activeSessionIds.Contains(s.Id)))];
    }

    public async Task<TgSession> GetSessionAsync(
        TgSessionId sessionId,
        CancellationToken cancellationToken)
    {
        var sessions = await storage.GetTgSessionsAsync(
            sessionId,
            cancellationToken);

        var session = sessions.Select(s => StorageToApplication(s)).Single();
        var isActive = session.BotTokenHash is { } hash
            && settingsStorage.FindCurrentTgSessionAsync(hash) == session.Id;

        return session with { IsActive = isActive };
    }

    public async Task RecordBotRequestAsync(
        TgSessionId sessionId,
        HttpRequest request,
        string tgBotApiMethod,
        string? body,
        CancellationToken cancellationToken)
    {
        //var sessionId = GetCurrentSessionId();

        await storage.RecordBotRequestAsync(
            sessionId,
            DateTime.UtcNow, // TODO Use TimeProvider instead of DateTime static properties.
            request.Method,
            tgBotApiMethod,
            request.QueryString.ToString(),
            request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            body,
            cancellationToken);
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
                var message = JsonSerializer.Deserialize<ApiResponse<Message>>(
                    content,
                    jsonOptions.Value.JsonSerializerOptions)?.Result;

                if (message != null)
                {
                    var botName = message.From?.Username
                        ?? message.From?.FirstName;
                    if (!string.IsNullOrWhiteSpace(botName))
                    {
                        await SetBotNameAsync(sessionId, botName.Trim(), cancellationToken);
                    }

                    await RecordUpdateAsync(
                        sessionId,
                        new Update { Message = message },
                        cancellationToken,
                        parentId,
                        isProducedByBot: true);
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

        return await storage.RecordUpdateAsync(
            sessionId,
            DateTime.UtcNow,
            content,
            update.Type.ToString(),
            updateId,
            parentId,
            isProducedByBot,
            cancellationToken);
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

    private Update StorageToApplication(TgSessionUpdateRecord updateRecord)
    {
        var update = JsonSerializer.Deserialize<Update>(
            updateRecord.EscapedContent,
            jsonOptions.Value.JsonSerializerOptions)
            ?? throw new Exception($"Cannot parse the update.");

        // The Emulator identifies updates by their record primary key. This works for
        // both real Telegram updates and locally produced ones (which have no Telegram ID).
        var queueId = (int)updateRecord.Id.Value;
        update.Id = queueId;

        return update;
    }

    private static TgSession StorageToApplication(Storage.Contracts.Entities.TgSession session, bool isActive = false) => new(
        session.Id,
        session.NonUniqueName,
        session.DtmCreated,
        session.DtmLastUpdated,
        session.BotTokenHash,
        session.BotName,
        isActive
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

    public async Task SetBotNameAsync(
        TgSessionId sessionId,
        string botName,
        CancellationToken cancellationToken)
    {
        await storage.UpdateTgSessionBotNameAsync(sessionId, botName, cancellationToken);
    }

    public async Task SetCurrentSessionAsync(
        string botTokenHash,
        TgSessionId sessionId,
        CancellationToken cancellationToken)
    {
        await settingsStorage.SetCurrentTgSessionAsync(botTokenHash, sessionId, cancellationToken);
    }
}
