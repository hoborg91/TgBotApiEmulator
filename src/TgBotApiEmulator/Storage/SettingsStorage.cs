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
        await (dbContext.Settings
            ?? throw new Exception())
            .Where(s => s.BotTokenHash == botTokenHash && s.IsActive)
            .ForEachAsync(
                s => s.IsActive = false,
                cancellationToken);

        dbContext.Settings.Add(new Settings
        {
            BotTokenHash = botTokenHash,
            CurrentTgSessionId = tgSessionId,
            DtmCreated = DateTime.UtcNow,
            IsActive = true,
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        // The active session for this bot changed. Any in-flight retranslated
        // request (for example a long-polling getUpdates) captured the previous
        // session id before the switch and would record its response under the
        // stale session. Abort it so the bot retries and resolves to the new
        // active session instead.
        abortManager.AbortPendingRequests(botTokenHash);

        return tgSessionId;
    }

    private Settings? GetActiveSettings(string botTokenHash)
    {
        var activeSettingsItems = (dbContext.Settings
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
