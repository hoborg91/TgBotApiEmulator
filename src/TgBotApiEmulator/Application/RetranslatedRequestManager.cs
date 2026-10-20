namespace TgBotApiEmulator.Application;

internal sealed class RetranslatedRequestManager : IRetranslatedRequestManager
{
    private readonly Lock _lock = new();
    private readonly Dictionary<string, CancellationTokenSource> _pendingRequests = [];

    public IRequestRegistration RegisterRequest(string botTokenHash)
    {
        var cts = new CancellationTokenSource();

        lock (_lock)
        {
            _pendingRequests[botTokenHash] = cts;
        }

        return new RequestRegistration(this, botTokenHash, cts);
    }

    public void AbortPendingRequests(string botTokenHash)
    {
        CancellationTokenSource? cts = null;

        lock (_lock)
        {
            if (_pendingRequests.TryGetValue(botTokenHash, out var existing))
            {
                _pendingRequests.Remove(botTokenHash);
                cts = existing;
            }
        }

        cts?.Cancel();
    }

    private void UnregisterRequest(string botTokenHash, CancellationTokenSource cts)
    {
        lock (_lock)
        {
            if (_pendingRequests.TryGetValue(botTokenHash, out var existing) && ReferenceEquals(existing, cts))
            {
                _pendingRequests.Remove(botTokenHash);
            }
        }
    }

    private sealed class RequestRegistration(
        RetranslatedRequestManager manager,
        string botTokenHash,
        CancellationTokenSource cts) : IRequestRegistration
    {
        private readonly RetranslatedRequestManager _manager = manager;
        private readonly string _botTokenHash = botTokenHash;
        private readonly CancellationTokenSource _cts = cts;
        private bool _disposed;

        public CancellationToken Token => _cts.Token;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _manager.UnregisterRequest(_botTokenHash, _cts);
            _cts.Dispose();
        }
    }
}
