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
    string NonUniqueName);

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
        var sessions = (await storage.GetTgSessionsAsync(
            tgSessionId: null,
            cancellationToken,
            botTokenHash: botTokenHash)).ToArray();

        if (sessions.Length > 1)
        {
            throw new SessionDataCorruptedException();
        }

        if (sessions.Length == 1)
        {
            return sessions.Single().Then(StorageToApplication);
        }

        return storage.CreateTgSession(botTokenHash, botTokenHash)
            .Then(StorageToApplication);
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

        return [.. result.Select(StorageToApplication)];
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

    private TgSessionId GetCurrentSessionId()
    {
        return settingsStorage.FindCurrentTgSessionAsync()
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

        if (update is { Message: { } m })
        {
            m.Id = queueId;
        }

        return update;
    }

    private TgSession StorageToApplication(Storage.Contracts.Entities.TgSession session) => new(
        session.Id,
        session.NonUniqueName
    );
}
