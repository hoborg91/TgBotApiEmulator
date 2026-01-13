// using TgBotApiEmulator.Storage.Contracts;

// namespace TgBotApiEmulator.Application;

// public interface ITestUtilities
// {
//     long CommitedOffset { get; }
//     Task CommitOffsetAsync(CancellationToken cancellationToken, long? offset = null);
// }

// internal sealed class TestUtilities(ITgBotSessionStorage storage) : ITestUtilities
// {
//     public long CommitedOffset { get; private set; } = -1;

//     public async Task CommitOffsetAsync(CancellationToken cancellationToken, long? offset = null)
//     {
//         if (offset == null)
//         {
//             var mostRecentRecord = await storage.GetMostRecentRecordAsync(cancellationToken);
//             CommitedOffset = mostRecentRecord?.Offset ?? -1;
//         }
//         else
//         {
//             CommitedOffset = offset.Value;
//         }
//     }
// }
