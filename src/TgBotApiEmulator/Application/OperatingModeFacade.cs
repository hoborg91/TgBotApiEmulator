using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Storage.Contracts;

namespace TgBotApiEmulator.Application;

public interface IOperatingModeFacade
{
    Task<OperatingMode> GetOperatingModeAsync(
        string botTokenHash,
        CancellationToken cancellationToken);

    Task SetOperatingModeAsync(
        string botTokenHash,
        OperatingMode operatingMode,
        CancellationToken cancellationToken);
}

internal sealed class OperatingModeFacade(
    ISettingsStorage storage) : IOperatingModeFacade
{
    public async Task<OperatingMode> GetOperatingModeAsync(
        string botTokenHash,
        CancellationToken cancellationToken)
    {
        return await storage.FindOperatingModeAsync(botTokenHash, cancellationToken)
            ?? OperatingMode.RecordingProxy;
    }

    public async Task SetOperatingModeAsync(
        string botTokenHash,
        OperatingMode operatingMode,
        CancellationToken cancellationToken)
    {
        await storage.SetOperatingModeAsync(botTokenHash, operatingMode, cancellationToken);
    }
}