using Application.Features.TokenKeyBindings.Interfaces;
using Application.Features.TokenKeyBindings.Services;
using Domain.Features.TokenKeyBindings;
using Host.Configuration;
using Infrastructure.Cli;
using Infrastructure.Persistence.KeyBinding;

var builder = WebApplication.CreateBuilder(args);

// === Core Config ===
builder.Services.AddAuthKitDeveloperToken();

builder.Services.ConfigureApp(builder.Configuration)
    .AddGrpcServices()
    .AddRestfulServices()
    .AddKeycloakServices();

builder.Services.AddSingleton<IKeyBindingRepository, InMemoryKeyBindingRepository>();
builder.Services.AddSingleton<IKeyBindingService, KeyBindingService>();

builder.ConfigureWolverine();
builder.Services.ConfigureMarten(builder.Configuration);
builder.WebHost.ConfigureKestrelServer();

builder.Services.Configure<AuthKitServerOptions>(
    builder.Configuration.GetSection("Server")
);

builder.Services.AddHostedService<ServerHost>();


// === Pipeline ===
var app = builder.Build();

app.ConfigureMiddleware() 
    .MapAppEndpoints()
    .MapGrpcEndpoints();

app.Run();