using TgBotApiEmulator.Contracts;

namespace TgBotApiEmulator.Application;

public interface IUpdateNotifier
{
    Task SubscribeAsync(TgSessionId sessionId, CancellationToken cancellationToken = default);

    void Notify(TgSessionId sessionId);
}
