using brewbase.server.Authentication;
using brewbase.server.Models;
using brewbase.server.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using brewbase.server.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "ApiPassthrough";
        options.DefaultForbidScheme = "ApiPassthrough";
        options.DefaultChallengeScheme = "ApiPassthrough";
    })
    .AddScheme<AuthenticationSchemeOptions, ApiPassthroughAuthHandler>("ApiPassthrough", _ => { });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

builder.Services.AddDbContext<BrewDbContext>(opt =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    opt.UseNpgsql(connectionString);
});

builder.Services.AddScoped<ICoffeeReadService, CoffeeReadService>();
builder.Services.AddScoped<IRecipeReadService, RecipeReadService>();
builder.Services.AddScoped<IBrewingMethodReadService, BrewingMethodReadService>();
builder.Services.AddScoped<ITastingSessionWriteService, TastingSessionWriteService>();
builder.Services.AddScoped<ITastingSessionReadService, TastingSessionReadService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program
{
}