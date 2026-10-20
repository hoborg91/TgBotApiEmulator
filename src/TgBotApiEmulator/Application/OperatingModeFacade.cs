using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Storage.Contracts;
using TgBotApiEmulator.Utilities;

namespace TgBotApiEmulator.Application;

public interface IOperatingModeFacade
{
    Task<OperatingMode> GetOperatingModeAsync(
        string botToken,
        CancellationToken cancellationToken);

    Task SetOperatingModeAsync(
        string botToken,
        OperatingMode operatingMode,
        CancellationToken cancellationToken);
}

internal sealed class OperatingModeFacade(
    IBotOperatingModeStorage storage,
    IRetranslatedRequestManager abortManager) : IOperatingModeFacade
{
    public async Task<OperatingMode> GetOperatingModeAsync(
        string botToken,
        CancellationToken cancellationToken)
    {
        var botTokenHash = HashHelper.Hash(botToken);

        return await storage.FindOperatingModeAsync(botTokenHash, cancellationToken)
            ?? OperatingMode.RecordingProxy;
    }

    public async Task SetOperatingModeAsync(
        string botToken,
        OperatingMode operatingMode,
        CancellationToken cancellationToken)
    {
        var botTokenHash = HashHelper.Hash(botToken);

        await storage.SetOperatingModeAsync(botTokenHash, operatingMode, cancellationToken);

        // Abort any in-flight proxied requests for this bot token so that a pending
        // getUpdates long-poll against Telegram servers is cancelled. The bot's next
        // getUpdates will then hit BotController (Emulator mode) and read updates
        // from the Emulator database instead.
        if (operatingMode == OperatingMode.Emulator)
        {
            abortManager.AbortPendingRequests(botTokenHash);
        }
    }
}