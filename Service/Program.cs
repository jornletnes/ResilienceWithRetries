using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
    var response = await client.GetAsync("https://localhost:44332/dert");
    
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
            response = await client.GetAsync("https://localhost:44332/dert");
        });
    
        if (!response.IsSuccessStatusCode)
        {
            return Results.Problem(response.StatusCode.ToString(), statusCode: (int)response.StatusCode);
        }
    
        var content = await response.Content.ReadAsStringAsync();
        return Results.Ok(content);
    })
    .WithOpenApi();




app.Run();