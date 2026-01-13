namespace TgBotApiEmulator.Exceptions;

public sealed class SessionDataCorruptedException : Exception
{
    public SessionDataCorruptedException() : base("Session data corrupted.")
    {
    }
}
