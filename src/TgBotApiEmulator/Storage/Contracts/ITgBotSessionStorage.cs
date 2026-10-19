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

    /// <summary>
    /// Returns updates ordered by their queue position (the record primary key).
    /// <paramref name="fromIncluding"/> is a queue position (record primary key), not a Telegram update ID.
    /// </summary>
    Task<IEnumerable<TgSessionUpdateRecord>> GetOrderedUpdatesAsync(
        TgSessionId sessionId,
        CancellationToken cancellationToken,
        TgSessionRecordId? fromIncluding = null,
        string? updateType = null,
        bool excludeProducedByBot = false);

    /// <summary>
    /// Finds the record primary key of the last update the bot received, based on the
    /// offset the bot passed to getUpdates. The offset may belong either to the Telegram
    /// update ID space (after recording proxy mode) or to the Emulator queue space.
    /// </summary>
    Task<TgSessionRecordId?> FindLastReceivedUpdateRecordIdAsync(
        TgSessionId sessionId,
        UpdateOffset botOffset,
        CancellationToken cancellationToken);

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

    Task<TgSessionRecordId> RecordTgServerResponseAsync(
        TgSessionId tgSessionId,
        DateTime timestamp,
        string statusCode,
        string content,
        CancellationToken cancellationToken);

    /// <summary>
    /// Records an update. <paramref name="updateId"/> is the real Telegram update ID
    /// for updates received from Telegram servers, or null for updates produced locally.
    /// Locally produced updates are identified by the record primary key.
    /// </summary>
    Task<UpdateOffset> RecordUpdateAsync(
        TgSessionId tgSessionId,
        DateTime timestamp,
        string content,
        string updateType,
        UpdateOffset? updateId,
        TgSessionRecordId? parentId,
        bool isProducedByBot,
        CancellationToken cancellationToken);

    Task<TgSessionRecordId?> GetLastReceivedUpdateRecordIdAsync(
        TgSessionId tgSessionId,
        CancellationToken cancellationToken);

    Task SetLastReceivedUpdateRecordIdAsync(
        TgSessionId tgSessionId,
        TgSessionRecordId recordId,
        CancellationToken cancellationToken);
}
