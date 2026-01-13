using TgBotApiEmulator.Contracts;

namespace TgBotApiEmulator.Storage.Contracts.Entities;

internal class Settings
{
    public SettingsId Id { get; set; }
    public bool IsActive { get; set; }
    public DateTime DtmCreated { get; set; }
    public TgSessionId? CurrentTgSessionId { get; set; }
}

internal class TgSession
{
    public TgSessionId Id { get; set; }
    public required string NonUniqueName { get; set; }
    public string? BotTokenHash { get; set; }
    public DateTime DtmCreated { get; set; }
    public DateTime DtmLastUpdated { get; set; }
    public ICollection<TgSessionRecord> Records { get; } = [];
}

internal abstract class TgSessionRecord
{
    public TgSessionRecordId Id { get; set; }
    public required TgSession Session { get; set; }
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
    public UpdateOffset Offset { get; set; }
}
