using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using MudBlazor.Services;
using WorkLog.Web;
using WorkLog.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// API 基底位址
var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl") ?? "http://localhost:5001";

// LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Auth 服務 - 使用不經過 handler 的獨立 HttpClient（認證 API 不需要 JWT）
builder.Services.AddHttpClient("AuthApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddScoped<AuthService>(sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("AuthApi");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    return new AuthService(httpClient, localStorage);
});

builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthService>());
builder.Services.AddAuthorizationCore();

// 一般 API 請求的 HttpClient - 含 JWT Token 攔截
builder.Services.AddScoped<AuthTokenHandler>();
builder.Services.AddHttpClient("WorkLogApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("WorkLogApi"));

// 註冊服務
builder.Services.AddScoped<TodoService>();
builder.Services.AddScoped<DataExportService>();
builder.Services.AddScoped<ThemeService>();

// 註冊 MudBlazor
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 4000;
    config.SnackbarConfiguration.HideTransitionDuration = 300;
    config.SnackbarConfiguration.ShowTransitionDuration = 300;
});

await builder.Build().RunAsync();
