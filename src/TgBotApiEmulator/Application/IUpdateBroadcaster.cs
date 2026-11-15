using System.Threading.Tasks;
using TgBotApiEmulator.Contracts;

namespace TgBotApiEmulator.Application;

public interface IUpdateBroadcaster
{
    Task BroadcastAsync(TgSessionId sessionId);
}
