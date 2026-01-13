using TgBotApiEmulator.Contracts;

namespace TgBotApiEmulator.Storage.Contracts;

internal interface ISettingsStorage
{
    TgSessionId? FindCurrentTgSessionAsync();

    Task<TgSessionId> SetCurrentTgSessionAsync(
        TgSessionId tgSessionId,
        CancellationToken cancellationToken);
}
