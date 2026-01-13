using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TgBotApiEmulator;
using TgBotApiEmulator.Application;
using TgBotApiEmulator.Application.Postman;
using TgBotApiEmulator.Middleware;
using TgBotApiEmulator.Storage;
using TgBotApiEmulator.Storage.Contracts;

[assembly: InternalsVisibleTo("TgBotApiEmulator.Tests")] // Required by TgBotApiEmulator.Tests.csproj
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] // Required by Moq 4.20.72 NuGet package in TgBotApiEmulator.Tests.csproj

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddCommandLine(args);

builder.Services.Configure<TgBotApiEmulator.Options>(builder.Configuration.GetSection("Settings"));

builder.Services.AddDbContext<TgBotSessionContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("TgBotSessionContextSQLite")));
builder.Services.AddScoped<ITgBotSessionStorage, TgBotSessionStorage>();
builder.Services.AddScoped<ISettingsStorage, SettingsStorage>();

builder.Services.AddHttpClient();

builder.Services.AddScoped<ISessionFacadeRead, RecordingProxyFacade>();
builder.Services.AddScoped<IRecordingProxyFacadeWrite, RecordingProxyFacade>();
builder.Services.AddScoped<IClock, Clock>();
builder.Services.AddScoped<IMyAssert, MyAssert>();
builder.Services.AddScoped<IUpdateReader, UpdateReader>();

builder.Services.AddScoped<IEndpointRegistry, EndpointRegistry>();
builder.Services.AddScoped<IPostmanFacade, PostmanFacade>();

builder.Services.AddScoped<IRecordingMiddlewareUtilities, RecordingMiddlewareUtilities>();
builder.Services.AddScoped<RecordingProxyMiddleware>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        new JsonSerializerConfig().Configure(options);
    });

builder.Services.AddSwaggerGen(options =>
{
    // Without UseAllOfForInheritance either the generated schema does not contain
    // information about hierarchies or this information is corrupted.
    options.UseAllOfForInheritance();
});

var app = builder.Build();

//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

using var startupScope = app.Services.CreateScope();
using var startupCts = new CancellationTokenSource();

var logger = startupScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
var settings = startupScope.ServiceProvider.GetRequiredService<IOptions<TgBotApiEmulator.Options>>().Value;

var dbContext = startupScope.ServiceProvider.GetRequiredService<TgBotSessionContext>();
await dbContext.Database.EnsureCreatedAsync(startupCts.Token);

var serverOperatingMode = settings.ServerOperatingMode;

logger.LogInformation("Server operating mode: {ServerOperatingMode}.", serverOperatingMode);

if (serverOperatingMode == ServerOperatingMode.Emulator)
{
}
else if (serverOperatingMode == ServerOperatingMode.RecordingProxy)
{
    app.UseMiddleware<RecordingProxyMiddleware>();
}
else
{
    throw new InvalidOperationException($"Server operating mode \"{serverOperatingMode}\" is not supported.");
}

app.MapControllers();

app.Run();
