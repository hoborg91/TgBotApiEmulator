using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Storage.Contracts;
using TgBotApiEmulator.Storage.Contracts.Entities;
using TgBotApiEmulator.Utilities;

namespace TgBotApiEmulator.Storage;

internal sealed class TgBotSessionStorage(
    TgBotSessionContext dbContext,
    TimeProvider timeProvider) : ITgBotSessionStorage
{
    public Task<IEnumerable<TgSession>> GetTgSessionsAsync(
        TgSessionId? tgSessionId,
        CancellationToken cancellationToken,
        string? nonUniqueName = null,
        string? botTokenHash = null,
        DateTimeOptionalInterval? dtmCreatedBetweenIncluding = null,
        DateTimeOptionalInterval? dtmLastUpdatedBetweenIncluding = null,
        SessionOrdering[]? orderBy = null)
    {
        IQueryable<TgSession> query = dbContext.TgSessions
            ?? throw new Exception();

        if (tgSessionId != null)
        {
            query = query.Where(s => s.Id == tgSessionId);
        }

        query = ApplyStringFilterIfAny(query, s => s.NonUniqueName, nonUniqueName);
        query = ApplyStringFilterIfAny(query, s => s.BotTokenHash, botTokenHash);

        query = ApplyDateTimeIntervalFilterIfAny(query, s => s.DtmCreated, dtmCreatedBetweenIncluding);
        query = ApplyDateTimeIntervalFilterIfAny(query, s => s.DtmLastUpdated, dtmLastUpdatedBetweenIncluding);

        orderBy ??= [new SessionOrdering(SessionOrderingField.Id, OrderingDirection.Ascending)];

        foreach (var orderByClause in orderBy)
        {
            query = orderByClause.Direction switch
            {
                OrderingDirection.Ascending => query.OrderBy(MakeExpression(orderByClause.Field)),
                OrderingDirection.Descending => query.OrderByDescending(MakeExpression(orderByClause.Field)),
                _ => throw new NotSupportedException(orderByClause.Direction.ToString()),
            };
        }

        return Task.FromResult(query.AsEnumerable());

        IQueryable<TEntity> ApplyDateTimeIntervalFilterIfAny<TEntity>(
            IQueryable<TEntity> query,
            Expression<Func<TEntity, DateTime?>> propertyAccessor,
            DateTimeOptionalInterval? interval)
        {
            var result = query;

            if (dtmCreatedBetweenIncluding is DateTimeOptionalInterval i)
            {
                if (i.From is DateTime @from)
                {
                    result = result.Where(ExpressionBuilder.CreateDateTimeComparison<TEntity>(
                        propertyAccessor,
                        from,
                        ComparisonOperator.GreaterThanOrEqual
                    ));
                }

                if (i.Till is DateTime till)
                {
                    result = result.Where(ExpressionBuilder.CreateDateTimeComparison<TEntity>(
                        propertyAccessor,
                        till,
                        ComparisonOperator.GreaterThanOrEqual
                    ));
                }
            }

            return result;
        }

        IQueryable<TEntity> ApplyStringFilterIfAny<TEntity>(
            IQueryable<TEntity> query,
            Expression<Func<TEntity, string?>> propertyAccessor,
            string? targetValue)
        {
            if (string.IsNullOrWhiteSpace(targetValue))
            {
                return query;
            }

            return query.Where(ExpressionBuilder.CreateStringContainsComparisonWithNullCheck(
                propertyAccessor,
                targetValue,
                StringComparison.InvariantCultureIgnoreCase));
        }

        Expression<Func<TgSession, object?>> MakeExpression(SessionOrderingField field) => field switch
        {
            SessionOrderingField.Id => s => s.Id,
            SessionOrderingField.NonUniqueName => s => s.NonUniqueName,
            SessionOrderingField.DtmCreated => s => s.DtmCreated,
            SessionOrderingField.DtmLastUpdated => s => s.DtmLastUpdated,
            _ => throw new NotSupportedException(field.ToString()),
        };
    }

    public TgSession GetTgSessionById(TgSessionId tgSessionId) =>
        GetTgSessionByIdInternal(tgSessionId);

    public TgSession CreateTgSession(
        string nonUniqueName,
        string? botTokenHash = null)
    {
        var now = timeProvider.GetUtcNow().DateTime;
        var session = new TgSession
        {
            NonUniqueName = nonUniqueName,
            BotTokenHash = botTokenHash,
            DtmCreated = now,
            DtmLastUpdated = now,
        };

        (dbContext.TgSessions
            ?? throw new Exception()).Add(session);

        dbContext.SaveChanges();

        return session;
    }

    // public Task<IEnumerable<TgSessionRecord>> GetOrderedRecordsAsync(
    //     CancellationToken cancellationToken,
    //     UpdateOffset? afterOffset = null)
    // {
    //     IQueryable<TgSessionRecord> query = dbContext.TgSessionRawRecords
    //         ?? throw new Exception();

    //     if (afterOffset.HasValue)
    //     {
    //         query = query.Where(r => r.Offset.Offset > afterOffset.Value.Offset);
    //     }

    //     return Task.FromResult(query
    //         .OrderBy(r => r.Offset)
    //         .AsEnumerable());
    // }

    public Task<IEnumerable<TgSessionUpdateRecord>> GetOrderedUpdatesAsync(
        TgSessionId sessionId,
        CancellationToken cancellationToken,
        TgSessionRecordId? fromIncluding = null,
        string? updateType = null,
        bool excludeProducedByBot = false)
    {
        var records = (dbContext.TgSessionRawRecords
            ?? throw new Exception())
            .OfType<TgSessionUpdateRecord>()
            .Where(u => u.Session.Id == sessionId);

        if (fromIncluding.HasValue)
        {
            // Updates are identified by their queue position (the record primary key).
            // This works both for real Telegram updates and for updates produced locally.
            var from = fromIncluding.Value.Value;
            records = records.Where(r => StronglyTypedIdValue.InnerValue(r.Id) >= from);
        }

        if (updateType != null)
        {
            records = records.Where(r => r.UpdateType == updateType);
        }

        if (excludeProducedByBot)
        {
            records = records.Where(r => !r.IsProducedByBot);
        }

        return Task.FromResult(records
            .OrderBy(r => StronglyTypedIdValue.InnerValue(r.Id))
            .AsEnumerable());
    }

    public async Task<TgSessionRecordId?> FindLastReceivedUpdateRecordIdAsync(
        TgSessionId sessionId,
        UpdateOffset botOffset,
        CancellationToken cancellationToken)
    {
        if (botOffset.Value <= 0)
        {
            return null;
        }

        var updates = (dbContext.TgSessionRawRecords
            ?? throw new Exception())
            .OfType<TgSessionUpdateRecord>()
            .Where(u => u.Session.Id == sessionId);

        // The offset was produced by the Emulator if it points directly at a record
        // without a Telegram update ID (locally produced updates use their primary key).
        var previousQueuePosition = botOffset.Value - 1;

        var exactRecord = await updates
            .FirstOrDefaultAsync(u => StronglyTypedIdValue.InnerValue(u.Id) == previousQueuePosition, cancellationToken);

        if (exactRecord is { UpdateId: null, IsProducedByBot: false })
        {
            return exactRecord.Id;
        }

        // Otherwise the offset belongs to the Telegram update ID space. Find the last
        // update whose Telegram ID is lower than the offset (the first one to return).
        var lastTelegramUpdate = await updates
            .Where(u => u.UpdateId.HasValue && StronglyTypedIdValue.InnerValue(u.UpdateId.Value) < botOffset.Value)
            .OrderByDescending(u => StronglyTypedIdValue.InnerValue(u.UpdateId!.Value))
            .FirstOrDefaultAsync(cancellationToken);

        if (lastTelegramUpdate != null)
        {
            return lastTelegramUpdate.Id;
        }

        return exactRecord?.Id;
    }

    public Task<TgSessionRecord?> GetMostRecentRecordAsync(
        CancellationToken cancellationToken)
    {
        return Task.FromResult((dbContext.TgSessionRawRecords
            ?? throw new Exception())
            .OrderByDescending(r => r.Id)
            .FirstOrDefault());
    }

    public async Task RecordBotRequestAsync(
        TgSessionId tgSessionId,
        DateTime timestamp,
        string httpMethod,
        string tgBotApiMethod,
        string queryString,
        IReadOnlyDictionary<string, string> headers,
        string? body,
        CancellationToken cancellationToken)
    {
        var session = GetTgSessionByIdInternal(tgSessionId);

        await (dbContext.TgSessionRawRecords
            ?? throw new Exception()).AddAsync(
            new TgSessionBotActionRecord
            {
                Session = session,
                Timestamp = timestamp,
                HttpMethod = httpMethod,
                TgBotApiMethod = tgBotApiMethod,
                QueryString = queryString,
                Headers = headers,
                EscapedBody = body,
            },
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<TgSessionRecordId> RecordTgServerResponseAsync(
        TgSessionId tgSessionId,
        DateTime timestamp,
        string statusCode,
        string content,
        CancellationToken cancellationToken)
    {
        var session = GetTgSessionByIdInternal(tgSessionId);

        var record = new TgSessionServerResponseRecord
        {
            Session = session,
            Timestamp = timestamp,
            StatusCode = statusCode,
            EscapedContent = content,
        };

        await dbContext.AddAsync(record, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return record.Id;
    }

    public async Task<UpdateOffset> RecordUpdateAsync(
        TgSessionId tgSessionId,
        DateTime timestamp,
        string content,
        string updateType,
        UpdateOffset? updateId,
        TgSessionRecordId? parentId,
        bool isProducedByBot,
        CancellationToken cancellationToken)
    {
        var session = GetTgSessionByIdInternal(tgSessionId);

        var record = new TgSessionUpdateRecord
        {
            Session = session,
            Timestamp = timestamp,
            EscapedContent = content,
            UpdateType = updateType,
            UpdateId = updateId,
            IsProducedByBot = isProducedByBot,
            ParentId = parentId,
        };

        await (dbContext.TgSessionRawRecords
            ?? throw new Exception()).AddAsync(
            record,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        // Locally produced updates have no Telegram ID; their queue identity is the
        // record primary key.
        return updateId ?? new UpdateOffset(record.Id.Value);
    }

    public Task<TgSessionRecordId?> GetLastReceivedUpdateRecordIdAsync(
        TgSessionId tgSessionId,
        CancellationToken cancellationToken)
    {
        var session = GetTgSessionByIdInternal(tgSessionId);

        return Task.FromResult(session.LastReceivedUpdateRecordId);
    }

    public Task SetLastReceivedUpdateRecordIdAsync(
        TgSessionId tgSessionId,
        TgSessionRecordId recordId,
        CancellationToken cancellationToken)
    {
        var session = GetTgSessionByIdInternal(tgSessionId);

        // Never move the cursor backwards: the bot cannot receive an older update
        // after a newer one has already been returned.
        if (!(session.LastReceivedUpdateRecordId?.Value >= recordId.Value))
        {
            session.LastReceivedUpdateRecordId = recordId;
            dbContext.SaveChanges();
        }

        return Task.CompletedTask;
    }

    private TgSession GetTgSessionByIdInternal(TgSessionId tgSessionId)
    {
        var sessions = (dbContext.TgSessions
            ?? throw new Exception())
            .Where(s => s.Id == tgSessionId)
            .ToList();

        if (sessions.Count > 1)
        {
            throw new Exception("Data about Telegram sessions corrupted.");
        }

        if (sessions.Count == 0)
        {
            throw new Exception($"Telegram session with ID [{tgSessionId}] not found.");
        }

        return sessions.Single();
    }
}
