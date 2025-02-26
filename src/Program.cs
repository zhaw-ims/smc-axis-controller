using SMCAxisController.Hardware;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.OpenApi.Models;
using MudBlazor;
using MudBlazor.Services;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using SMCAxisController.DataModel;
using SMCAxisController.Pages;
using SMCAxisController.StateMachine;

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

builder.Configuration.AddJsonFile(RobotSequences.FILENAME);
builder.Services.Configure<RobotSequences>(builder.Configuration.GetSection(RobotSequences.KEY));

var robotSequences = new RobotSequences();
builder.Configuration.GetSection(RobotSequences.KEY).Bind(robotSequences);
builder.Services.AddSingleton(robotSequences);

// var robotSequences = builder.Configuration
//     .GetSection(RobotSequences.KEY)
//     .Get<RobotSequences>();

List<MoveSequence> generatedSequences = new List<MoveSequence>();
if(robotSequences != null)
    generatedSequences = GridSequencesGenerator.GenerateGridSequences(robotSequences.SamplesGrid);

foreach (var sequence in generatedSequences)
{
    robotSequences.DefinedSequences.Add(sequence.Name, sequence);
    var gridFlow = GridFlowsGenerator.GenerateGridFlow(sequence, robotSequences.GeneratedFlowPattern);
    robotSequences.SequenceFlows.Add(gridFlow.Name, gridFlow);
}

builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console(theme: AnsiConsoleTheme.Code)
        .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
        .WriteTo.Seq("http://localhost:5341"));

builder.Services.AddScoped<IIndexVm, IndexVm>();
builder.Services.AddSingleton<IStateMachine,StateMachine>();

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
builder.Services.AddSingleton<IConnectorsRepository, ConnectorsRepository>();
builder.Services.AddHostedService<SmcRegisterPollingBackgroundService>();

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var swaggerSettings = builder.Configuration.GetSection("Swagger");

if (swaggerSettings.GetValue<bool>("Enabled"))
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc(swaggerSettings["Version"], new OpenApiInfo
        {
            Title = swaggerSettings["Title"],
            Version = swaggerSettings["Version"],
            Description = swaggerSettings["Description"]
        });
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() && swaggerSettings.GetValue<bool>("Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint($"/swagger/{swaggerSettings["Version"]}/swagger.json", swaggerSettings["Title"]);
        options.RoutePrefix = swaggerSettings["RoutePrefix"];
    });
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.MapControllers();  // Ensure API controllers are registered properly

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();