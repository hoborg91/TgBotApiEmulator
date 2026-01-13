using TgBotApiEmulator.Application;
using TgBotApiEmulator.Contracts;

namespace TgBotApiEmulator.Tests;

internal static class MockExtensions
{
    public static Mock<IClock> SetupStopwatchAndDelay(
        this Mock<IClock> clockMock,
        Func<double> getTotalWaitingMilliseconds,
        Action<double> setTotalWaitingMilliseconds)
    {
        var stopwatchMock = new Mock<IStopwatch>();
        stopwatchMock.Setup(s => s.Elapsed).Returns(() => TimeSpan.FromMilliseconds(getTotalWaitingMilliseconds()));

        clockMock
            .Setup(s => s.Delay(
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Callback(
                (TimeSpan delay,
                CancellationToken cancellationToken) =>
                {
                    setTotalWaitingMilliseconds(getTotalWaitingMilliseconds() + delay.TotalMilliseconds);
                });

        clockMock
            .Setup(s => s.StartStopwatch())
            .Returns(stopwatchMock.Object);

        return clockMock;
    }

    public static Mock<ISessionFacadeRead> SetupGetOrderedUpdatesAsync(
        this Mock<ISessionFacadeRead> recordingProxyFacadeReadMock,
        Func<double> totalWaitingMilliseconds,
        IReadOnlyCollection<(double Ms, IReadOnlyCollection<Telegram.Bot.Types.Update> Updates)> updates)
    {
        var doNotExist = updates.ToList();
        var exist = new List<Telegram.Bot.Types.Update>();

        recordingProxyFacadeReadMock
            .Setup(s => s.GetOrderedUpdatesAsync(
                It.IsAny<TgSessionId>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<UpdateOffset?>(),
                It.IsAny<string?>()
            ))
            .ReturnsAsync(
                (TgSessionId sessionId,
                CancellationToken cancellationToken,
                UpdateOffset? afterOffset,
                string? updateType) =>
                {
                    var ms = totalWaitingMilliseconds();
                    var newUpdates = doNotExist
                        .Where(u => u.Ms <= ms)
                        .ToList();
                    foreach (var newUpdate in newUpdates)
                    {
                        doNotExist.Remove(newUpdate);
                    }
                    exist.AddRange(newUpdates.SelectMany(nu => nu.Updates));

                    return [.. exist.Where(u => afterOffset == null || u.Id > afterOffset.Value.Offset)];
                });

        return recordingProxyFacadeReadMock;
    }
}
