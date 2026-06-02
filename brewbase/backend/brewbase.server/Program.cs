//using brewbase.server.Authentication;
//using Microsoft.AspNetCore.Authentication;
using brewbase.server.Models;
using brewbase.server.Services;
using brewbase.server.Services.Validation;
using brewbase.server.Configuration;
using brewbase.server.Authentication;
using Microsoft.EntityFrameworkCore;
using brewbase.server.Services.Interfaces;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var swaggerEnabled = builder.Configuration.GetValue(
    "Swagger:Enabled",
    builder.Environment.IsDevelopment());

// Add services to the container.
/*
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "ApiPassthrough";
        options.DefaultForbidScheme = "ApiPassthrough";
        options.DefaultChallengeScheme = "ApiPassthrough";
    })
    .AddScheme<AuthenticationSchemeOptions, ApiPassthroughAuthHandler>("ApiPassthrough", _ => { });
    */

builder.Services.AddControllers();

//JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keyString = builder.Configuration["Jwt:Key"];

        if (string.IsNullOrEmpty(keyString))
            throw new InvalidOperationException("JWT Key is not configured");

        var key = Encoding.UTF8.GetBytes(keyString);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier,
            
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is ClaimsIdentity identity)
                {
                    UserClaims.Normalize(identity);
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        var configuredOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        var allowedOrigins = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "http://localhost:5173",
            "http://127.0.0.1:5173"
        };

        foreach (var origin in configuredOrigins)
        {
            if (!string.IsNullOrWhiteSpace(origin))
            {
                allowedOrigins.Add(origin.Trim());
            }
        }

        policy
            .WithOrigins(allowedOrigins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Wpisz: Bearer {token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

var configureBrewDbContext = (DbContextOptionsBuilder opt) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    opt.UseNpgsql(connectionString);
};

// Options as Singleton so IDbContextFactory (singleton) can coexist with scoped DbContext.
builder.Services.AddDbContext<BrewDbContext>(
    configureBrewDbContext,
    contextLifetime: ServiceLifetime.Scoped,
    optionsLifetime: ServiceLifetime.Singleton);

builder.Services.AddDbContextFactory<BrewDbContext>(configureBrewDbContext);

builder.Services.AddScoped<ICoffeeReadService, CoffeeReadService>();
builder.Services.AddScoped<IRecipeReadService, RecipeReadService>();
builder.Services.AddScoped<IRankingReadService, RankingReadService>();
builder.Services.AddScoped<IBrewingMethodReadService, BrewingMethodReadService>();
builder.Services.AddScoped<ICuppingSessionWriteService, CuppingSessionWriteService>();
builder.Services.AddScoped<ICuppingSessionReadService, CuppingSessionReadService>();
builder.Services.AddScoped<IQuickNoteService, QuickNoteService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ICommunityService, CommunityService>();
builder.Services.AddScoped<IRankingRefreshService, RankingRefreshService>();
builder.Services.AddOptions<RankingRefreshOptions>()
    .Bind(builder.Configuration.GetSection(RankingRefreshOptions.SectionName))
    .PostConfigure<IHostEnvironment>((options, environment) =>
    {
        if (environment.IsProduction())
        {
            options.Enabled = true;
        }
    });
builder.Services.AddHostedService<RankingRefreshBackgroundService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddScoped<IRecipeFavoriteService, RecipeFavoriteService>();
builder.Services.AddScoped<IRecipeValidationService, RecipeValidationService>();
builder.Services.AddScoped<ICoffeeFavoriteService, CoffeeFavoriteService>();

builder.Services.AddScoped<IArticleReadService, ArticleReadService>();
builder.Services.AddScoped<IArticleWriteService, ArticleWriteService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGlobalSearchService, GlobalSearchService>();

builder.Services.AddScoped<IPreferenceService, PreferenceService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("frontend");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program
{
}