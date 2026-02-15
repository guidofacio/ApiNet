using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://tu-frontend.onrender.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Render / PORT
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// IMPORTANTE: aplicar CORS (antes de mapear endpoints)
app.UseCors("AllowFrontend");

// Swagger (si querés verlo también en Render, sacá el if)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// En Render suele ser mejor NO redireccionar a HTTPS dentro del contenedor
// (Render termina TLS afuera). Si lo dejás y te funciona, ok.
// Si te genera problemas, dejalo comentado.
//// app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return Results.Ok(forecast);
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/medicos", async () =>
{
    var connStr = Environment.GetEnvironmentVariable("MYSQL_CONN");

    var medicos = new List<object>();

    using var conn = new MySqlConnection(connStr);
    await conn.OpenAsync();

    var cmd = new MySqlCommand("SELECT * FROM medicos", conn);
    using var reader = await cmd.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        medicos.Add(new
        {
            id = reader["id"],
            nombre = reader["nombre"],
            apellido = reader["apellido"],
            especialidad = reader["especialidad"],
            matricula = reader["matricula"],
            telefono = reader["telefono"],
            email = reader["email"]
        });
    }

    return Results.Ok(medicos);
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}