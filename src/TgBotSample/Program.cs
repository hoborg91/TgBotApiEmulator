using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var token = Environment.GetEnvironmentVariable("TOKEN");

if (string.IsNullOrWhiteSpace(token))
{
    Console.WriteLine("Bot token is not set. You can set token in either of the following ways.");
    Console.WriteLine("- Set environment variable TOKEN. Example for PowerShell: $Env:TOKEN = \"1234567891:abcdefghijklmnopqrstuvwxyz123456789\"");
    Console.WriteLine("- If you use IDE, set token in corresponding settings. Example: Project Properties > Debug > Launch profiles UI > Environment variables.");
    Console.WriteLine("- If you has access to the source code, hardcode it (not recommended).");
    return;
}

var cmdLineArgs = Environment.GetCommandLineArgs();
var baseUrl = cmdLineArgs.Length > 1
    ? cmdLineArgs[1]
    : "https://api.telegram.org";

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(
    new TelegramBotClientOptions(token, baseUrl),
    cancellationToken: cts.Token);

var me = await bot.GetMe();
await bot.DeleteWebhook();
await bot.DropPendingUpdates();

bot.OnError += OnError;
bot.OnMessage += OnMessage;

Console.WriteLine($"@{me.Username} is running... Press Escape to terminate");
while (Console.ReadKey(true).Key != ConsoleKey.Escape)
{
}
cts.Cancel();

async Task OnError(Exception exception, HandleErrorSource source)
{
    Console.WriteLine(exception);
    await Task.Delay(2000, cts.Token); // delay for 2 seconds before eventually trying again
}

async Task OnMessage(Message msg, UpdateType type)
{
    if (msg.Text is not { } text)
    {
        Console.WriteLine($"Received a message of type {msg.Type}");
        return;
    }

    Console.WriteLine($"Received message: {text}");

    await bot.SendMessage(msg.Chat, $"Text length: {text.Length}.");
}
