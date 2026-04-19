using brewbase.server.Models;
using Microsoft.EntityFrameworkCore;
using brewbase.server.Services;
using brewbase.server.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BrewDbContext>(opt =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    opt.UseNpgsql(connectionString);
});

builder.Services.AddScoped<ICoffeeReadService, CoffeeReadService>();
builder.Services.AddScoped<IRecipeReadService, RecipeReadService>();
builder.Services.AddScoped<IBrewingMethodReadService, BrewingMethodReadService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program
{
}