using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

const string serviceName = "DetUltimateEndepunkt";
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var returnSuccess = true;

app.MapGet("/dert", () =>
{
    using var activitySource = new ActivitySource("Gabbe er en kosegutt");
    var activity = activitySource.StartActivity(nameof(Program));

    returnSuccess = !returnSuccess;
    return returnSuccess ? Results.Ok("Dette gikk jo vidunderlig") : Results.Problem();
})
.WithOpenApi();

app.Run();
