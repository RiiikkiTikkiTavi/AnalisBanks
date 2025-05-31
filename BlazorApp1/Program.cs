using BlazorApp1;
using BlazorApp1.Components;
using BlazorApp1.Models;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Charts;
using Blazorise.Icons.FontAwesome;
using CregitInfoWS;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.ServiceModel;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<CreditOrgInfoClient>();

// ��������� ������ �����������
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

// ��������� ��������� PostgreSQL ����� Entity Framework Core
builder.Services.AddDbContextFactory<BanksContext>(options =>
	options.UseNpgsql(connectionString));

builder.Services.AddBlazorBootstrap();



builder.Services.AddScoped<CreditOrgInfoSoap>(_ =>
{
    var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport) // ��� HTTPS
    {
        MaxReceivedMessageSize = 1024 * 1024 * 10, // 10 ��, ����� ������
        ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas
        {
            MaxDepth = 64,
            MaxStringContentLength = 1024 * 1024,
            MaxArrayLength = 1024 * 1024,
            MaxBytesPerRead = 4096,
            MaxNameTableCharCount = 1024 * 1024
        }
    };

    var endpoint = new EndpointAddress("https://www.cbr.ru/CreditInfoWebServ/CreditOrgInfo.asmx"); // HTTPS!

    var factory = new ChannelFactory<CreditOrgInfoSoap>(binding, endpoint);
    return factory.CreateChannel();
});

builder.Services.AddScoped<CreditOrgInfoClient>();

builder.Services.AddScoped<ExcelService>();

builder.Services
	.AddBlazorise(options =>
	{
		options.Immediate = true;
	})
	.AddBootstrapProviders()
	.AddFontAwesomeIcons();

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
