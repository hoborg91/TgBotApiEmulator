using TgBotApiEmulator.Contracts;

namespace TgBotApiEmulator.Storage.Contracts;

internal interface IBotOperatingModeStorage
{
    Task<OperatingMode?> FindOperatingModeAsync(
        string botTokenHash,
        CancellationToken cancellationToken);

    Task SetOperatingModeAsync(
        string botTokenHash,
        OperatingMode operatingMode,
        CancellationToken cancellationToken);
}