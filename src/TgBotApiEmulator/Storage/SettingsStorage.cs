using Microsoft.EntityFrameworkCore;
using TgBotApiEmulator.Application;
using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Storage.Contracts;
using TgBotApiEmulator.Storage.Contracts.Entities;

namespace TgBotApiEmulator.Storage;

internal sealed class SettingsStorage(
    TgBotSessionContext dbContext,
    IRetranslatedRequestManager abortManager) : ISettingsStorage
{
    public TgSessionId? FindCurrentTgSessionAsync(
        string botTokenHash) => GetActiveSettings(botTokenHash)?.CurrentTgSessionId;

    public async Task<TgSessionId> SetCurrentTgSessionAsync(
        string botTokenHash,
        TgSessionId tgSessionId,
        CancellationToken cancellationToken)
    {
        await (dbContext.TgBotSettings
            ?? throw new Exception())
            .Where(s => s.BotTokenHash == botTokenHash && s.IsActive)
            .ForEachAsync(
                s => s.IsActive = false,
                cancellationToken);

        dbContext.TgBotSettings.Add(new TgBotSettings
        {
            BotTokenHash = botTokenHash,
            CurrentTgSessionId = tgSessionId,
            DtmCreated = DateTime.UtcNow,
            IsActive = true,
            OperatingMode = OperatingMode.RecordingProxy,
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        abortManager.AbortPendingRequests(botTokenHash);

        return tgSessionId;
    }

    public async Task<OperatingMode?> FindOperatingModeAsync(
        string botTokenHash,
        CancellationToken cancellationToken)
    {
        var settings = GetActiveSettings(botTokenHash);
        return settings?.OperatingMode;
    }

    public async Task SetOperatingModeAsync(
        string botTokenHash,
        OperatingMode operatingMode,
        CancellationToken cancellationToken)
    {
        var settings = GetActiveSettings(botTokenHash);

        if (settings == null)
        {
            (dbContext.TgBotSettings ?? throw new Exception()).Add(new TgBotSettings
            {
                BotTokenHash = botTokenHash,
                CurrentTgSessionId = null,
                DtmCreated = DateTime.UtcNow,
                IsActive = true,
                OperatingMode = operatingMode,
            });
        }
        else
        {
            settings.OperatingMode = operatingMode;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (operatingMode == OperatingMode.Emulator)
        {
            abortManager.AbortPendingRequests(botTokenHash);
        }
    }

    public TgBotSettings? FindSettingsBySessionId(
        TgSessionId sessionId)
    {
        return (dbContext.TgBotSettings
            ?? throw new Exception())
            .FirstOrDefault(s => s.CurrentTgSessionId == sessionId);
    }

    private TgBotSettings? GetActiveSettings(string botTokenHash)
    {
        var activeSettingsItems = (dbContext.TgBotSettings
            ?? throw new Exception())
            .Where(s => s.BotTokenHash == botTokenHash && s.IsActive)
            .ToList();

        if (activeSettingsItems.Count > 1)
        {
            throw new Exception("Application settings corrupted.");
        }

        if (activeSettingsItems.Count == 0)
        {
            return null;
        }

        return activeSettingsItems.Single();
    }
}
