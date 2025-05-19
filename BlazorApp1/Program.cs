using BlazorApp1;
using BlazorApp1.Components;
using BlazorApp1.Models;
using CregitInfoWS;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.ServiceModel;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<CreditOrgInfoClient>();

// получение строки подключения
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

// Добавляем поддержку PostgreSQL через Entity Framework Core
builder.Services.AddDbContextFactory<BanksContext>(options =>
	options.UseNpgsql(connectionString));

builder.Services.AddBlazorBootstrap();

// Регистрируем CreditOrgInfoSoap вручную:
builder.Services.AddScoped<CreditOrgInfoSoap>(_ =>
{
	var binding = new BasicHttpBinding();
	var endpoint = new EndpointAddress("http://www.cbr.ru/CreditInfoWebServ/CreditOrgInfo.asmx");
	var factory = new ChannelFactory<CreditOrgInfoSoap>(binding, endpoint);
	return factory.CreateChannel();
});

// И регистрируем сам клиент
builder.Services.AddScoped<CreditOrgInfoClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
