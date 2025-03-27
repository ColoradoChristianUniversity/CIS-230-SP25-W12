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
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new ITransactionConverter());
        });

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        // Map endpoints
        app.MapGet("/", async (IEndpointHandler handler) => await handler.ListAccountsAsync());
        app.MapPost("/account", async (IEndpointHandler handler) => await handler.CreateAccountAsync());
        app.MapDelete("/account/{accountId}", async (IEndpointHandler handler, int accountId) => await handler.DeleteAccountAsync(accountId));

        // LS Implementations
        app.MapGet("/account/{accountId}", 
                    async (IEndpointHandler handler, int accountId) => 
                        await handler.GetAccountAsync(accountId));
        app.MapPost("/withdraw/{accountId}/{amount}", 
                    async (IEndpointHandler handler, int accountId, double amount) => 
                        await handler.WithdrawAsync(accountId, amount));
        app.MapPost("/deposit/{accountId}/{amount}", 
                    async (IEndpointHandler handler, int accountId, double amount) => 
                        await handler.DepositAsync(accountId, amount));
        app.MapGet("/transactions/{accountId}", 
                    async (IEndpointHandler handler, int accountId) => 
                        await handler.GetAccountAsync(accountId));
        app.MapPost("/transactions/{accountId}/{type}/{amount}", 
                    async (IEndpointHandler handler, int accountId, string type, double amount) => 
                        await handler.AddTransactionAsync(accountId, type, amount));

        app.Run();
    }
}
