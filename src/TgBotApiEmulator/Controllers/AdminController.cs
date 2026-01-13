using Microsoft.AspNetCore.Mvc;

namespace TgBotApiEmulator.Controllers;

[ApiController]
[Route("admin")]
public sealed class AdminController() : ControllerBase
{
    [HttpPut("currentSession/{sessionId:long}")]
    public async Task<string> SetCurrentSession(
        [FromRoute] long sessionId)
    {
        Console.WriteLine($"currentSession/{sessionId}");
        await Task.CompletedTask;

        return "Ok";
    }
}
