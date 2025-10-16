using Host.Configuration;
using Host.Middleware;

var builder = WebApplication.CreateBuilder(args);

// === Core Config ===
builder.Services.ConfigureApp(builder.Configuration, builder.Environment)
    .AddGrpcServices()
    .AddRestfulServices()
    .AddKeycloakServices();

builder.Host.ConfigureWolverine(builder.Configuration);
builder.WebHost.ConfigureKestrelServer();

// === Pipeline ===
var app = builder.Build();

app.ConfigureMiddleware() 
    .MapAppEndpoints()
    .MapGrpcEndpoints();
app.UseMiddleware<ValidationExceptionMiddleware>();
app.Run();
