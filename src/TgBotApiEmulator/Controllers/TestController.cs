using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using TgBotApiEmulator.Application;
using TgBotApiEmulator.Application.Postman;
using TgBotApiEmulator.Contracts;

namespace TgBotApiEmulator.Controllers;

[ApiController]
[Route("test")]
public class TestController(
    ISessionFacadeRead facade,
    IPostmanFacade postmanFacade,
    IRecordingProxyFacadeWrite facadeWrite) : Controller
{
    [HttpGet("sessions")]
    public async Task<IReadOnlyCollection<TgSession>> GetSessionsAsync(
        CancellationToken cancellationToken)
    {
        var sessions = await facade.GetSessionsAsync(cancellationToken);

        return [.. sessions.Select(s => new TgSession(s.Id, s.NonUniqueName, s.DtmCreated, s.DtmLastUpdated, s.BotTokenHash, s.BotName, s.IsActive))];
    }

    [HttpDelete("sessions/{sessionId}")]
    public async Task<IActionResult> DeleteSessionAsync(
        [FromRoute] long sessionId,
        CancellationToken cancellationToken)
    {
        var deleted = await facadeWrite.DeleteSessionAsync(
            new(sessionId),
            cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
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
        long? sessionId,
        CancellationToken cancellationToken)
    {
        var resolvedSessionId = await ResolveSessionIdAsync(botToken, sessionId, cancellationToken);

        var updates = await facade.GetOrderedUpdatesAsync(
            resolvedSessionId,
            cancellationToken);
        return updates;
    }

    [HttpGet("bot/{botToken}/sessions")]
    public async Task<IReadOnlyCollection<TgSession>> GetSessionsByTokenAsync(
        string botToken,
        CancellationToken cancellationToken)
    {
        var sessions = await facade.GetSessionsAsync(
            cancellationToken,
            botTokenHash: botToken);

        return [.. sessions.Select(s => new TgSession(s.Id, s.NonUniqueName, s.DtmCreated, s.DtmLastUpdated, s.BotTokenHash, s.BotName, s.IsActive))];
    }

    [HttpPost("bot/{botTokenHash}/sessions")]
    public async Task<TgSession> CreateSessionByTokenAndSetActiveAsync(
        string botTokenHash,
        [FromBody] CreateSessionRequest request,
        CancellationToken cancellationToken)
    {
        var session = await facadeWrite.CreateSessionAsync(
            botTokenHash,
            request.Name,
            request.BotName,
            cancellationToken);
        return new TgSession(session.Id, session.NonUniqueName, session.DtmCreated, session.DtmLastUpdated, session.BotTokenHash, session.BotName, session.IsActive);
    }

    [HttpPost("bot/{botTokenHash}/sessions/{sessionId}/set-active")]
    public async Task<IActionResult> SetActiveSessionByTokenAsync(
        string botTokenHash,
        long sessionId,
        CancellationToken cancellationToken)
    {
        await facadeWrite.SetCurrentSessionAsync(
            botTokenHash,
            new TgSessionId(sessionId),
            cancellationToken);
        return Ok();
    }

    [HttpGet("bot/{botToken}/postman-collection")]
    public async Task<IActionResult> GetPostmanCollectionByTokenAsync(
        [FromRoute] string botToken,
        long? sessionId,
        CancellationToken cancellationToken)
    {
        var resolvedSessionId = await ResolveSessionIdAsync(botToken, sessionId, cancellationToken);
        var session = await facade.GetSessionAsync(resolvedSessionId, cancellationToken);
        return await GetPostmanCollectionAsync(session, cancellationToken);
    }

    [HttpGet("sessions/{sessionId}/postman-collection")]
    public async Task<IActionResult> GetPostmanCollectionBySessionIdAsync(
        [FromRoute] long sessionId,
        CancellationToken cancellationToken)
    {
        var session = await facade.GetSessionAsync(new(sessionId), cancellationToken);
        return await GetPostmanCollectionAsync(session, cancellationToken);
    }

    private async Task<IActionResult> GetPostmanCollectionAsync(
        Application.TgSession session,
        CancellationToken cancellationToken)
    {
        var updates = await facade.GetOrderedUpdatesAsync(
            session.Id,
            cancellationToken);
        var bytes = postmanFacade.MakeUtf8Bytes(updates);
        return File(bytes, MediaTypeNames.Application.Json, $"{session.NonUniqueName}.json");
    }

    private async Task<TgSessionId> ResolveSessionIdAsync(
        string botToken,
        long? sessionId,
        CancellationToken cancellationToken)
    {
        if (sessionId is { } id)
        {
            return (await facadeWrite.GetSessionAsync(new TgSessionId(id), cancellationToken)).Id;
        }

        return (await facadeWrite.GetOrCreateSessionAsync(botToken, cancellationToken)).Id;
    }
}

public record TgSession(
    TgSessionId Id,
    string NonUniqueName,
    DateTime DtmCreated,
    DateTime DtmLastUpdated,
    string? BotTokenHash,
    string? BotName,
    bool IsActive);

public record CreateSessionRequest(
    string Name,
    string? BotName);

public record RenameSessionRequest(
    string Name);
