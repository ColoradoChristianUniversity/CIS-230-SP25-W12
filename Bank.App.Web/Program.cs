using Bank.App.Shared;  // This contains IApiClient and BankApiClient
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);

// Register the API client using the HttpClient factory.
// This provides an HttpClient with the BaseAddress of "http://localhost:1234"
builder.Services.AddHttpClient<IApiClient, BankApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:1234");
});

// Add Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// If you use custom extension methods, make sure they are available. Otherwise, map Razor Pages normally:
app.MapRazorPages();

app.Run();
