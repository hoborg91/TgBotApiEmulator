using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using TgBotApiEmulator.Application;
using TgBotApiEmulator.Storage.Contracts;
using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Storage;
using TgBotApiEmulator.Utilities;

namespace TgBotApiEmulator.Tests;

public class UpdateNotifierIntegrationTests
{
    private SqliteConnection? _connection;
    private TgBotSessionContext? _dbContext;
    private RecordingProxyFacade? _facade;
    private UpdateNotifier? _notifier;
    private UpdateReader? _reader;
    private TgSessionId _sessionA;
    private TgSessionId _sessionB;

    [SetUp]
    public async Task SetUp()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<TgBotSessionContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new TgBotSessionContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        var storage = new TgBotSessionStorage(_dbContext, TimeProvider.System);
        var settingsStorageMock = new Mock<ISettingsStorage>();
        var jsonOptions = new OptionsWrapper<Microsoft.AspNetCore.Mvc.JsonOptions>(new Microsoft.AspNetCore.Mvc.JsonOptions());
        new JsonSerializerConfig().Configure(jsonOptions.Value);

        _facade = new RecordingProxyFacade(
            storage,
            settingsStorageMock.Object,
            jsonOptions);

        _notifier = new UpdateNotifier();
        _reader = new UpdateReader(_facade, new Clock(), _notifier);

        _sessionA = (await _facade.GetOrCreateSessionAsync(HashHelper.Hash("tokenA"), CancellationToken.None)).Id;
        _sessionB = (await _facade.GetOrCreateSessionAsync(HashHelper.Hash("tokenB"), CancellationToken.None)).Id;
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }

        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
    }

    /// <summary>
    /// A waiter is woken promptly when an update for its session is recorded and
    /// Notify is called, instead of polling to timeout.
    /// </summary>
    [Test]
    public async Task WaitForUpdateAsync_ReturnsPromptlyWhenNotified()
    {
        // Arrange
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var waitTask = _reader!.WaitForUpdateAsync(
            _sessionA,
            new TgSessionRecordId(0),
            _ => true,
            TimeSpan.FromSeconds(10),
            cts.Token);

        // Act: record an update for session A and notify.
        await _facade!.RecordUpdateAsync(
            _sessionA,
            new Update { Message = new Message { Text = "hello" } },
            CancellationToken.None);
        _notifier!.Notify(_sessionA);

        // Assert: the waiter completes promptly with the update.
        var (update, elapsed, offset) = await waitTask;
        Assert.That(update, Is.Not.Null);
        Assert.That(update!.Message?.Text, Is.EqualTo("hello"));
        Assert.That(elapsed.TotalMilliseconds, Is.LessThan(1000));
    }

    /// <summary>
    /// A waiter for one session is not woken by an update recorded for another session.
    /// </summary>
    [Test]
    public async Task WaitForUpdateAsync_PerSessionScope_DoesNotWakeOnOtherSession()
    {
        // Arrange: start a waiter for session A with nothing recorded for it yet.
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var waitTask = _reader!.WaitForUpdateAsync(
            _sessionA,
            new TgSessionRecordId(0),
            _ => true,
            TimeSpan.FromSeconds(10),
            cts.Token);

        // Act: record and notify for session B only.
        await _facade!.RecordUpdateAsync(
            _sessionB,
            new Update { Message = new Message { Text = "for B" } },
            CancellationToken.None);
        _notifier!.Notify(_sessionB);

        // Assert: the waiter for session A stays un-woken for a short window
        // (updates for another session must not wake it). The waiter is blocked on
        // the notifier during this delay, so no concurrent EF access occurs.
        await Task.Delay(250);
        Assert.That(waitTask.IsCompleted, Is.False, "Session A waiter was woken by a notification for session B.");

        // Now record an update for session A and notify session A -> it completes.
        await _facade!.RecordUpdateAsync(
            _sessionA,
            new Update { Message = new Message { Text = "for A" } },
            CancellationToken.None);
        _notifier!.Notify(_sessionA);

        var (update, _, _) = await waitTask;
        Assert.That(update, Is.Not.Null);
        Assert.That(update!.Message?.Text, Is.EqualTo("for A"));
    }

    /// <summary>
    /// SubscribeAsync completes when Notify is called after subscribing.
    /// </summary>
    [Test]
    public async Task SubscribeAsync_ThenNotify_ReturnsCompleted()
    {
        // Arrange
        var task = _notifier!.SubscribeAsync(_sessionA, CancellationToken.None);
        Assert.That(task.IsCompleted, Is.False);

        // Act
        _notifier!.Notify(_sessionA);

        // Assert
        Assert.That(task.IsCompleted, Is.True);
    }
}
