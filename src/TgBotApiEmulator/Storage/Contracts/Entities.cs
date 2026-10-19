using TgBotApiEmulator.Contracts;

namespace TgBotApiEmulator.Storage.Contracts.Entities;

internal class Settings
{
    public SettingsId Id { get; set; }
    public bool IsActive { get; set; }
    public DateTime DtmCreated { get; set; }
    public TgSessionId? CurrentTgSessionId { get; set; }
}

internal class BotOperatingMode
{
    public required string BotTokenHash { get; set; }
    public OperatingMode OperatingMode { get; set; }
}

internal class TgSession
{
    public TgSessionId Id { get; set; }
    public required string NonUniqueName { get; set; }
    public string? BotTokenHash { get; set; }
    public DateTime DtmCreated { get; set; }
    public DateTime DtmLastUpdated { get; set; }

    /// <summary>
    /// The primary key of the last update record returned to the bot.
    /// Unlike the Telegram update ID, this cursor also works for updates produced
    /// by the Emulator, which do not have a Telegram update ID.
    /// </summary>
    public TgSessionRecordId? LastReceivedUpdateRecordId { get; set; }

    public virtual ICollection<TgSessionRecord> Records { get; set; } = [];
}

internal abstract class TgSessionRecord
{
    public TgSessionRecordId Id { get; set; }
    public required virtual TgSession Session { get; set; }
    public TgSessionRecordId? ParentId { get; set; }
    public virtual TgSessionRecord? Parent { get; set; }
    public virtual ICollection<TgSessionRecord> Children { get; set; } = [];
    public DateTime Timestamp { get; set; }
}

internal class TgSessionBotActionRecord : TgSessionRecord
{
    public required string HttpMethod { get; set; }
    public required string TgBotApiMethod { get; set; }
    public required string QueryString { get; set; }
    public IReadOnlyDictionary<string, string>? Headers { get; set; }
    public required string? EscapedBody { get; set; }
}

internal class TgSessionServerResponseRecord : TgSessionRecord
{
    public required string StatusCode { get; set; }
    public required string EscapedContent { get; set; }
}

internal class TgSessionUpdateRecord : TgSessionRecord
{
    public required string EscapedContent { get; set; }
    public required string UpdateType { get; set; }

    /// <summary>
    /// The ID of the update in the Telegram update ID space, if the update came from
    /// Telegram servers. Locally produced updates do not have such an ID: their queue
    /// identity is the record primary key (<see cref="TgSessionRecord.Id"/>).
    /// </summary>
    public UpdateOffset? UpdateId { get; set; }

    /// <summary>
    /// Whether the update was produced by the bot itself (for example, a message sent
    /// via sendMessage or a reaction set via setMessageReaction).
    /// </summary>
    public bool IsProducedByBot { get; set; }
}
