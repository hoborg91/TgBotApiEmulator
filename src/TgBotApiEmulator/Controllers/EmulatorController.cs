using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using TgBotApiEmulator.Application;
using TgBotApiEmulator.Utilities;

namespace TgBotApiEmulator.Controllers;

[ApiController]
[Route("emulate")]
public sealed class EmulatorController(
    IRecordingProxyFacadeWrite facade) : ControllerBase
{
    [HttpPost("message")]
    public async Task<MessageResult> MessageAsync(
        MessageRequest request,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"message({request.Text})...");

        var botTokenHash = HashHelper.Hash(request.BotToken);
        var sessionId = (await facade.GetOrCreateSessionAsync(botTokenHash, cancellationToken)).Id;

        var updateId = await facade.RecordUpdateAsync(
            sessionId,
            new Update
            {
                Message = new Message
                {
                    Text = request.Text,
                    From = new User
                    {
                        Id = 11,
                        Username = "testUser",
                    },
                    Chat = new Chat
                    {
                        Id = request.ChatId,
                        Type = Telegram.Bot.Types.Enums.ChatType.Group,
                    },
                },
            },
            cancellationToken);

        Console.WriteLine($"message returns");
        return new(updateId);
    }
}

public record MessageRequest(string BotToken, long ChatId, string Text);

public record MessageResult(long Offset);
