namespace TgBotApiEmulator.Contracts;

public enum OrderingDirection
{
    Ascending,
    Descending,
}

public enum SessionOrderingField
{
    Id,
    NonUniqueName,
    DtmCreated,
    DtmLastUpdated,
}

public record SessionOrdering(SessionOrderingField Field, OrderingDirection Direction);
