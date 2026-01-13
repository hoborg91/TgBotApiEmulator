using System.Linq.Expressions;
using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Storage.Contracts;
using TgBotApiEmulator.Storage.Contracts.Entities;
using TgBotApiEmulator.Utilities;

namespace TgBotApiEmulator.Storage;

internal sealed class TgBotSessionStorage(
    TgBotSessionContext dbContext) : ITgBotSessionStorage
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
        var session = new TgSession
        {
            NonUniqueName = nonUniqueName,
            BotTokenHash = botTokenHash,
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
        UpdateOffset? afterOffset = null,
        string? updateType = null)
    {
        var query = (dbContext.TgSessionRawRecords
            ?? throw new Exception())
            .OfType<TgSessionUpdateRecord>()
            .Where(u => u.Session.Id == sessionId);

        if (afterOffset.HasValue)
        {
            query = query.Where(r => r.Offset.InnerValue() > afterOffset.Value.Offset);
        }

        if (updateType != null)
        {
            query = query.Where(r => r.UpdateType == updateType);
        }

        return Task.FromResult(query
            .OrderBy(r => r.Offset)
            .AsEnumerable());
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

    public async Task RecordTgServerResponseAsync(
        TgSessionId tgSessionId,
        DateTime timestamp,
        string statusCode,
        string content,
        CancellationToken cancellationToken)
    {
        var session = GetTgSessionByIdInternal(tgSessionId);

        await dbContext.AddAsync(
            new TgSessionServerResponseRecord
            {
                Session = session,
                Timestamp = timestamp,
                StatusCode = statusCode,
                EscapedContent = content,
            },
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UpdateOffset> RecordUpdateAsync(
        TgSessionId tgSessionId,
        DateTime timestamp,
        string content,
        string updateType,
        int offset,
        CancellationToken cancellationToken)
    {
        var session = GetTgSessionByIdInternal(tgSessionId);

        var record = new TgSessionUpdateRecord
        {
            Session = session,
            Timestamp = timestamp,
            EscapedContent = content,
            UpdateType = updateType,
            Offset = new(offset),
        };

        await (dbContext.TgSessionRawRecords
            ?? throw new Exception()).AddAsync(
            record,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return record.Offset;
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
