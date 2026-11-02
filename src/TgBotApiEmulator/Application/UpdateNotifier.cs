using System.Collections.Concurrent;
using TgBotApiEmulator.Contracts;

namespace TgBotApiEmulator.Application;

internal sealed class UpdateNotifier : IUpdateNotifier
{
    private readonly ConcurrentDictionary<TgSessionId, List<TaskCompletionSource>> _waiters = new();

    public Task SubscribeAsync(
        TgSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource();

        if (_waiters.TryGetValue(sessionId, out var waiters))
        {
            lock (waiters)
            {
                waiters.Add(tcs);
            }
        }
        else
        {
            _waiters.TryAdd(sessionId, [tcs]);
        }

        cancellationToken.Register(() => tcs.TrySetCanceled());

        return tcs.Task;
    }

    public void Notify(TgSessionId sessionId)
    {
        if (!_waiters.TryRemove(sessionId, out var waiters))
        {
            return;
        }

        foreach (var tcs in waiters)
        {
            tcs.TrySetResult();
        }
    }
}
