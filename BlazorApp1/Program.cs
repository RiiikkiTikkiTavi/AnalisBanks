using BlazorApp1;
using BlazorApp1.Components;
using BlazorApp1.Models;

using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Charts;
using Blazorise.Icons.FontAwesome;

using CregitInfoWS;
using Google.Protobuf.WellKnownTypes;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

using Microsoft.EntityFrameworkCore;
using Npgsql;

using System.ServiceModel;

var builder = WebApplication.CreateBuilder(args);

// --- UI framework and components ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// --- Blazorise (UI библиотека) ---
builder.Services.AddBlazorise(options =>
{
    options.Immediate = true;
})
.AddBootstrapProviders()
.AddFontAwesomeIcons();

builder.Services.AddBlazorBootstrap(); // Есть дублирование с Blazorise.Bootstrap? 

// --- HTTP context ---
builder.Services.AddHttpContextAccessor();

// --- Database ---
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");
builder.Services.AddDbContextFactory<BanksContext>(options =>
    options.UseNpgsql(connectionString));

// --- SOAP клиент для CBR CreditOrgInfo ---
builder.Services.AddScoped<CreditOrgInfoSoap>(_ =>
{
    var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
    {
        MaxReceivedMessageSize = 10 * 1024 * 1024, // 10 МБ
        ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas
        {
            MaxDepth = 64,
            MaxStringContentLength = 1024 * 1024,
            MaxArrayLength = 1024 * 1024,
            MaxBytesPerRead = 4096,
            MaxNameTableCharCount = 1024 * 1024
        }
    };

    var endpoint = new EndpointAddress("https://www.cbr.ru/CreditInfoWebServ/CreditOrgInfo.asmx");

    var factory = new ChannelFactory<CreditOrgInfoSoap>(binding, endpoint);
    return factory.CreateChannel();
});

// --- Собственные сервисы ---
//builder.Services.AddScoped<CreditOrgInfoClient>(); // Повторяется дважды, удали один
builder.Services.AddScoped<ExcelService>();

// --- Безопасность (Authentication/Authorization) ---
builder.Services.AddAntiforgery();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    builder.Configuration.Bind("Authentication:Google", options);
});

builder.Services.AddAuthorization();

var app = builder.Build();


// Обработка ошибок и безопасность
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

// Роуты для входа и выхода
app.MapGet("/login", async context =>
{
    await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = "/Home"
    });
});

app.MapGet("/logout", async context =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/Home");
});


app.Run();