using Microsoft.EntityFrameworkCore;
using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Storage.Contracts;
using TgBotApiEmulator.Storage.Contracts.Entities;

namespace TgBotApiEmulator.Storage;

internal sealed class BotOperatingModeStorage(
    TgBotSessionContext dbContext) : IBotOperatingModeStorage
{
    public async Task<OperatingMode?> FindOperatingModeAsync(
        string botTokenHash,
        CancellationToken cancellationToken)
    {
        var entity = await (dbContext.BotOperatingModes
            ?? throw new Exception())
            .SingleOrDefaultAsync(
                e => e.BotTokenHash == botTokenHash,
                cancellationToken);

        return entity?.OperatingMode;
    }

    public async Task SetOperatingModeAsync(
        string botTokenHash,
        OperatingMode operatingMode,
        CancellationToken cancellationToken)
    {
        var entity = await (dbContext.BotOperatingModes
            ?? throw new Exception())
            .SingleOrDefaultAsync(
                e => e.BotTokenHash == botTokenHash,
                cancellationToken);

        if (entity == null)
        {
            dbContext.BotOperatingModes.Add(new BotOperatingMode
            {
                BotTokenHash = botTokenHash,
                OperatingMode = operatingMode,
            });
        }
        else
        {
            entity.OperatingMode = operatingMode;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}