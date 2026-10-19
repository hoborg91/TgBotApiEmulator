using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Storage;

namespace TgBotApiEmulator.Tests;

public class TgBotSessionStorageTests
{
    private static DbContextOptions<TgBotSessionContext> CreateOptions(SqliteConnection connection)
        => new DbContextOptionsBuilder<TgBotSessionContext>()
            .UseSqlite(connection)
            .Options;

    private static async Task<SqliteConnection> OpenInMemoryConnectionAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var dbContext = new TgBotSessionContext(CreateOptions(connection));
        await dbContext.Database.EnsureCreatedAsync();
        return connection;
    }

    /// <summary>
    /// BUG-1. Updates produced by the bot must be stored but must not be returned
    /// when updates are requested for the bot.
    /// </summary>
    [Test]
    public async Task GetOrderedUpdates_ExcludesBotProducedUpdates_WhenRequested()
    {
        // Arrange
        await using var connection = await OpenInMemoryConnectionAsync();
        await using var dbContext = new TgBotSessionContext(CreateOptions(connection));
        var storage = new TgBotSessionStorage(dbContext, TimeProvider.System);
        var session = storage.CreateTgSession("session", botTokenHash: "token-hash");

        await storage.RecordUpdateAsync(
            session.Id,
            DateTime.UtcNow,
            "{\"update_id\":12345}",
            "Message",
            new UpdateOffset(12345),
            parentId: null,
            isProducedByBot: false,
            cancellationToken: CancellationToken.None);

        await storage.RecordUpdateAsync(
            session.Id,
            DateTime.UtcNow,
            "{\"message\":{\"text\":\"qwerty\"}}",
            "Message",
            updateId: null,
            parentId: null,
            isProducedByBot: false,
            cancellationToken: CancellationToken.None);

        var ownRecordId = await storage.RecordUpdateAsync(
            session.Id,
            DateTime.UtcNow,
            "{\"message\":{\"text\":\"Text length: 6.\"}}",
            "Message",
            updateId: null,
            parentId: null,
            isProducedByBot: true,
            cancellationToken: CancellationToken.None);

        // Act
        var all = (await storage.GetOrderedUpdatesAsync(
            session.Id,
            CancellationToken.None)).ToArray();
        var visibleToBot = (await storage.GetOrderedUpdatesAsync(
            session.Id,
            CancellationToken.None,
            excludeProducedByBot: true)).ToArray();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(all, Has.Length.EqualTo(3));
            Assert.That(
                all.Single(r => r.IsProducedByBot).Id,
                Is.EqualTo(new TgSessionRecordId(ownRecordId.Value)));

            // Locally produced updates must not receive artificial Telegram update IDs.
            Assert.That(all.Single(r => r.UpdateId.HasValue).UpdateId!.Value.Value, Is.EqualTo(12345));
            Assert.That(all.Count(r => r.UpdateId == null), Is.EqualTo(2));

            Assert.That(
                visibleToBot.Select(r => r.Id),
                Is.EqualTo(all
                    .Where(r => !r.IsProducedByBot)
                    .Select(r => r.Id)));
            Assert.That(visibleToBot.Any(r => r.IsProducedByBot), Is.False);
        });
    }

    /// <summary>
    /// BUG-1 reproduction. After recording proxy mode is turned off, the bot passes the
    /// offset from the real Telegram ID space. The Emulator must resolve it to the last
    /// natural update record and must not mistake the bot's own reply for that update.
    /// </summary>
    [Test]
    public async Task FindLastReceivedUpdateRecordId_TelegramOffset_IgnoresBotProducedUpdate()
    {
        // Arrange
        await using var connection = await OpenInMemoryConnectionAsync();
        await using var dbContext = new TgBotSessionContext(CreateOptions(connection));
        var storage = new TgBotSessionStorage(dbContext, TimeProvider.System);
        var session = storage.CreateTgSession("session", botTokenHash: "token-hash");

        await storage.RecordUpdateAsync(
            session.Id,
            DateTime.UtcNow,
            "{\"update_id\":12345}",
            "Message",
            new UpdateOffset(12345),
            parentId: null,
            isProducedByBot: false,
            cancellationToken: CancellationToken.None);

        var ownRecordId = await storage.RecordUpdateAsync(
            session.Id,
            DateTime.UtcNow,
            "{\"message\":{\"text\":\"Text length: 4.\"}}",
            "Message",
            updateId: null,
            parentId: null,
            isProducedByBot: true,
            cancellationToken: CancellationToken.None);

        // Act
        var lastReceived = await storage.FindLastReceivedUpdateRecordIdAsync(
            session.Id,
            new UpdateOffset(12346),
            CancellationToken.None);

        var updatesForBot = (await storage.GetOrderedUpdatesAsync(
            session.Id,
            CancellationToken.None,
            fromIncluding: new TgSessionRecordId(lastReceived!.Value.Value + 1),
            excludeProducedByBot: true)).ToArray();

        var updatesIncludingOwn = (await storage.GetOrderedUpdatesAsync(
            session.Id,
            CancellationToken.None,
            fromIncluding: new TgSessionRecordId(lastReceived.Value.Value + 1))).ToArray();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(lastReceived.Value.Value, Is.Not.EqualTo(ownRecordId.Value));
            Assert.That(updatesForBot, Is.Empty);

            // The own update is still visible to TestController/AssertController.
            Assert.That(
                updatesIncludingOwn.Select(r => r.Id),
                Is.EqualTo([new TgSessionRecordId(ownRecordId.Value)]));
        });
    }

    /// <summary>
    /// When the bot already works with Emulator queue positions, the offset points
    /// directly at the last delivered update record.
    /// </summary>
    [Test]
    public async Task FindLastReceivedUpdateRecordId_EmulatorOffset_ReturnsPreviousRecord()
    {
        // Arrange
        await using var connection = await OpenInMemoryConnectionAsync();
        await using var dbContext = new TgBotSessionContext(CreateOptions(connection));
        var storage = new TgBotSessionStorage(dbContext, TimeProvider.System);
        var session = storage.CreateTgSession("session", botTokenHash: "token-hash");

        var first = await storage.RecordUpdateAsync(
            session.Id,
            DateTime.UtcNow,
            "{\"message\":{\"text\":\"qwerty\"}}",
            "Message",
            updateId: null,
            parentId: null,
            isProducedByBot: false,
            cancellationToken: CancellationToken.None);

        var own = await storage.RecordUpdateAsync(
            session.Id,
            DateTime.UtcNow,
            "{\"message\":{\"text\":\"Text length: 6.\"}}",
            "Message",
            updateId: null,
            parentId: null,
            isProducedByBot: true,
            cancellationToken: CancellationToken.None);

        // Act
        var lastReceived = await storage.FindLastReceivedUpdateRecordIdAsync(
            session.Id,
            new UpdateOffset(first.Value + 1),
            CancellationToken.None);

        var updatesForBot = (await storage.GetOrderedUpdatesAsync(
            session.Id,
            CancellationToken.None,
            fromIncluding: new TgSessionRecordId(lastReceived!.Value.Value + 1),
            excludeProducedByBot: true)).ToArray();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(lastReceived.Value.Value, Is.EqualTo(first.Value));
            Assert.That(lastReceived.Value.Value, Is.Not.EqualTo(own.Value));
            Assert.That(updatesForBot, Is.Empty);
        });
    }
}
