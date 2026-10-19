using System.Text.Json.Serialization;

namespace TgBotApiEmulator.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OperatingMode
{
    RecordingProxy,
    Emulator,
}