using Api.Middleware;
using Application;
using Infrastructure;
using Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Enregistre le DbContext LocaCar (voir Infrastructure/DependencyInjection.cs).
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// ProblemDetails : format d'erreur normalise (RFC 9457), commun aux erreurs de
// validation produites par [ApiController] et a celles du handler metier.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionsMetierHandler>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Migrations appliquees et jeu de demonstration insere si la base est vide
    // (voir Infrastructure/Persistence/DonneesDemonstration.cs).
    await app.Services.InitialiserBaseDeDemonstrationAsync();
}

// Doit preceder tout le reste du pipeline pour intercepter les exceptions
// levees par les controleurs.
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
