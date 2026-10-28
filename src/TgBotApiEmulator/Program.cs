using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
    options
        .UseLazyLoadingProxies()
        .UseSqlite(builder.Configuration.GetConnectionString("TgBotSessionContextSQLite")));
builder.Services.AddScoped<ITgBotSessionStorage, TgBotSessionStorage>();
builder.Services.AddScoped<ISettingsStorage, SettingsStorage>();
builder.Services.AddScoped<IBotOperatingModeStorage, BotOperatingModeStorage>();

builder.Services.AddHttpClient();

builder.Services.AddScoped<ISessionFacadeRead, RecordingProxyFacade>();
builder.Services.AddScoped<IRecordingProxyFacadeWrite, RecordingProxyFacade>();
builder.Services.AddScoped<IOperatingModeFacade, OperatingModeFacade>();
builder.Services.AddScoped<IClock, Clock>();
builder.Services.AddScoped<IMyAssert, MyAssert>();
builder.Services.AddScoped<IUpdateReader, UpdateReader>();

builder.Services.AddScoped<IEndpointRegistry, EndpointRegistry>();
builder.Services.AddScoped<IPostmanFacade, PostmanFacade>();

builder.Services.AddSingleton<IRetranslatedRequestManager, RetranslatedRequestManager>();
builder.Services.AddScoped<IRecordingMiddlewareUtilities, RecordingMiddlewareUtilities>();
builder.Services.AddScoped<RecordingProxyMiddleware>();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        new JsonSerializerConfig().Configure(options);
    });

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
if (allowedOrigins.Length == 0)
{
    allowedOrigins = ["http://localhost:3000"];
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebGui", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
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

app.UseMiddleware<RecordingProxyMiddleware>();

app.UseCors("AllowWebGui");

app.MapControllers();

app.Run();
