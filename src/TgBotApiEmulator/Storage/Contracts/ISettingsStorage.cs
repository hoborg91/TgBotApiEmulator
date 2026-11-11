using TgBotApiEmulator.Contracts;

namespace TgBotApiEmulator.Storage.Contracts;

internal interface ISettingsStorage
{
    TgSessionId? FindCurrentTgSessionAsync(
        string botTokenHash);

    Task<TgSessionId> SetCurrentTgSessionAsync(
        string botTokenHash,
        TgSessionId tgSessionId,
        CancellationToken cancellationToken);
}
