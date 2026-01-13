using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using TgBotApiEmulator.Contracts;

namespace TgBotApiEmulator.Application;

public interface IMyAssert
{
    Task<WaitForUpdateResultBase> WaitForMessageFromBot(
        TgSessionId sessionId,
        UpdateOffset afterOffset,
        long chatId,
        Regex messageTextRegex,
        TimeSpan timeout,
        CancellationToken cancellationToken);

    Task<WaitForUpdateResultBase> WaitForMessageReactionFromBot(
        TgSessionId sessionId,
        UpdateOffset afterOffset,
        long chatId,
        int messageId,
        ReactionType reactionType,
        TimeSpan timeout,
        CancellationToken cancellationToken);
}

public enum AssertResult
{
    Success,
    Failure,
}

public abstract record WaitForUpdateResultBase(AssertResult Result);

internal sealed record WaitForUpdateResultSuccess(
    TimeSpan Elapsed,
    UpdateOffset SatisfyingUpdateOffset
) : WaitForUpdateResultBase(AssertResult.Success);

internal sealed record WaitForUpdateResultFailure(
    TimeSpan Elapsed,
    UpdateOffset LastObservedOffset,
    WaitForUpdateResultFailureReason FailureReason,
    string FailureDescription
) : WaitForUpdateResultBase(AssertResult.Failure);

internal enum WaitForUpdateResultFailureReason
{
    Timeout,
}

internal sealed class MyAssert(IUpdateReader updateReader) : IMyAssert
{
    public async Task<WaitForUpdateResultBase> WaitForMessageFromBot(
        TgSessionId sessionId,
        UpdateOffset afterOffset,
        long chatId,
        Regex messageTextRegex,
        TimeSpan timeout,
        CancellationToken cancellationToken
    ) =>
        await WaitForUpdate(
            sessionId,
            afterOffset,
            u => u is { Message: { } message } &&
                message.Chat.Id == chatId &&
                message.Text != null &&
                messageTextRegex.IsMatch(message.Text),
            timeout,
            cancellationToken
        );

    public async Task<WaitForUpdateResultBase> WaitForMessageReactionFromBot(
        TgSessionId sessionId,
        UpdateOffset afterOffset,
        long chatId,
        int messageId,
        ReactionType reactionType,
        TimeSpan timeout,
        CancellationToken cancellationToken
    )
    {
        var expectedEmojis = new[] { reactionType }
            .Select(rt =>
            {
                if (rt is not ReactionTypeEmoji emoji)
                {
                    throw new NotSupportedException();
                }

                return emoji.Emoji;
            })
            .OrderBy(i => i)
            .ToList();
        return await WaitForUpdate(
            sessionId,
            afterOffset,
            u =>
            {
                if (u is not { MessageReaction: { } mr })
                {
                    return false;
                }

                if (mr.Chat.Id != chatId || mr.MessageId != messageId)
                {
                    return false;
                }

                var updateEmojis = mr.NewReaction
                    .Select(rt =>
                    {
                        if (rt is not ReactionTypeEmoji emoji)
                        {
                            throw new NotSupportedException();
                        }

                        return emoji.Emoji;
                    })
                    .OrderBy(i => i)
                    .ToList();

                return updateEmojis.SequenceEqual(expectedEmojis);
            },
            timeout,
            cancellationToken
        );
    }

    private async Task<WaitForUpdateResultBase> WaitForUpdate(
        TgSessionId sessionId,
        UpdateOffset afterOffset,
        Predicate<Update> filter,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var (update, waitingTime, offset) = await updateReader.WaitForUpdateAsync(
            sessionId,
            afterOffset,
            filter,
            timeout,
            cancellationToken
        );

        if (update == null)
        {
            return new WaitForUpdateResultFailure(
                waitingTime,
                offset,
                WaitForUpdateResultFailureReason.Timeout,
                $"Timeout expired. Timeout: {timeout.TotalMilliseconds} ms. Elapsed: {waitingTime.TotalMilliseconds} ms.");
        }

        return new WaitForUpdateResultSuccess(
            waitingTime,
            offset);
    }
}
