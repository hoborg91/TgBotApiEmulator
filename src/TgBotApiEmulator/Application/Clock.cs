using System.Diagnostics;

namespace TgBotApiEmulator.Application;

internal interface IClock
{
    Task Delay(TimeSpan delay, CancellationToken cancellationToken);
    IStopwatch StartStopwatch();
}

internal interface IStopwatch
{
    TimeSpan Elapsed { get; }
}

internal sealed class Clock : IClock
{
    public async Task Delay(TimeSpan delay, CancellationToken cancellationToken)
    {
        await Task.Delay(delay, cancellationToken);
    }

    public IStopwatch StartStopwatch()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        return new StopwatchWrapper(stopwatch);
    }

    private sealed class StopwatchWrapper(Stopwatch stopwatch) : IStopwatch
    {
        public TimeSpan Elapsed => stopwatch.Elapsed;
    }
}
