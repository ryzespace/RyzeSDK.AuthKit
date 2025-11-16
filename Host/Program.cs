using System.Security.Cryptography;
using Application.Interfaces;
using Application.Services;
using Domain.Repositories;
using Host.Configuration;
using Infrastructure.Repositories;

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

// === Pipeline ===
var app = builder.Build();

app.ConfigureMiddleware() 
    .MapAppEndpoints()
    .MapGrpcEndpoints();

app.Run();