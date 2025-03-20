using Bank.Api.Logic;

namespace Bank.Api;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register services
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton<IEndpointHandler, EndpointHandler>((x) => new EndpointHandler());

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        // Map endpoints
        app.MapGet("/", async (IEndpointHandler handler) => await handler.ListAccountsAsync());
        app.MapPost("/account", async (IEndpointHandler handler) => await handler.CreateAccountAsync());
        app.MapDelete("/account/{accountId}", async (IEndpointHandler handler, int accountId) => await handler.DeleteAccountAsync(accountId));

        app.Run();
    }
}
