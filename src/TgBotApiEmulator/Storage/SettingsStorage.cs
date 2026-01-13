using Microsoft.EntityFrameworkCore;
using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Storage.Contracts;
using TgBotApiEmulator.Storage.Contracts.Entities;

namespace TgBotApiEmulator.Storage;

internal sealed class SettingsStorage(
    TgBotSessionContext dbContext) : ISettingsStorage
{
    public TgSessionId? FindCurrentTgSessionAsync() => GetActiveSettings()?.CurrentTgSessionId;

    public async Task<TgSessionId> SetCurrentTgSessionAsync(
        TgSessionId tgSessionId,
        CancellationToken cancellationToken)
    {
        await (dbContext.Settings
            ?? throw new Exception())
            .Where(s => s.IsActive)
            .ForEachAsync(
                s => s.IsActive = false,
                cancellationToken);

        dbContext.Settings.Add(new Settings
        {
            CurrentTgSessionId = tgSessionId,
            DtmCreated = DateTime.UtcNow,
            IsActive = true,
        });

        return tgSessionId;
    }

    private Settings? GetActiveSettings()
    {
        var activeSettingsItems = (dbContext.Settings
            ?? throw new Exception())
            .Where(s => s.IsActive)
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
