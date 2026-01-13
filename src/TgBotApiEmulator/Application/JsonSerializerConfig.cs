using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using TgBotApiEmulator.Converters;

namespace TgBotApiEmulator.Application;

internal sealed class JsonSerializerConfig
{
    public void Configure(JsonOptions options)
    {
        JsonBotAPI.Configure(options.JsonSerializerOptions);
        options.JsonSerializerOptions.Converters.Add(new ReplyMarkupJsonConverter());
    }
}
