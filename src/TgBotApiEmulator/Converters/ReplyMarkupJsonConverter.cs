using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgBotApiEmulator.Converters;

public class ReplyMarkupJsonConverter : JsonConverter<ReplyMarkup>
{
    public override ReplyMarkup Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var readerClone = reader;

        if (readerClone.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        readerClone.Read();
        if (readerClone.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException();
        }

        var propertyName = readerClone.GetString();
        var result = (ReplyMarkup)(propertyName switch
        {
            "force_reply" => JsonSerializer.Deserialize<ForceReplyMarkup>(ref reader)!,
            "is_persistent" => JsonSerializer.Deserialize<ReplyKeyboardMarkup>(ref reader)!,
            "inline_keyboard" => JsonSerializer.Deserialize<InlineKeyboardMarkup>(ref reader)!,
            "remove_keyboard" => JsonSerializer.Deserialize<ReplyKeyboardRemove>(ref reader)!,
            _ => throw new JsonException(),
        });

        return result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        ReplyMarkup replyMarkupValue,
        JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
