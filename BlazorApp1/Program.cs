using BlazorApp1;
using BlazorApp1.Components;
using BlazorApp1.Models;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using CregitInfoWS;
using Microsoft.EntityFrameworkCore;
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



builder.Services.AddScoped<CreditOrgInfoSoap>(_ =>
{
    var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport) // для HTTPS
    {
        MaxReceivedMessageSize = 1024 * 1024 * 10, // 10 МБ, можно больше
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

builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Google", options.ProviderOptions);

    options.ProviderOptions.Authority = "https://accounts.google.com";
    options.ProviderOptions.ClientId = "{41036326306-7iihk5gbbit6tb6d3agrnoddpggiiqrg.apps.googleusercontent.com}";
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.ProviderOptions.DefaultScopes.Add("email");

    // redirect uri (по умолчанию /authentication/login-callback)
});

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
