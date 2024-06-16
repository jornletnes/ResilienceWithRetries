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

var returnSuccess = true;

app.MapGet("/dert", () =>
{
    returnSuccess = !returnSuccess;
    return returnSuccess ? Results.Ok("Dette gikk jo vidunderlig") : Results.Problem();
})
.WithOpenApi();

app.Run();
