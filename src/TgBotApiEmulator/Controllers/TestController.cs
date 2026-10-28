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
    IPostmanFacade postmanFacade,
    IRecordingProxyFacadeWrite facadeWrite) : Controller
{
    [HttpGet("makePostmanCollection/{botToken}")]
    public async Task<IActionResult> GetRecordsAsync(
        [FromRoute] string botToken,
        CancellationToken cancellationToken)
    {
        var sessionId = await GetSessionIdAsync(botToken, cancellationToken);

        var updates = await facade.GetOrderedUpdatesAsync(
            sessionId,
            cancellationToken);
        var bytes = postmanFacade.MakeUtf8Bytes(
            updates);
        return File(bytes, MediaTypeNames.Application.Json, "hello.json");
    }

    [HttpGet("sessions")]
    public async Task<IReadOnlyCollection<TgSession>> GetSessionsAsync(
        CancellationToken cancellationToken)
    {
        var sessions = await facade.GetSessionsAsync(cancellationToken);

        return [.. sessions.Select(s => new TgSession(s.Id, s.NonUniqueName, s.DtmCreated, s.DtmLastUpdated))];
    }

    [HttpPatch("sessions/{sessionId}/name")]
    public async Task<IActionResult> RenameSessionAsync(
        [FromRoute] long sessionId,
        [FromBody] RenameSessionRequest request,
        CancellationToken cancellationToken)
    {
        var session = await facadeWrite.RenameSessionAsync(
            new(sessionId),
            request.Name,
            cancellationToken);

        if (session is null)
        {
            return NotFound();
        }

        return Ok(new { name = session.NonUniqueName });
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

    [HttpGet("bot/{botToken}/updates")]
    public async Task<IReadOnlyCollection<Telegram.Bot.Types.Update>> GetSessionUpdatesAsync(
        string botToken,
        CancellationToken cancellationToken)
    {
        var sessionId = await GetSessionIdAsync(botToken, cancellationToken);

        var updates = await facade.GetOrderedUpdatesAsync(
            sessionId,
            cancellationToken);
        return updates;
    }

    private async Task<TgSessionId> GetSessionIdAsync(string botToken, CancellationToken cancellationToken)
    {
        var botTokenHash = HashHelper.Hash(botToken);

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
    string NonUniqueName,
    DateTime DtmCreated,
    DateTime DtmLastUpdated);

public record RenameSessionRequest(
    string Name);
