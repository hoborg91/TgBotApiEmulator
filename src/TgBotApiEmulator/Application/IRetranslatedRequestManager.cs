namespace TgBotApiEmulator.Application;

public interface IRequestRegistration : IDisposable
{
    CancellationToken Token { get; }
}

public interface IRetranslatedRequestManager
{
    IRequestRegistration RegisterRequest(string botTokenHash);
    void AbortPendingRequests(string botTokenHash);
}
