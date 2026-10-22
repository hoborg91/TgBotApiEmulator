using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using TgBotApiEmulator.Application;

namespace TgBotApiEmulator.Tests.Serialization;

public class SerializationTests
{
    private JsonSerializerOptions _jsonSerializerOptions = new();

    [SetUp]
    public void Setup()
    {
        var options = new JsonOptions();
        new JsonSerializerConfig().Configure(options);
        _jsonSerializerOptions = options.JsonSerializerOptions;
    }

    [TestCaseSource(nameof(GetTestCases))]
    public void SerializationRoundtrip(TestCase testCase)
    {
        var serialized = JsonSerializer.Serialize(testCase.Update, _jsonSerializerOptions);

        var deserialized = JsonSerializer.Deserialize<Update>(serialized, _jsonSerializerOptions);

        if (AreEqualByTypeAndStructure(testCase.Update, deserialized, out var firstDiff))
        {
            return;
        }

        Assert.Fail(firstDiff);
    }

    [Test]
    public void DeserizalizeUpdate_DoesNotFail()
    {
        var serialized = File.ReadAllText("Serialization/UpdateWithMessageReaction.json");

        _ = JsonSerializer.Deserialize<Update>(serialized, _jsonSerializerOptions);
    }

    [Test]
    public void DeserizalizeApiResponseUpdate_DoesNotFail()
    {
        var serialized = File.ReadAllText("Serialization/ApiResponseWithUpdateWithMessageReaction.json");

        _ = JsonSerializer.Deserialize<ApiResponse<Update[]>>(serialized, _jsonSerializerOptions);
    }

    private static IEnumerable<TestCase> GetTestCases()
    {
        yield return new TestCase("Message", new Update
        {
            Id = 12,
            Message = new Message
            {
                Chat = new Chat
                {
                    Id = 1004,
                    Type = Telegram.Bot.Types.Enums.ChatType.Group,
                    Username = "Update.Message.Chat.Username",
                },
                From = new User
                {
                    Id = 203,
                    Username = "Update.Message.From.Username",
                },
                Id = 103,
                Text = "Update.Message.Text",
            },
        });

        yield return new TestCase("MessageReaction", new Update
        {
            Id = 12,
            MessageReaction = new MessageReactionUpdated
            {
                Chat = new Chat
                {
                    Id = 1004,
                    Type = Telegram.Bot.Types.Enums.ChatType.Group,
                    Username = "Update.Message.Chat.Username",
                },
                User = new User
                {
                    Id = 203,
                    Username = "Update.Message.From.Username",
                },
                MessageId = 12,
                NewReaction = ["👍", "👎"],
            }
        });
    }

    public sealed record class TestCase(
        string Caption,
        Update Update)
    {
        public override string ToString() => Caption;
    }

    private static bool AreEqualByTypeAndStructure(
        object? a,
        object? b,
        [NotNullWhen(false)] out string? firstDiff,
        string? aPath = null,
        string? bPath = null,
        IDictionary<object, ISet<object>>? alreadyChecked = null)
    {
        aPath ??= nameof(a);
        bPath ??= nameof(b);

        firstDiff = null;

        if (a == null && b == null)
        {
            return true;
        }

        if (a == null || b == null)
        {
            firstDiff = a == null
                ? $"{aPath} is null, but {nameof(b)} is not null"
                : $"{bPath} is null, but {aPath} is not null";
            return false;
        }

        var aType = a.GetType();
        var bType = b.GetType();

        if (aType != bType)
        {
            firstDiff = $"Type of {aPath} is \"{aType.FullName}\", but " +
                $"type of {bPath} is \"{bType.FullName}\"";
            return false;
        }

        if (IsSimpleType(aType))
        {
            if (a.Equals(b))
            {
                return true;
            }

            firstDiff = $"{aPath} == {a}, but {bPath} == {b}.";
            return false;
        }

        alreadyChecked ??= new Dictionary<object, ISet<object>>();
        if (!alreadyChecked.TryGetValue(a, out var checkedObjectList))
        {
            checkedObjectList = new HashSet<object>();
            alreadyChecked.Add(a, checkedObjectList);
        }
        if (checkedObjectList.Contains(b))
        {
            return true;
        }
        checkedObjectList.Add(b);

        const BindingFlags BindingFlagsPublic = BindingFlags.Instance | BindingFlags.Public;
        const BindingFlags BindingFlagsNonPublic = BindingFlags.Instance | BindingFlags.NonPublic;

        var propertiesAndFields = aType.GetProperties(BindingFlagsPublic).Select(ToPropertyOrField)
            .Concat(aType.GetProperties(BindingFlagsNonPublic).Select(ToPropertyOrField))
            .Concat(aType.GetFields(BindingFlagsPublic).Select(ToPropertyOrField))
            .Concat(aType.GetFields(BindingFlagsNonPublic).Select(ToPropertyOrField))
            .ToList();

        foreach (var member in propertiesAndFields)
        {
            var aValue = member.GetValue(a);
            var bValue = member.GetValue(b);

            if (AreEqualByTypeAndStructure(
                aValue,
                bValue,
                out var innerDiff,
                aPath: aPath + "." + member.Name,
                bPath: bPath + "." + member.Name,
                alreadyChecked: alreadyChecked))
            {
                continue;
            }

            firstDiff = innerDiff;
            return false;
        }

        return true;
    }

    // https://gist.github.com/jonathanconway/3330614
    /// <summary>
    /// Determine whether a type is simple (String, Decimal, DateTime, etc)
    /// or complex (i.e. custom class with public properties and methods).
    /// </summary>
    /// <see cref="http://stackoverflow.com/questions/2442534/how-to-test-if-type-is-primitive"/>
    private static bool IsSimpleType(Type type) =>
        type.IsValueType ||
        type.IsPrimitive ||
        new Type[]
        {
            typeof(string),
            typeof(decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(Guid)
        }.Contains(type) ||
        Convert.GetTypeCode(type) != TypeCode.Object;

    private static PropertyOrField ToPropertyOrField(PropertyInfo pi) => new(
        pi.Name,
        pi.GetValue);

    private static PropertyOrField ToPropertyOrField(FieldInfo pi) => new(
        pi.Name,
        pi.GetValue);

    private sealed record class PropertyOrField(
        string Name,
        Func<object?, object?> GetValue);
}
