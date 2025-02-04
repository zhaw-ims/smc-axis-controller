using SMCAxisController.Hardware;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using MudBlazor;
using MudBlazor.Services;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using SMCAxisController.DataModel;
using SMCAxisController.Pages;

var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;

    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console(theme: AnsiConsoleTheme.Code)
        .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
        .WriteTo.Seq("http://localhost:5341"));

builder.Services.AddScoped<ISmcEthernetIpConnectorFactory, SmcEthernetIpConnectorFactory>();
builder.Services.AddScoped<IIndexVm, IndexVm>();

// Bind the "Controllers" section from appsettings.json to a list
var controllerConfigs = builder.Configuration.GetSection("Controllers").Get<List<ControllerProperties>>() 
                        ?? new List<ControllerProperties>();

// If you need to inject the list elsewhere, you can also register it:
builder.Services.Configure<List<ControllerProperties>>(builder.Configuration.GetSection("Controllers"));


// For each controller, register a new connector and its background service
foreach (var controller in controllerConfigs)
{
    builder.Services.AddScoped<ISmcEthernetIpConnector>(sp =>
        new SmcEthernetIpConnector(sp.GetRequiredService<ILogger<SmcEthernetIpConnector>>())
        {
            ControllerProperties = controller
        });
}
builder.Services.AddHostedService<SmcRegisterPollingBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();