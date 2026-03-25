using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using SmartTourApp.Application.Services;
using SmartTourApp.Domain.Interfaces;
using SmartTourApp.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices(config =>
{
    config.PopoverOptions.ThrowOnDuplicateProvider = false;
});

// ── Database (same as API, shared DbContext) ──
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.UseNetTopologySuite()
    ));
builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

// ── Redis Cache ──
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.InstanceName = "SmartTour_";
});

// ── Services injected directly (no HTTP calls!) ──
builder.Services.AddScoped<IPoiService, PoiService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISyncService, SyncService>();
builder.Services.AddScoped<IAiGuideService, AiGuideService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddScoped<IServicePackageService, ServicePackageService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IRevenueService, RevenueService>();
builder.Services.AddScoped<IPoiRequestService, PoiRequestService>();
builder.Services.AddHttpClient("OpenAI");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<SmartTourApp.WebAdmin.Components.App>()
    .AddInteractiveServerRenderMode();

// Auto-migrate (best-effort to allow local runs without DB access)
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Database initialization failed. Continuing without DB.");
}

app.Run();
