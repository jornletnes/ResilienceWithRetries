using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

const string serviceName = "EndepunktSesam";
builder.Logging.AddOpenTelemetry(options => options
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddService(serviceName))
    .AddConsoleExporter());
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource(nameof(Program))
        .AddSource("Controller")
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName)))
    .WithMetrics(metrics => metrics
        .AddRuntimeInstrumentation()
        .AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel", "System.Net.Http"))
    .UseOtlpExporter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapGet("/", async () =>
{
    using var activitySource = new ActivitySource(serviceName);
    var activity = activitySource.StartActivity(nameof(Program));
    activity?.SetTag("ControllerKey", "ControllerValue");

    Thread.Sleep(new Random().Next(1000, 3000));

    var client = new HttpClient();
    var response = await client.GetAsync("http://localhost:5092/dert");
    
    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem(response.StatusCode.ToString(), statusCode: (int)response.StatusCode);
    }
    
    var content = await response.Content.ReadAsStringAsync();
    return Results.Ok(content);
})
.WithOpenApi();

app.Run();