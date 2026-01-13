using TgBotApiEmulator.Application;
using TgBotApiEmulator.Contracts;

namespace TgBotApiEmulator.Tests;

public class UpdateReaderTests
{
    private readonly Mock<ISessionFacadeRead> _recordingProxyFacadeReadMock = new();
    private readonly Mock<IClock> _clockMock = new();
    private UpdateReader? _sut;
    private TgSessionId _tgSessionId = new(1); // TODO Add test(s?) for "wrong" session ID.

    private UpdateReader Sut => _sut ?? throw new InvalidOperationException("SUT is not initialized.");

    [SetUp]
    public void Setup()
    {
        _sut = new UpdateReader(
            _recordingProxyFacadeReadMock.Object,
            _clockMock.Object);
    }

    /// <summary>
    /// TEC-0301.UNT-0111. Happy path. Immediate result
    /// </summary>
    [Test]
    public async Task HappyPathImmediate()
    {
        // Arrange
        const int InitialOffsetValue = 1001;
        const double SomeMilliseconds = 100;
        var totalWaitingMilliseconds = 0.0;

        IReadOnlyList<(double Ms, IReadOnlyCollection<Telegram.Bot.Types.Update> Updates)> updates = [
            (SomeMilliseconds * 0, [Msg(InitialOffsetValue + 0, "Bad 1")]),
            (SomeMilliseconds * 0, [Msg(InitialOffsetValue + 1, "Bad 2")]),
            (SomeMilliseconds * 0, [Msg(InitialOffsetValue + 2, "Good")]),
            (SomeMilliseconds * 4, [Msg(InitialOffsetValue + 3, "Bad 3")]),
        ];

        _clockMock.SetupStopwatchAndDelay(
            () => totalWaitingMilliseconds,
            v => totalWaitingMilliseconds = v);

        _recordingProxyFacadeReadMock.SetupGetOrderedUpdatesAsync(
            () => totalWaitingMilliseconds,
            updates);

        // Act
        var (update, elapsed, offset) = await Sut.WaitForUpdateAsync(
            _tgSessionId,
            new(InitialOffsetValue),
            u => u is { Message: { } m } && m.Text == "Good",
            TimeSpan.FromMilliseconds(SomeMilliseconds * 10),
            CancellationToken.None
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(update, Is.EqualTo(updates[2].Updates.Single()));
            Assert.That(offset.Offset, Is.EqualTo(InitialOffsetValue + 2));
            Assert.That(elapsed.TotalMilliseconds, Is.EqualTo(0));
        });
    }

    /// <summary>
    /// TEC-0301.UNT-0121. Happy path. Delayed result
    /// </summary>
    [Test]
    public async Task HappyPathDelayed()
    {
        // Arrange
        const int InitialOffsetValue = 1001;
        const double SomeMilliseconds = 100;
        var totalWaitingMilliseconds = 0.0;

        IReadOnlyList<(double Ms, IReadOnlyCollection<Telegram.Bot.Types.Update> Updates)> updates = [
            (SomeMilliseconds * 1, [Msg(InitialOffsetValue + 0, "Bad 1")]),
            (SomeMilliseconds * 2, [Msg(InitialOffsetValue + 1, "Bad 2")]),
            (SomeMilliseconds * 3, [Msg(InitialOffsetValue + 2, "Good")]),
            (SomeMilliseconds * 4, [Msg(InitialOffsetValue + 3, "Bad 3")]),
        ];

        _clockMock.SetupStopwatchAndDelay(
            () => totalWaitingMilliseconds,
            v => totalWaitingMilliseconds = v);

        _recordingProxyFacadeReadMock.SetupGetOrderedUpdatesAsync(
            () => totalWaitingMilliseconds,
            updates);

        // Act
        var (update, elapsed, offset) = await Sut.WaitForUpdateAsync(
            _tgSessionId,
            new(InitialOffsetValue),
            u => u is { Message: { } m } && m.Text == "Good",
            TimeSpan.FromMilliseconds(SomeMilliseconds * 10),
            CancellationToken.None
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(update, Is.EqualTo(updates[2].Updates.Single()));
            Assert.That(offset.Offset, Is.EqualTo(InitialOffsetValue + 2));
            Assert.That(elapsed.TotalMilliseconds, Is.GreaterThan(0));
        });
    }

    /// <summary>
    /// TEC-0301.UNT-0211. Timeout. No message
    /// </summary>
    [Test]
    public async Task TimeoutWithNoMessage()
    {
        // Arrange
        const int InitialOffsetValue = 1001;
        const double SomeMilliseconds = 100;
        var totalWaitingMilliseconds = 0.0;

        _clockMock.SetupStopwatchAndDelay(
            () => totalWaitingMilliseconds,
            v => totalWaitingMilliseconds = v);

        _recordingProxyFacadeReadMock.SetupGetOrderedUpdatesAsync(
            () => totalWaitingMilliseconds,
            [
                (SomeMilliseconds * 1, [Msg(InitialOffsetValue + 0, "Bad 1")]),
                (SomeMilliseconds * 2, [Msg(InitialOffsetValue + 1, "Bad 2")]),
                (SomeMilliseconds * 3, [Msg(InitialOffsetValue + 2, "Bad 3")]),
                (SomeMilliseconds * 4, [Msg(InitialOffsetValue + 3, "Bad 4")]),
            ]);

        // Act
        var (update, elapsed, offset) = await Sut.WaitForUpdateAsync(
            _tgSessionId,
            new(InitialOffsetValue),
            u => u is { Message: { } m } && m.Text == "Good",
            TimeSpan.FromMilliseconds(SomeMilliseconds * 10),
            CancellationToken.None
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(update, Is.EqualTo(null));
            Assert.That(offset.Offset, Is.EqualTo(InitialOffsetValue + 3));
        });
    }

    /// <summary>
    /// TEC-0301.UNT-0221. Timeout. Message with earlier offset
    /// </summary>
    [Test]
    public async Task TimeoutWithEarlierOffset()
    {
        // Arrange
        const int InitialOffsetValue = 1001;
        const double SomeMilliseconds = 100;
        var totalWaitingMilliseconds = 0.0;

        _clockMock.SetupStopwatchAndDelay(
            () => totalWaitingMilliseconds,
            v => totalWaitingMilliseconds = v);

        _recordingProxyFacadeReadMock.SetupGetOrderedUpdatesAsync(
            () => totalWaitingMilliseconds,
            [
                (SomeMilliseconds * 1, [Msg(InitialOffsetValue + 0, "Good")]),
                (SomeMilliseconds * 2, [Msg(InitialOffsetValue + 1, "Bad 1")]),
                (SomeMilliseconds * 3, [Msg(InitialOffsetValue + 2, "Bad 2")]),
                (SomeMilliseconds * 4, [Msg(InitialOffsetValue + 3, "Bad 3")]),
            ]);

        // Act
        var (update, elapsed, offset) = await Sut.WaitForUpdateAsync(
            _tgSessionId,
            new(InitialOffsetValue),
            u => u is { Message: { } m } && m.Text == "Good",
            TimeSpan.FromMilliseconds(SomeMilliseconds * 10),
            CancellationToken.None
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(update, Is.EqualTo(null));
            Assert.That(offset.Offset, Is.EqualTo(InitialOffsetValue + 3));
        });
    }

    /// <summary>
    /// TEC-0301.UNT-0231. Timeout. Message appears after timeout
    /// </summary>
    [Test]
    public async Task TimeoutWithNonObservedMessage()
    {
        // Arrange
        const int InitialOffsetValue = 1001;
        const double SomeMilliseconds = 100;
        var totalWaitingMilliseconds = 0.0;

        _clockMock.SetupStopwatchAndDelay(
            () => totalWaitingMilliseconds,
            v => totalWaitingMilliseconds = v);

        _recordingProxyFacadeReadMock.SetupGetOrderedUpdatesAsync(
            () => totalWaitingMilliseconds,
            [
                (SomeMilliseconds * 1, [Msg(InitialOffsetValue + 0, "Bad 1")]),
                (SomeMilliseconds * 2, [Msg(InitialOffsetValue + 1, "Bad 2")]),
                (SomeMilliseconds * 3, [Msg(InitialOffsetValue + 2, "Bad 3")]),
                (SomeMilliseconds * 9, [Msg(InitialOffsetValue + 3, "Good")]),
            ]);

        // Act
        var (update, elapsed, offset) = await Sut.WaitForUpdateAsync(
            _tgSessionId,
            new(InitialOffsetValue),
            u => u is { Message: { } m } && m.Text == "Good",
            TimeSpan.FromMilliseconds(SomeMilliseconds * 5),
            CancellationToken.None
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(update, Is.EqualTo(null));
            Assert.That(offset.Offset, Is.EqualTo(InitialOffsetValue + 2));
        });
    }

    /// <summary>
    /// TEC-0301.UNT-0311. Check input. Offset
    /// </summary>
    [Test]
    public void CheckInputOffset()
    {
        // Arrange
        const int InitialOffsetValue = -1;
        var totalWaitingMilliseconds = 0.0;

        _clockMock.SetupStopwatchAndDelay(
            () => totalWaitingMilliseconds,
            v => totalWaitingMilliseconds = v);

        _recordingProxyFacadeReadMock.SetupGetOrderedUpdatesAsync(
            () => totalWaitingMilliseconds,
            []);

        // Act, Assert
        Assert.That(
            () => Sut.WaitForUpdateAsync(
                _tgSessionId,
                new(InitialOffsetValue),
                _ => true,
                TimeSpan.FromMilliseconds(1000),
                CancellationToken.None
            ),
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    /// <summary>
    /// TEC-0301.UNT-0321. Check input. Timeout
    /// </summary>
    [Test]
    public void CheckInputTimeout()
    {
        // Arrange
        const int InitialOffsetValue = 1001;
        var totalWaitingMilliseconds = 0.0;

        _clockMock.SetupStopwatchAndDelay(
            () => totalWaitingMilliseconds,
            v => totalWaitingMilliseconds = v);

        _recordingProxyFacadeReadMock.SetupGetOrderedUpdatesAsync(
            () => totalWaitingMilliseconds,
            []);

        // Act, Assert
        Assert.That(
            () => Sut.WaitForUpdateAsync(
                _tgSessionId,
                new(InitialOffsetValue),
                _ => true,
                TimeSpan.FromMilliseconds(1),
                CancellationToken.None
            ),
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    private static Telegram.Bot.Types.Update Msg(int offset, string text) => new()
    {
        Id = offset,
        Message = new Telegram.Bot.Types.Message
        {
            Chat = new Telegram.Bot.Types.Chat
            {
                Id = 1,
            },
            Text = text,
        },
    };
}
