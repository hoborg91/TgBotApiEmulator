using TgBotApiEmulator.Contracts;

namespace TgBotApiEmulator.Application;

public interface IUpdateReader
{
    Task<(Telegram.Bot.Types.Update? Update, TimeSpan Elapsed, TgSessionRecordId Offset)> WaitForUpdateAsync(
        TgSessionId sessionId,
        TgSessionRecordId offset,
        Predicate<Telegram.Bot.Types.Update> filter,
        TimeSpan timeout,
        CancellationToken cancellationToken,
        bool excludeProducedByBot = false);
}

internal sealed class UpdateReader(
    ISessionFacadeRead sessionFacadeRead,
    IClock clock,
    IUpdateNotifier? updateNotifier = null) : IUpdateReader
{
    private readonly TimeSpan _step = TimeSpan.FromMilliseconds(100);

    public async Task<(Telegram.Bot.Types.Update? Update, TimeSpan Elapsed, TgSessionRecordId Offset)> WaitForUpdateAsync(
        TgSessionId sessionId,
        TgSessionRecordId offset,
        Predicate<Telegram.Bot.Types.Update> filter,
        TimeSpan timeout,
        CancellationToken cancellationToken,
        bool excludeProducedByBot = false)
    {
        if (offset.Value < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offset),
                "Negative offset is not supported for now.");
        }

        if (timeout != TimeSpan.Zero && timeout <= _step)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeout),
                $"Timeout must be either a zero or more than {_step}.");
        }

        var waitingMilliseconds = 0.0;

        // Telegram semantics: the offset is the identifier of the first update to be returned,
        // i.e. the border is INCLUSIVE. The reader looks for updates with an ID >= offset.
        var nextOffset = offset;
        var lastObservedOffset = offset;

        var sw = clock.StartStopwatch();
        while (true)
        {
            var updates = await sessionFacadeRead.GetOrderedUpdatesAsync(
                sessionId,
                cancellationToken,
                fromIncluding: nextOffset,
                null,
                excludeProducedByBot);
            var observedOffset = lastObservedOffset;
            foreach (var update in updates)
            {
                // update.Id is the Emulator queue position (the record primary key).
                observedOffset = new TgSessionRecordId(update.Id);
                lastObservedOffset = observedOffset;

                if (filter(update))
                {
                    return (update, TimeSpan.FromMilliseconds(waitingMilliseconds), observedOffset);
                }
            }

            // Advance the inclusive border past the observed updates so they are not fetched again.
            if (updates.Count > 0)
            {
                nextOffset = new TgSessionRecordId(lastObservedOffset.Value + 1);
            }

            var elapsed = sw.Elapsed;
            if (elapsed > timeout)
            {
                return (null, elapsed, observedOffset);
            }

            // Wait for a notification about a new update (bounded by the remaining timeout),
            // then re-poll the database. Falls back to a plain delay when no notifier is
            // available (for example in unit tests that drive timing via IClock).
            // We wait whenever we have not yet strictly passed the timeout, so that the
            // stopwatch can advance and the elapsed > timeout check can eventually fire.
            if (elapsed <= timeout)
            {
                var remaining = timeout - elapsed;
                var waitTimeout = remaining > TimeSpan.Zero
                    ? (remaining < _step ? remaining : _step)
                    : _step;

                using var waitCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                waitCts.CancelAfter(waitTimeout);

                try
                {
                    if (updateNotifier is not null)
                    {
                        await updateNotifier.SubscribeAsync(sessionId, waitCts.Token);
                    }
                    else
                    {
                        await clock.Delay(waitTimeout, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Notification timeout or cancellation: loop and re-check elapsed/timeout.
                }
            }

            waitingMilliseconds = elapsed.TotalMilliseconds;
        }
    }
}
