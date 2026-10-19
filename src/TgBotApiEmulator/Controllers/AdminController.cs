using Microsoft.AspNetCore.Mvc;
using TgBotApiEmulator.Application;
using TgBotApiEmulator.Contracts;

namespace TgBotApiEmulator.Controllers;

[ApiController]
[Route("admin")]
public sealed class AdminController(
    IOperatingModeFacade operatingModeFacade) : ControllerBase
{
    [HttpGet("operatingMode/{botToken}")]
    public async Task<OperatingMode> GetOperatingModeAsync(
        string botToken,
        CancellationToken cancellationToken)
    {
        return await operatingModeFacade.GetOperatingModeAsync(botToken, cancellationToken);
    }

    [HttpPut("operatingMode/{botToken}/{operatingMode}")]
    public async Task<OperatingMode> SetOperatingModeAsync(
        [FromRoute] string botToken,
        [FromRoute] OperatingMode operatingMode,
        CancellationToken cancellationToken)
    {
        await operatingModeFacade.SetOperatingModeAsync(botToken, operatingMode, cancellationToken);

        return operatingMode;
    }

    // [HttpPut("currentSession/{sessionId:long}")]
    // public async Task<string> SetCurrentSession(
    //     [FromRoute] long sessionId)
    // {
    //     Console.WriteLine($"currentSession/{sessionId}");
    //     await Task.CompletedTask;

    //     return "Ok";
    // }
}
