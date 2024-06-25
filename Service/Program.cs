using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;

var builder = WebApplication.CreateBuilder(args);

const string serviceName = "Service";
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

app.MapGet("/slapp", async () =>
{
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


app.MapGet("/ivrig", async () =>
    {
        var retryPolicy = Policy
            // .Handle<HttpRequestException>()
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        
        var client = new HttpClient();
        HttpResponseMessage response = null;

        await retryPolicy.ExecuteAsync(async () =>
        {
            response = await client.GetAsync("http://localhost:5092/dert");
            return response;
        });
    
        if (!response.IsSuccessStatusCode)
        {
            return Results.Problem(response.StatusCode.ToString(), statusCode: (int)response.StatusCode);
        }
    
        var content = await response.Content.ReadAsStringAsync();
        return Results.Ok(content);
    })
    .WithOpenApi();


app.MapGet("/tracete", async () =>
    {
        var client = new HttpClient();
        var response = await client.GetAsync("http://localhost:5219/");
    
        if (!response.IsSuccessStatusCode)
        {
            return Results.Problem(response.StatusCode.ToString(), statusCode: (int)response.StatusCode);
        }
    
        var content = await response.Content.ReadAsStringAsync();
        return Results.Ok(content);
    })
    .WithOpenApi();

app.Run();