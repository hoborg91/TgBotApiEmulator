using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using TgBotApiEmulator.Application;
using TgBotApiEmulator.Controllers;
using TgBotApiEmulator.Storage;
using TgBotApiEmulator.Storage.Contracts;
using TgBotApiEmulator.Utilities;
using Assert = NUnit.Framework.Assert;

namespace TgBotApiEmulator.Tests;

/// <summary>
/// Regression tests for BUG-1: a bot must not receive its own messages.
/// </summary>
public class BotControllerGetUpdatesTests
{
    private const string Token = "123456:token";
    private const string BotPrefixedToken = "bot" + Token;

    private SqliteConnection? _connection;
    private TgBotSessionContext? _dbContext;
    private RecordingProxyFacade? _facade;
    private BotController? _sut;
    private TgBotApiEmulator.Contracts.TgSessionId _sessionId;

    private BotController Sut => _sut ?? throw new InvalidOperationException("SUT is not initialized.");

    private RecordingProxyFacade Facade => _facade ?? throw new InvalidOperationException("Facade is not initialized.");

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
        var jsonOptions = new OptionsWrapper<JsonOptions>(new JsonOptions());
        new JsonSerializerConfig().Configure(jsonOptions.Value);

        _facade = new RecordingProxyFacade(
            storage,
            settingsStorageMock.Object,
            jsonOptions);

        _sessionId = (await _facade.GetOrCreateSessionAsync(
            HashHelper.Hash(Token),
            CancellationToken.None)).Id;

        var clockMock = new Mock<IClock>();
        var totalWaitingMilliseconds = 0.0;
        clockMock.SetupStopwatchAndDelay(
            () => totalWaitingMilliseconds,
            v => totalWaitingMilliseconds = v);

        var updateReader = new UpdateReader(_facade, clockMock.Object);

        _sut = new BotController(_facade, _facade, updateReader);
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
    /// BUG-1 reproduction. The bot has just switched from recording proxy mode and
    /// passes the offset from the real Telegram ID space. It must not receive the
    /// reply it produced itself while recording.
    /// </summary>
    [Test]
    public async Task GetUpdates_AfterSwitchingFromRecording_DoesNotReturnOwnMessage()
    {
        // Arrange: a natural user update and the bot's own reply, both recorded.
        await Facade.RecordUpdateAsync(
            _sessionId,
            new Update
            {
                Id = 12345,
                Message = UserMessage("asdf"),
            },
            CancellationToken.None);

        await Facade.RecordUpdateAsync(
            _sessionId,
            new Update
            {
                Message = BotMessage("Text length: 4."),
            },
            CancellationToken.None,
            isProducedByBot: true);

        // Act: the bot asks for updates starting from its last Telegram ID + 1.
        var response = await Sut.GetUpdatesAsync(
            BotPrefixedToken,
            new GetUpdatesRequest { Offset = 12346, Timeout = 1 },
            CancellationToken.None);

        // Assert
        Assert.That(response.Ok, Is.True);
        Assert.That(response.Result, Is.Empty);

        // The own update is still visible through TestController/AssertController.
        var allUpdates = await Facade.GetOrderedUpdatesAsync(
            _sessionId,
            CancellationToken.None);
        Assert.That(
            allUpdates.Any(u => u.Message?.Text == "Text length: 4."),
            Is.True);
    }

    /// <summary>
    /// A message produced by the Emulator must reach the bot, while updates produced
    /// by the bot itself must stay invisible to it.
    /// </summary>
    [Test]
    public async Task GetUpdates_EmulatorMessage_IsReturnedAndOwnMessagesAreSkipped()
    {
        // Arrange: an emulated user message followed by a reply produced by the bot.
        var emulated = await Facade.RecordUpdateAsync(
            _sessionId,
            new Update
            {
                Message = UserMessage("qwerty"),
            },
            CancellationToken.None);

        await Facade.RecordUpdateAsync(
            _sessionId,
            new Update
            {
                Message = BotMessage("Text length: 6."),
            },
            CancellationToken.None,
            isProducedByBot: true);

        // Act: a fresh bot calls getUpdates without an offset.
        var firstResponse = await Sut.GetUpdatesAsync(
            BotPrefixedToken,
            new GetUpdatesRequest { Offset = null, Timeout = 1 },
            CancellationToken.None);

        // Assert
        Assert.That(firstResponse.Result, Has.Length.EqualTo(1));
        Assert.That(firstResponse.Result![0].Message?.Text, Is.EqualTo("qwerty"));
        Assert.That(firstResponse.Result[0].Id, Is.EqualTo(emulated.Value));

        // Act: the bot confirms the received update and asks for the next one.
        var secondResponse = await Sut.GetUpdatesAsync(
            BotPrefixedToken,
            new GetUpdatesRequest { Offset = firstResponse.Result[0].Id + 1, Timeout = 1 },
            CancellationToken.None);

        // Assert: the bot's own reply is not returned.
        Assert.That(secondResponse.Result, Is.Empty);
    }

    private static Message UserMessage(string text) => new()
    {
        Text = text,
        From = new User
        {
            Id = 11,
            Username = "testUser",
        },
        Chat = new Chat
        {
            Id = 1,
            Type = Telegram.Bot.Types.Enums.ChatType.Private,
        },
    };

    private static Message BotMessage(string text) => new()
    {
        Text = text,
        From = new User
        {
            Id = 1001,
            Username = "bot",
            IsBot = true,
        },
        Chat = new Chat
        {
            Id = 1,
            Type = Telegram.Bot.Types.Enums.ChatType.Private,
        },
    };
}
