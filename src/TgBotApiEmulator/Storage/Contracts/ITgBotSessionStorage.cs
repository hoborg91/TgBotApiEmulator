using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Storage.Contracts.Entities;
using TgBotApiEmulator.Utilities;

namespace TgBotApiEmulator.Storage.Contracts;

internal interface ITgBotSessionStorage
{
    Task<IEnumerable<TgSession>> GetTgSessionsAsync(
        TgSessionId? tgSessionId,
        CancellationToken cancellationToken,
        string? nonUniqueName = null,
        string? botTokenHash = null,
        DateTimeOptionalInterval? dtmCreatedBetweenIncluding = null,
        DateTimeOptionalInterval? dtmLastUpdatedBetweenIncluding = null,
        SessionOrdering[]? orderBy = null);

    TgSession GetTgSessionById(
        TgSessionId tgSessionId);

    TgSession CreateTgSession(
        string nonUniqueName,
        string? botTokenHash = null);

    // Task<IEnumerable<TgSessionRecord>> GetOrderedRecordsAsync(
    //     CancellationToken cancellationToken,
    //     UpdateOffset? afterOffset = null);

    Task<IEnumerable<TgSessionUpdateRecord>> GetOrderedUpdatesAsync(
        TgSessionId sessionId,
        CancellationToken cancellationToken,
        UpdateOffset? afterOffset = null,
        string? updateType = null);

    Task<TgSessionRecord?> GetMostRecentRecordAsync(
        CancellationToken cancellationToken);

    Task RecordBotRequestAsync(
        TgSessionId tgSessionId,
        DateTime timestamp,
        string httpMethod,
        string tgBotApiMethod,
        string queryString,
        IReadOnlyDictionary<string, string> headers,
        string? body,
        CancellationToken cancellationToken);

    Task RecordTgServerResponseAsync(
        TgSessionId tgSessionId,
        DateTime timestamp,
        string statusCode,
        string content,
        CancellationToken cancellationToken);

    Task<UpdateOffset> RecordUpdateAsync(
        TgSessionId tgSessionId,
        DateTime timestamp,
        string content,
        string updateType,
        int offset,
        CancellationToken cancellationToken);
}
