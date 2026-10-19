using System.Globalization;
using System.Text.Json;
using Telegram.Bot.Types;
using TgBotApiEmulator.Controllers;

namespace TgBotApiEmulator.Application.Postman;

public interface IPostmanFacade
{
    byte[] MakeUtf8Bytes(
        IReadOnlyCollection<Update> updates);
}

internal sealed class PostmanFacade(
    IEndpointRegistry endpointRegistry) : IPostmanFacade
{
    private readonly JsonSerializerOptions _opts1 = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NewLine = "\r\n",
        WriteIndented = true,
        IndentCharacter = ' ',
        IndentSize = 2,// 4,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly JsonSerializerOptions _optsMain = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NewLine = "\n",
        WriteIndented = true,
        IndentCharacter = '\t',
        IndentSize = 1,// 4,

        // UnsafeRelaxedJsonEscaping - to turn \u0022 into ("). Source: https://stackoverflow.com/questions/67189964/system-text-json-jsonserializer-serialize-adds-u0022
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public byte[] MakeUtf8Bytes(
        IReadOnlyCollection<Update> updates)
    {
        var emulatorHostVariableName = "emulatorUrl";
        var recentOffsetVariableName = "recentOffset";
        var botTokenVariableName = "botToken";
        var postmanItems = updates
            .Select(u => ToPostmanItem(
                u,
                "{{" + emulatorHostVariableName + "}}",
                "{{" + botTokenVariableName + "}}",
                recentOffsetVariableName))
            .ToList();

        var myCollection = new PostmanCollection(
            new PostmanInfo(
                "My.Test",
                new Uri("https://schema.getpostman.com/json/collection/v2.1.0/collection.json")),
            [.. postmanItems],
            [
                new PostmanSingleVariable(
                    emulatorHostVariableName,
                    "",
                    "default"
                ),
                new PostmanSingleVariable(
                    recentOffsetVariableName,
                    "",
                    "string"
                ),
                new PostmanSingleVariable(
                    botTokenVariableName,
                    "",
                    "string"
                ),
            ]
        );

        var bytes = JsonSerializer.SerializeToUtf8Bytes(
            myCollection,
            _optsMain);

        return bytes;
    }

    private PostmanRequestUrl GetPostmanRequestUrlForEmulateMessage(string host) =>
        GetPostmanRequestUrl(host, endpointRegistry.GetEmulatorMessage());

    private PostmanRequestUrl GetPostmanRequestUrlForAssertBotShouldSendMessage(string host) =>
        GetPostmanRequestUrl(host, endpointRegistry.GetAssertBotShouldSendMessage());

    private PostmanRequestUrl GetPostmanRequestUrlForAssertBotShouldSetMessageReaction(string host) =>
        GetPostmanRequestUrl(host, endpointRegistry.GetAssertBotShouldSetMessageReaction());

    private static PostmanRequestUrl GetPostmanRequestUrl(string host, RouteEndpoint endpoint)
    {
        return new PostmanRequestUrl(
            $"{host}/{endpoint.RoutePattern.RawText}",
            [host],
            endpoint.RoutePattern.RawText!.Split('/') // TODO Explore endpoint.RoutePattern.PathSegments instead of RawText.Split('/')
        );
    }

    private PostmanSingleItem ToPostmanItem(
        Update update,
        string emulatorHostTemplate,
        string botTokenTemplate,
        string recentOffsetVariableName)
    {
        if (update is { Message: { } userMessage } && (userMessage.From?.IsBot == false))
        {
            return new PostmanSingleItem(
                $"User: {userMessage.Text}",
                [
                    new PostmanSingleEvent(
                        "test",
                        new PostmanScript(
                            [
                                "pm.test(\"Status code is 200\", function () {\r",
                                "    pm.response.to.have.status(200);\r",
                                "});",
                                "let jsonData = pm.response.json();",
                                $"pm.collectionVariables.set(\"{recentOffsetVariableName}\", jsonData.offset);\r",
                            ],
                            "text/javascript",
                            new object())),
                ],
                new PostmanRequest(
                    "POST",
                    [],
                    new PostmanRequestBody(
                        "raw",
                        JsonSerializer.Serialize(
                            new MessageRequest(botTokenTemplate, 1, userMessage.Text ?? throw new NotSupportedException()),
                            _opts1),
                        new PostmanRequestBodyOptions(
                            new PostmanRequestBodyOptionsRaw("json")
                        )),
                    GetPostmanRequestUrlForEmulateMessage(emulatorHostTemplate)),
                []);
        }

        if (update is { Message: { } botMessage } && (botMessage.From?.IsBot == true))
        {
            var anchor = -87398234; // TODO Bad thing. Refactor.
            var body = JsonSerializer.Serialize(
                new MessageAssert(
                    BotToken: botTokenTemplate,
                    AfterOffset: anchor,
                    ChatId: 1,
                    MessageRegexPattern: botMessage.Text ?? ".*",
                    TimeoutMilliseconds: 5000
                ),
                _opts1);
            body = body.Replace(
                anchor.ToString(CultureInfo.InvariantCulture),
                "{{" + recentOffsetVariableName + "}}");

            return new PostmanSingleItem(
                $"Bot should reply \"{botMessage.Text}\"",
                [
                    new PostmanSingleEvent(
                        "test",
                        new PostmanScript(
                            [
                                "pm.test(\"Status code is 200\", function () {\r",
                                "    pm.response.to.have.status(200);\r",
                                "});\r",
                                "\r",
                                "pm.test(\"Result is 'Success'\", function () {\r",
                                "    var jsonData = pm.response.json();\r",
                                "    pm.expect(jsonData.Result).to.eql(\"Success\");\r",
                                "});\r",
                                "",
                            ],
                            "text/javascript",
                            new object()
                        )),
                ],
                new PostmanRequest(
                    "POST",
                    [],
                    new PostmanRequestBody(
                        "raw",
                        body,
                        new PostmanRequestBodyOptions(
                            new PostmanRequestBodyOptionsRaw("json")
                        )),
                    GetPostmanRequestUrlForAssertBotShouldSendMessage(emulatorHostTemplate)),
                []);
        }

        if (update is { MessageReaction: { } botMessageReaction } && (botMessageReaction.User?.Username == "bot"))
        {
            var emojis = botMessageReaction.NewReaction
                .OfType<ReactionTypeEmoji>()
                .Select(rte => rte.Emoji)
                .ToList();
            if (emojis.Count == 0 || emojis.Count > 1)
            {
                throw new NotSupportedException();
            }
            // var emoji = emojis.Single();
            // var s1 = GetEscapeSequence(emoji[0]);

            var anchor = -87398234;
            var body = JsonSerializer.Serialize(
                new MessageReactionAssert(
                    BotToken: botTokenTemplate,
                    AfterOffset: anchor,
                    ChatId: 1,
                    MessageId: anchor,
                    ReactionType: emojis.Single(),
                    TimeoutMilliseconds: 5000
                ),
                _opts1);
            body = body.Replace(
                anchor.ToString(CultureInfo.InvariantCulture),
                "{{" + recentOffsetVariableName + "}}");

            return new PostmanSingleItem(
                $"Bot should set reaction",
                [
                    new PostmanSingleEvent(
                        "test",
                        new PostmanScript(
                            [
                                "pm.test(\"Status code is 200\", function () {\r",
                                "    pm.response.to.have.status(200);\r",
                                "});\r",
                                "\r",
                                "pm.test(\"Result is 'Success'\", function () {\r",
                                "    var jsonData = pm.response.json();\r",
                                "    pm.expect(jsonData.Result).to.eql(\"Success\");\r",
                                "});\r",
                                "",
                            ],
                            "text/javascript",
                            new object()
                        )),
                ],
                new PostmanRequest(
                    "POST",
                    [],
                    new PostmanRequestBody(
                        "raw",
                        body,
                        new PostmanRequestBodyOptions(
                            new PostmanRequestBodyOptionsRaw("json")
                        )),
                    GetPostmanRequestUrlForAssertBotShouldSetMessageReaction(emulatorHostTemplate)),
                []);
        }

        throw new NotSupportedException();
    }

    private static string GetEscapeSequence(char c)
    {
        return "\\u" + ((int)c).ToString("X4");
    }
}

internal sealed record class PostmanCollection(
    PostmanInfo Info,
    PostmanSingleItem[] Item,
    PostmanSingleVariable[] Variable);

internal sealed record class PostmanInfo(
    string Name,
    Uri Schema);

internal sealed record class PostmanSingleItem(
    string Name,
    PostmanSingleEvent[] Event,
    PostmanRequest Request,
    object[] Response);

internal sealed record class PostmanSingleEvent(
    string Listen,
    PostmanScript Script);

internal sealed record class PostmanScript(
    string[] Exec,
    string Type,
    object Packages);

internal sealed record class PostmanRequest(
    string Method,
    object[] Header,
    PostmanRequestBody Body,
    PostmanRequestUrl Url);

internal sealed record class PostmanRequestBody(
    string Mode,
    string Raw,
    PostmanRequestBodyOptions Options);

internal sealed record class PostmanRequestUrl(
    string Raw,
    string[] Host,
    string[] Path);

internal sealed record class PostmanSingleVariable(
    string Key,
    string Value,
    string Type);

internal sealed record class PostmanRequestBodyOptions(
    PostmanRequestBodyOptionsRaw Raw);

internal sealed record class PostmanRequestBodyOptionsRaw(
    string Language);
