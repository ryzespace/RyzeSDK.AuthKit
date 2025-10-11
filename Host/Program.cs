using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Domain.Repositories;
using Host.Configuration;
using Infrastructure.Keycloak.Providers;
using Infrastructure.Keycloak.Repositories;

var builder = WebApplication.CreateBuilder(args);

// === Core Config ===
builder.Services.ConfigureApp(builder.Configuration, builder.Environment)
    .AddGrpcServices()
    .AddRestfulServices()
    .AddKeycloakServices()
    .AddScoped<IKeycloakTokenProvider, KeycloakTokenProvider>()
    .AddScoped<IKeycloakUserRepository, KeycloakUserRepository>()
    .AddScoped<IKeycloakService, KeycloakService>();

builder.Host.ConfigureWolverine(builder.Configuration);
builder.WebHost.ConfigureKestrelServer();

// === Pipeline ===
var app = builder.Build();

app.ConfigureMiddleware() 
    .MapAppEndpoints()
    .MapGrpcEndpoints();

app.Run();
