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

    public static bool operator <(TgSessionRecordId a, TgSessionRecordId b) => throw new EvaluationNotExpectedException();

    public static bool operator >(TgSessionRecordId a, TgSessionRecordId b) => throw new EvaluationNotExpectedException();
}

/// <summary>
/// The identifier of an update in the Telegram update ID space (the ID space the bot operates in).
/// Stored separately from the database record primary key.
/// </summary>
public readonly record struct UpdateOffset(long Value)
{
    public override string ToString() => Value.ToString();

    public static bool operator <(UpdateOffset a, UpdateOffset b) => throw new EvaluationNotExpectedException();

    public static bool operator >(UpdateOffset a, UpdateOffset b) => throw new EvaluationNotExpectedException();

    public static bool operator <=(UpdateOffset a, UpdateOffset b) => throw new EvaluationNotExpectedException();

    public static bool operator >=(UpdateOffset a, UpdateOffset b) => throw new EvaluationNotExpectedException();
}

public static class StronglyTypedIdValue
{
    public static long InnerValue(this TgSessionRecordId value)
    {
        throw new NotImplementedException();
    }

    public static long InnerValue(this UpdateOffset value)
    {
        throw new NotImplementedException();
    }
}

public class EvaluationNotExpectedException() : NotImplementedException(
    "It was expected that the method would be interpreted only as a part of expression. " +
    "The real execution of this method was not expected.");