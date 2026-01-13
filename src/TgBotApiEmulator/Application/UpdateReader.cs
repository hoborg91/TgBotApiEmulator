using TgBotApiEmulator.Contracts;

namespace TgBotApiEmulator.Application;

public interface IUpdateReader
{
    Task<(Telegram.Bot.Types.Update? Update, TimeSpan Elapsed, UpdateOffset Offset)> WaitForUpdateAsync(
        TgSessionId sessionId,
        UpdateOffset afterOffset,
        Predicate<Telegram.Bot.Types.Update> filter,
        TimeSpan timeout,
        CancellationToken cancellationToken);
}

internal sealed class UpdateReader(
    ISessionFacadeRead recordingProxyFacadeRead,
    IClock clock) : IUpdateReader
{
    private readonly TimeSpan _step = TimeSpan.FromMilliseconds(100);

    public async Task<(Telegram.Bot.Types.Update? Update, TimeSpan Elapsed, UpdateOffset Offset)> WaitForUpdateAsync(
        TgSessionId sessionId,
        UpdateOffset afterOffset,
        Predicate<Telegram.Bot.Types.Update> filter,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        if (afterOffset.Offset < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(afterOffset),
                "Negative offset is not supported for now.");
        }

        if (timeout != TimeSpan.Zero && timeout <= _step)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeout),
                $"Timeout must be either a zero or more than {_step}.");
        }

        var waitingMilliseconds = 0.0;
        var commitedOffset = afterOffset;
        // var observedMessages = new List<(int Step, UpdateOffset CommitedOffset, TgSessionMessage Message)>();
        // var step = 0;

        var sw = clock.StartStopwatch();
        while (true)
        {
            // step++;
            var udpates = await recordingProxyFacadeRead.GetOrderedUpdatesAsync(
                sessionId,
                cancellationToken,
                afterOffset: commitedOffset.Offset >= 0 ? commitedOffset : null,
                null);
            var offset = commitedOffset;
            foreach (var update in udpates)
            {
                offset = new UpdateOffset(update.Id);// update.Offset;
                                                     // observedMessages.Add((step, commitedOffset, message));

                if (filter(update))
                {
                    return (update, TimeSpan.FromMilliseconds(waitingMilliseconds), offset);
                }
            }
            commitedOffset = offset;

            var elapsed = sw.Elapsed;
            if (elapsed > timeout)
            {
                return (null, elapsed, offset);
            }

            await clock.Delay(_step, cancellationToken);

            waitingMilliseconds = elapsed.TotalMilliseconds;
        }
    }
}
