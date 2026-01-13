using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using TgBotApiEmulator.Application;
using TgBotApiEmulator.Application.Postman;
using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Exceptions;
using TgBotApiEmulator.Utilities;

namespace TgBotApiEmulator.Controllers;

[ApiController]
[Route("test")]
public class TestController(
    ISessionFacadeRead facade,
    IPostmanFacade postmanFacade) : Controller
{
    [HttpGet("makePostmanCollection/{botTokenString}")]
    public async Task<IActionResult> GetRecordsAsync(
        string botTokenString,
        CancellationToken cancellationToken)
    {
        var sessionId = await GetSessionIdAsync(botTokenString, cancellationToken);

        var updates = await facade.GetOrderedUpdatesAsync(
            sessionId,
            cancellationToken);
        var bytes = postmanFacade.MakeUtf8Bytes(
            botTokenString,
            updates);
        return File(bytes, MediaTypeNames.Application.Json, "hello.json");
    }

    [HttpGet("sessions")]
    public async Task<IReadOnlyCollection<TgSession>> GetSessionsAsync(
        CancellationToken cancellationToken)
    {
        var sessions = await facade.GetSessionsAsync(cancellationToken);

        return [.. sessions.Select(s => new TgSession(s.Id, s.NonUniqueName))];
    }

    [HttpGet("sessions/{sessionId}/updates")]
    public async Task<IReadOnlyCollection<Telegram.Bot.Types.Update>> GetSessionUpdatesAsync(
        long sessionId,
        CancellationToken cancellationToken)
    {
        var updates = await facade.GetOrderedUpdatesAsync(
            new(sessionId),
            cancellationToken);
        return updates;
    }

    [HttpGet("bot/{botTokenString}/updates")]
    public async Task<IReadOnlyCollection<Telegram.Bot.Types.Update>> GetSessionUpdatesAsync(
        string botTokenString,
        CancellationToken cancellationToken)
    {
        var sessionId = await GetSessionIdAsync(botTokenString, cancellationToken);

        var updates = await facade.GetOrderedUpdatesAsync(
            sessionId,
            cancellationToken);
        return updates;
    }

    private async Task<TgSessionId> GetSessionIdAsync(string botTokenString, CancellationToken cancellationToken)
    {
        var botTokenHash = HashHelper.Hash(botTokenString);

        var sessions = await facade.GetSessionsAsync(cancellationToken, botTokenHash: botTokenHash);
        if (sessions.Count == 0)
        {
            throw new Exception("There is no record for the given bot token.");
        }

        if (sessions.Count > 1)
        {
            throw new SessionDataCorruptedException();
        }
        return sessions.Single().Id;
    }
}

public record TgSession(
    TgSessionId Id,
    string NonUniqueName);
