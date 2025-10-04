using Host.Services;
using Infrastructure.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    // REST + Swagger (HTTPS)
    options.ListenAnyIP(5000, listenOptions =>
    {
        listenOptions.UseHttps("/root/certs/devcert.pfx", "YourPassword");
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });

    // gRPC only (HTTPS)
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.UseHttps("/root/certs/devcert.pfx", "YourPassword");
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

// Grpc
builder.Services.AddGrpc();

// === REST API + Swagger ===
builder.Services.AddControllers()
    .AddApplicationPart(typeof(AuthController).Assembly);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();       // REST
app.MapGet("/", () => Results.Json(new
{
    name = "Auth Microservice",
    description = "Core service for authentication, authorization and identity management.",
    endpoints = new
    {
        rest = new
        {
            baseUrl = "https://localhost:5000/api",
            swagger = "https://localhost:5000/swagger"
        },
        grpc = new
        {
            baseUrl = "https://localhost:5001",
            note = "Use gRPC client to interact with this service."
        }
    },
    version = "1.0.0",
    environment = app.Environment.EnvironmentName
}));


app.MapGrpcService<GreeterService>(); // gRPC server
app.Run();