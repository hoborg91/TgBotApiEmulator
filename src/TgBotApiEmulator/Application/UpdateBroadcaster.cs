using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TgBotApiEmulator.Contracts;

namespace TgBotApiEmulator.Application;

internal sealed class UpdateBroadcaster : IUpdateBroadcaster
{
    private readonly IHubContext<SessionUpdatesHub> _hubContext;

    public UpdateBroadcaster(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        _hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<SessionUpdatesHub>>();
    }

    public Task BroadcastAsync(TgSessionId sessionId)
    {
        return _hubContext.Clients.Group(sessionId.ToString()).SendAsync("NewUpdate", new { });
    }
}
