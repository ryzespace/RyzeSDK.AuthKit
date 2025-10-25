using Host.Configuration;

var builder = WebApplication.CreateBuilder(args);

// === Core Config ===
builder.Services.ConfigureApp(builder.Configuration, builder.Environment)
    .AddGrpcServices()
    .AddRestfulServices()
    .AddKeycloakServices()
    .AddAuthKitDeveloperToken();

builder.Host.ConfigureWolverine(builder.Configuration);
builder.WebHost.ConfigureKestrelServer();

// === Pipeline ===
var app = builder.Build();

app.ConfigureMiddleware() 
    .MapAppEndpoints()
    .MapGrpcEndpoints();

app.Run();
