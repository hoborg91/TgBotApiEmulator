using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Storage.Contracts.Entities;

namespace TgBotApiEmulator.Storage.Contracts;

internal interface ISettingsStorage
{
    TgSessionId? FindCurrentTgSessionAsync(
        string botTokenHash);

    Task<TgSessionId> SetCurrentTgSessionAsync(
        string botTokenHash,
        TgSessionId tgSessionId,
        CancellationToken cancellationToken);

    Task<OperatingMode?> FindOperatingModeAsync(
        string botTokenHash,
        CancellationToken cancellationToken);

    Task SetOperatingModeAsync(
        string botTokenHash,
        OperatingMode operatingMode,
        CancellationToken cancellationToken);

    TgBotSettings? FindSettingsBySessionId(
        TgSessionId sessionId);
}
