// using System.Text.Json;
// using System.Text.Json.Serialization;
// using Telegram.Bot.Types;

// namespace TgBotApiEmulator.Converters;

// public class ReactionTypeConverter : JsonConverter<ReactionType>
// {
//     public override ReactionType Read(
//         ref Utf8JsonReader reader,
//         Type typeToConvert,
//         JsonSerializerOptions options)
//     {
//         Utf8JsonReader readerClone = reader;

//         if (readerClone.TokenType != JsonTokenType.StartObject)
//         {
//             throw new JsonException();
//         }

//         readerClone.Read();
//         if (readerClone.TokenType != JsonTokenType.PropertyName)
//         {
//             throw new JsonException();
//         }

//         string? propertyName = readerClone.GetString();
//         ReactionType result = propertyName switch
//         {
//             "emoji" => JsonSerializer.Deserialize<ReactionTypeEmoji>(ref reader)!,
//             "custom_emoji" => JsonSerializer.Deserialize<ReactionTypeCustomEmoji>(ref reader)!,
//             "paid" => JsonSerializer.Deserialize<ReactionTypePaid>(ref reader)!,
//             _ => throw new JsonException(),
//         };

//         return result;
//     }

//     public override void Write(
//         Utf8JsonWriter writer,
//         ReactionType replyMarkupValue,
//         JsonSerializerOptions options)
//     {
//         throw new NotImplementedException();
//     }
// }
