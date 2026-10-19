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
    IClock clock) : IUpdateReader
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

            await clock.Delay(_step, cancellationToken);

            waitingMilliseconds = elapsed.TotalMilliseconds;
        }
    }
}
