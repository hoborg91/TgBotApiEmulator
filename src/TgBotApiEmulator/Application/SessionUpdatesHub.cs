using Microsoft.AspNetCore.SignalR;

namespace TgBotApiEmulator.Application;

public sealed class SessionUpdatesHub : Hub
{
    public Task JoinSession(long sessionId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, sessionId.ToString());
    }

    public Task LeaveSession(long sessionId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId.ToString());
    }
}
