namespace TgBotApiEmulator.Contracts;

public readonly record struct SettingsId(long Value)
{
    public override string ToString() => Value.ToString();
}

public readonly record struct TgSessionId(long Value)
{
    public override string ToString() => Value.ToString();
}

public readonly record struct TgSessionRecordId(long Value)
{
    public override string ToString() => Value.ToString();
}

public readonly record struct UpdateOffset(int Offset)
{
    public override string ToString() => Offset.ToString();
}

public static class StronglyTypedIdValue
{
    public static long InnerValue(this UpdateOffset value)
    {
        throw new NotImplementedException();
    }
}
