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
        UpdateOffset? afterOffset = null,
        string? updateType = null);

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
        string? path,
        string? body,
        CancellationToken cancellationToken);

    Task RecordTgServerResponseAsync(
        TgSessionId sessionId,
        HttpStatusCode statusCode,
        string content,
        CancellationToken cancellationToken);

    Task<long> RecordUpdateAsync(
        TgSessionId sessionId,
        Update update,
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
        UpdateOffset? afterOffset = null,
        string? updateType = null)
    {
        var orderedRecords = await storage.GetOrderedUpdatesAsync(
            sessionId,
            cancellationToken,
            afterOffset,
            updateType);
        var result = orderedRecords
            .Select(StorageToApplication)
            .ToList();

        return result;
    }

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
        string? path,
        string? body,
        CancellationToken cancellationToken)
    {
        var tgBotApiMethod = path == null
            ? string.Empty
            : (path.StartsWith('/')
                ? path.TrimStart('/')
                : path);

        if (tgBotApiMethod.Contains('/'))
        {
            throw new NotSupportedException($"Path {tgBotApiMethod} is not supported.");
        }

        //var sessionId = GetCurrentSessionId();

        await storage.RecordBotRequestAsync(
            sessionId,
            DateTime.UtcNow,
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
        CancellationToken cancellationToken)
    {
        try
        {
            var updates = JsonSerializer.Deserialize<ApiResponse<Update[]>>(
                content,
                jsonOptions.Value.JsonSerializerOptions)?.Result!;

            foreach (var update in updates)
            {
                await RecordUpdateAsync(sessionId, update, cancellationToken);
            }
        }
        catch (JsonException)
        {
            await storage.RecordTgServerResponseAsync(
                sessionId,
                DateTime.UtcNow,
                statusCode.ToString(),
                content,
                cancellationToken);
        }
    }

    public async Task<long> RecordUpdateAsync(
        TgSessionId sessionId,
        Update update,
        CancellationToken cancellationToken)
    {
        var content = JsonSerializer.Serialize(
            new ApiResponse<Update[]>
            {
                Ok = true,
                Result = [update],
            },
            jsonOptions.Value.JsonSerializerOptions);

        var result = await storage.RecordUpdateAsync(
            sessionId,
            DateTime.UtcNow,
            content,
            update.Type.ToString(),
            update.Id,
            cancellationToken);

        return result.Offset;
    }

    private TgSessionId GetCurrentSessionId()
    {
        return settingsStorage.FindCurrentTgSessionAsync()
            ?? throw new Exception("No current session.");
    }

    private Update StorageToApplication(TgSessionUpdateRecord updateRecord)
    {
        var updates = JsonSerializer.Deserialize<ApiResponse<Update[]>>(
            updateRecord.EscapedContent,
            jsonOptions.Value.JsonSerializerOptions)?.Result;

        if (updates == null || updates.Length != 1)
        {
            throw new Exception($"Cannot parse the update.");
        }

        var update = updates.Single();
        update.Id = updateRecord.Offset.Offset;

        if (update is { Message: { } m })
        {
            m.Id = updateRecord.Offset.Offset;
        }

        return update;
    }

    private TgSession StorageToApplication(Storage.Contracts.Entities.TgSession session) => new(
        session.Id,
        session.NonUniqueName
    );
}
