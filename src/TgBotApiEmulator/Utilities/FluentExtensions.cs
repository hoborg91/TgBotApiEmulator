namespace TgBotApiEmulator.Utilities;

public static class Ext
{
    public static TOutput Then<TInput, TOutput>(this TInput input, Func<TInput, TOutput> mapper) => mapper(input);
}
