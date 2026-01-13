namespace TgBotApiEmulator;

internal sealed class Options
{
    public required ServerOperatingMode ServerOperatingMode { get; set; }
}

internal enum ServerOperatingMode
{
    Emulator,
    RecordingProxy,
}
