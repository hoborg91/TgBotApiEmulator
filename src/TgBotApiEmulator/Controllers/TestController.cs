using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using TgBotApiEmulator.Application;
using TgBotApiEmulator.Application.Postman;
using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Utilities;

namespace TgBotApiEmulator.Controllers;

[ApiController]
[Route("test")]
public class TestController(
    ISessionFacadeRead facade,
    IPostmanFacade postmanFacade,
    ISessionUpdateFacade facadeWrite) : Controller
{
    [HttpGet("sessions")]
    public async Task<IReadOnlyCollection<TgSession>> GetSessionsAsync(
        CancellationToken cancellationToken)
    {
        var sessions = await facade.GetSessionsAsync(cancellationToken);

        return [.. sessions.Select(s => new TgSession(s.Id, s.NonUniqueName, s.DtmCreated, s.DtmLastUpdated, s.BotTokenHash, s.BotName, s.TgUserId, s.IsActive))];
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

    [HttpGet("bot/{botToken}/sessions")]
    [HttpGet("bot/hash/{botTokenHash}/sessions")]
    public async Task<IReadOnlyCollection<TgSession>> GetSessionsByTokenAsync(
        [FromRoute] string? botToken,
        [FromRoute] string? botTokenHash,
        CancellationToken cancellationToken)
    {
        var hash = ResolveHash(botToken, botTokenHash);
        var sessions = await facade.GetSessionsAsync(
            cancellationToken,
            botTokenHash: hash);

        return [.. sessions.Select(s => new TgSession(s.Id, s.NonUniqueName, s.DtmCreated, s.DtmLastUpdated, s.BotTokenHash, s.BotName, s.TgUserId, s.IsActive))];
    }

    [HttpPost("bot/{botToken}/sessions")]
    [HttpPost("bot/hash/{botTokenHash}/sessions")]
    public async Task<TgSession> CreateSessionByTokenAndSetActiveAsync(
        [FromRoute] string? botToken,
        [FromRoute] string? botTokenHash,
        [FromBody] CreateSessionRequest request,
        CancellationToken cancellationToken)
    {
        var hash = ResolveHash(botToken, botTokenHash);
        var session = await facadeWrite.CreateSessionAsync(
            hash,
            request.Name,
            request.BotName,
            cancellationToken);
        return new TgSession(session.Id, session.NonUniqueName, session.DtmCreated, session.DtmLastUpdated, session.BotTokenHash, session.BotName, session.TgUserId, session.IsActive);
    }

    [HttpPost("bot/{botToken}/sessions/{sessionId}/set-active")]
    [HttpPost("bot/hash/{botTokenHash}/sessions/{sessionId}/set-active")]
    public async Task<IActionResult> SetActiveSessionByTokenAsync(
        [FromRoute] string? botToken,
        [FromRoute] string? botTokenHash,
        long sessionId,
        CancellationToken cancellationToken)
    {
        var hash = ResolveHash(botToken, botTokenHash);
        await facadeWrite.SetCurrentSessionAsync(
            hash,
            new TgSessionId(sessionId),
            cancellationToken);
        return Ok();
    }

    [HttpGet("bot/{botToken}/postman-collection")]
    [HttpGet("bot/hash/{botTokenHash}/postman-collection")]
    public async Task<IActionResult> GetPostmanCollectionByTokenAsync(
        [FromRoute] string? botToken,
        [FromRoute] string? botTokenHash,
        CancellationToken cancellationToken)
    {
        var hash = ResolveHash(botToken, botTokenHash);
        var resolvedSessionId = (await facadeWrite.GetOrCreateSessionAsync(hash, cancellationToken)).Id;
        var session = await facade.GetSessionAsync(resolvedSessionId, cancellationToken);
        return await GetPostmanCollectionAsync(session, cancellationToken);
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
}

public record TgSession(
    TgSessionId Id,
    string NonUniqueName,
    DateTime DtmCreated,
    DateTime DtmLastUpdated,
    string? BotTokenHash,
    string? BotName,
    long? TgUserId,
    bool IsActive);

public record CreateSessionRequest(
    string Name,
    string? BotName);

public record RenameSessionRequest(
    string Name);
