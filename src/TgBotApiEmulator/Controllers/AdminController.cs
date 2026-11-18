using Microsoft.AspNetCore.Mvc;
using TgBotApiEmulator.Application;
using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Utilities;

namespace TgBotApiEmulator.Controllers;

[ApiController]
[Route("admin")]
public sealed class AdminController(
    IOperatingModeFacade operatingModeFacade) : ControllerBase
{
    [HttpGet("operatingMode/{botToken}")]
    [HttpGet("operatingMode/hash/{botTokenHash}")]
    public async Task<OperatingMode> GetOperatingModeAsync(
        [FromRoute] string? botToken,
        [FromRoute] string? botTokenHash,
        CancellationToken cancellationToken)
    {
        var hash = ResolveHash(botToken, botTokenHash);
        return await operatingModeFacade.GetOperatingModeAsync(hash, cancellationToken);
    }

    [HttpPut("operatingMode/{botToken}/{operatingMode}")]
    [HttpPut("operatingMode/hash/{botTokenHash}/{operatingMode}")]
    public async Task<OperatingMode> SetOperatingModeAsync(
        [FromRoute] string? botToken,
        [FromRoute] string? botTokenHash,
        [FromRoute] OperatingMode operatingMode,
        CancellationToken cancellationToken)
    {
        var hash = ResolveHash(botToken, botTokenHash);
        await operatingModeFacade.SetOperatingModeAsync(hash, operatingMode, cancellationToken);

        return operatingMode;
    }

    private static string ResolveHash(string? botToken, string? botTokenHash)
    {
        if (!string.IsNullOrWhiteSpace(botTokenHash))
        {
            return botTokenHash;
        }

        if (!string.IsNullOrWhiteSpace(botToken))
        {
            return HashHelper.Hash(botToken);
        }

        throw new ArgumentException("Either botToken or botTokenHash must be specified.");
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
