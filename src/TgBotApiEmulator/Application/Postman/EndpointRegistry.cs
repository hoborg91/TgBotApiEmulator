namespace TgBotApiEmulator.Application.Postman;

public interface IEndpointRegistry
{
    RouteEndpoint GetAssertBotShouldSendMessage();
    RouteEndpoint GetAssertBotShouldSetMessageReaction();
    RouteEndpoint GetEmulatorMessage();
}

internal sealed class EndpointRegistry(
    IEnumerable<EndpointDataSource> endpointDataSources) : IEndpointRegistry
{
    private readonly Lazy<RouteEndpoint> _emulatorMessage = new(() =>
        endpointDataSources.GetByControllerAndAction("Emulator", "Message"));

    private readonly Lazy<RouteEndpoint> _assertBotShouldSendMessage = new(() =>
        endpointDataSources.GetByControllerAndAction("Assert", "WaitForMessageFromBot"));

    private readonly Lazy<RouteEndpoint> _assertBotShouldSetMessageReaction = new(() =>
        endpointDataSources.GetByControllerAndAction("Assert", "WaitForMessageReactionFromBot"));

    public RouteEndpoint GetAssertBotShouldSendMessage() => _assertBotShouldSendMessage.Value;

    public RouteEndpoint GetAssertBotShouldSetMessageReaction() => _assertBotShouldSetMessageReaction.Value;

    public RouteEndpoint GetEmulatorMessage() => _emulatorMessage.Value;
}

internal static class Ext
{
    public static RouteEndpoint GetByControllerAndAction(
        this IEnumerable<EndpointDataSource> endpointDataSources,
        string controllerName,
        string actionName)
    {
        var endpoints = endpointDataSources.SelectMany(es => es.Endpoints)
            .OfType<RouteEndpoint>()
            .ToList();

        return endpoints.Single(e =>
        {
            var descriptor = e.Metadata.OfType<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>().Single();
            return descriptor.ControllerName == controllerName && descriptor.ActionName == actionName;
        });
    }
}
