using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wanderverse.Data;
using Wanderverse.ETL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container with proper constructor parameter
builder.Services.AddSingleton<Database>(sp => new Database("game.db")); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Run ETL if requested
if (args.Length > 0 && args[0].ToLower() == "etl")
{
    ETLPrototype.RunETL();
    return; // exit after ETL
}

// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Wanderverse API v1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

// API endpoints
app.MapPost("/api/game/createplayer", (string username, Database db) =>
{
    try
    {
        db.AddPlayer(username);
        return Results.Ok($"Player '{username}' created successfully.");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error creating player: {ex.Message}");
    }
});

app.MapPost("/api/game/moveplayer", (PlayerMoveRequest request, Database db) =>
{
    try
    {
        db.RecordPlayerMove(request.PlayerId, request.LocationId);
        return Results.Ok($"Player {request.PlayerId} moved to location {request.LocationId}.");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error recording move: {ex.Message}");
    }
});

app.MapGet("/api/game/playerpaths/{playerId}", (int playerId, Database db) =>
{
    try
    {
        var paths = db.GetPlayerPaths(playerId);
        return Results.Ok(paths);
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error fetching player paths: {ex.Message}");
    }
});

app.Run();

// Models
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class PlayerMoveRequest
{
    public int PlayerId { get; set; }
    public int LocationId { get; set; }
}