using System.Text;
using FinancialPlanner.Api.Json;
using FinancialPlanner.Api.Middleware;
using FinancialPlanner.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Financial Planner API",
        Version = "v1"
    });
});

// JWT — empty GitHub secret becomes "", not null; SymmetricSecurityKey then throws at runtime (IDX10703).
static string RequireJwtValue(string? value, string configKey, string envName)
{
    if (string.IsNullOrWhiteSpace(value))
        throw new InvalidOperationException(
            $"{configKey} is missing or empty. For Docker/production set environment variable {envName} (e.g. GitHub Actions secret). Use a signing key of at least 32 characters for HS256.");
    return value;
}

var jwtKey = RequireJwtValue(builder.Configuration["Jwt:Key"], "Jwt:Key", "Jwt__Key");
var jwtIssuer = RequireJwtValue(builder.Configuration["Jwt:Issuer"], "Jwt:Issuer", "Jwt__Issuer");
var jwtAudience = RequireJwtValue(builder.Configuration["Jwt:Audience"], "Jwt:Audience", "Jwt__Audience");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        var origins = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "http://localhost:5173",
            "https://localhost:5173"
        };
        var extra = builder.Configuration["Cors:AllowedOrigins"];
        if (!string.IsNullOrWhiteSpace(extra))
        {
            foreach (var o in extra.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                origins.Add(o);
        }

        policy.WithOrigins(origins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DocumentTitle = "Financial Planner API";
    c.EnableDeepLinking();
    c.EnableFilter();
    c.EnableValidator();
    c.DisplayRequestDuration();
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    // Inject dark theme CSS
    c.InjectStylesheet("/swagger-ui/dark-theme.css");
});

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
