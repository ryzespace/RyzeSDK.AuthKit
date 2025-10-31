using Host.Configuration;

var builder = WebApplication.CreateBuilder(args);

// === Core Config ===
builder.Services.ConfigureApp(builder.Configuration)
    .AddGrpcServices()
    .AddRestfulServices()
    .AddKeycloakServices()
    .AddAuthKitDeveloperToken();

builder.ConfigureWolverine();
builder.Services.ConfigureMarten(builder.Configuration);
builder.WebHost.ConfigureKestrelServer();

// === Pipeline ===
var app = builder.Build();

app.ConfigureMiddleware() 
    .MapAppEndpoints()
    .MapGrpcEndpoints();

app.Run();
