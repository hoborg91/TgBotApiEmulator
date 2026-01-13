using System.Globalization;
using System.Text.Json;
using Telegram.Bot.Types;
using TgBotApiEmulator.Controllers;

namespace TgBotApiEmulator.Application.Postman;

public interface IPostmanFacade
{
    byte[] MakeUtf8Bytes(
        string botTokenString,
        IReadOnlyCollection<Update> updates);
    byte[] MakeUtf8BytesDebug();
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

    public byte[] MakeUtf8BytesDebug()
    {
        var emulatorHostTemplate = "{{emulatorUrl}}";

        var myCollection = new PostmanCollection(
            new PostmanInfo(
                "My.Test",
                new Uri("https://schema.getpostman.com/json/collection/v2.1.0/collection.json")),
            [
                new PostmanSingleItem(
                    "/start",
                    [
                        new PostmanSingleEvent(
                            "test",
                            new PostmanScript(
                                [
                                    "pm.test(\"Status code is 200\", function () {\r",
                                    "    pm.response.to.have.status(200);\r",
                                    "});",
                                ],
                                "text/javascript",
                                new object())),
                    ],
                    new PostmanRequest(
                        "POST",
                        [],
                        new PostmanRequestBody(
                            "raw",
                            JsonSerializer.Serialize(new { Text = "/start" }, _opts1),
                            new PostmanRequestBodyOptions(
                                new PostmanRequestBodyOptionsRaw("json")
                            )),
                        GetPostmanRequestUrlForEmulateMessage(emulatorHostTemplate)),
                    []),
                new PostmanSingleItem(
                    "Expect \"Participant registered\"",
                    [
                        new PostmanSingleEvent(
                            "test",
                            new PostmanScript(
                                [
                                    "pm.test(\"Status code is 200\", function () {\r",
                                    "    pm.response.to.have.status(200);\r",
                                    "});\r",
                                    "\r",
                                    "pm.test(\"Your test name\", function () {\r",
                                    "    var jsonData = pm.response.json();\r",
                                    "    pm.expect(jsonData.result).to.eql(\"Pass\");\r",
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
                            JsonSerializer.Serialize(
                                new {
                                    MessageContainsText = "Participant registered",
                                    TimeoutMilliseconds = 1500,
                                },
                                _opts1),
                            new PostmanRequestBodyOptions(
                                new PostmanRequestBodyOptionsRaw("json")
                            )),
                        GetPostmanRequestUrlForAssertBotShouldSendMessage(emulatorHostTemplate)),
                    []),
            ],
            [
                new PostmanSingleVariable(
                    "emulatorUrl",
                    "",
                    "default"
                ),
            ]
        );
        var bytes = JsonSerializer.SerializeToUtf8Bytes(
            myCollection,
            _optsMain);

        return bytes;
    }

    public byte[] MakeUtf8Bytes(
        string botTokenString,
        IReadOnlyCollection<Update> updates)
    {
        var emulatorHostVariableName = "emulatorUrl";
        var recentOffsetVariableName = "recentOffset";
        var postmanItems = updates
            .Select(u => ToPostmanItem(botTokenString, u, "{{" + emulatorHostVariableName + "}}"))
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

    private PostmanSingleItem ToPostmanItem(string botTokenString, Update update, string emulatorHostTemplate)
    {
        if (update is { Message: { } userMessage } && (userMessage.From?.Username?.Contains("User") ?? false))
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
                                "pm.collectionVariables.set(\"recentOffset\", jsonData.offset);\r",
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
                            new MessageRequest(botTokenString, 1, userMessage.Text ?? throw new NotSupportedException()),
                            _opts1),
                        new PostmanRequestBodyOptions(
                            new PostmanRequestBodyOptionsRaw("json")
                        )),
                    GetPostmanRequestUrlForEmulateMessage(emulatorHostTemplate)),
                []);
        }

        if (update is { Message: { } botMessage } && (botMessage.From?.Username == "bot"))
        {
            var anchor = -87398234;
            var body = JsonSerializer.Serialize(
                new MessageAssert(
                    SessionId: 1,
                    AfterOffset: anchor,
                    ChatId: 1,
                    MessageRegexPattern: botMessage.Text ?? ".*",
                    TimeoutMilliseconds: 1500
                ),
                _opts1);
            body = body.Replace(
                anchor.ToString(CultureInfo.InvariantCulture),
                "{{" + "recentOffset" + "}}");

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
                    SessionId: 1,
                    AfterOffset: anchor,
                    ChatId: 1,
                    MessageId: anchor,
                    ReactionType: emojis.Single(),
                    TimeoutMilliseconds: 1500
                ),
                _opts1);
            body = body.Replace(
                anchor.ToString(CultureInfo.InvariantCulture),
                "{{" + "recentOffset" + "}}");

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
