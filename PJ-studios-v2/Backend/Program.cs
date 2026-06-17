using Backend.Data;
using Backend.Migrations;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables into configuration
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? "";
    builder.Configuration["Jwt:Key"] = jwtKey;
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? "";
}

var mailPassword = builder.Configuration["MailSettings:Password"];
if (string.IsNullOrEmpty(mailPassword))
{
    mailPassword = Environment.GetEnvironmentVariable("MAIL_PASSWORD") ?? "";
    builder.Configuration["MailSettings:Password"] = mailPassword;
}

var mailFrom = builder.Configuration["MailSettings:From"];
if (string.IsNullOrEmpty(mailFrom))
{
    mailFrom = Environment.GetEnvironmentVariable("MAIL_FROM") ?? "";
    builder.Configuration["MailSettings:From"] = mailFrom;
}

var jwt = builder.Configuration.GetSection("Jwt");

const string FrontendCorsPolicy = "FrontendCorsPolicy";

// Configure Heroku port
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://*:{port}");
}

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddSingleton<MailService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Authentication + JWT
builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            )
        };
    });

// Swagger + JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Backend", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT token",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await dbContext.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS ""Items"" (
            ""Id"" text PRIMARY KEY,
            ""UserId"" text NOT NULL DEFAULT '',
            ""Name"" text NOT NULL,
            ""Description"" text NOT NULL,
            ""ImageUrl"" text NOT NULL DEFAULT ''
        );
    ");

    await dbContext.Database.ExecuteSqlRawAsync(@"
        ALTER TABLE ""Items""
        ADD COLUMN IF NOT EXISTS ""UserId"" text NOT NULL DEFAULT '',
        ADD COLUMN IF NOT EXISTS ""ImageUrl"" text NOT NULL DEFAULT '';
    ");

    await dbContext.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS ""Ratings"" (
            ""Id"" SERIAL PRIMARY KEY,
            ""ItemId"" text NOT NULL,
            ""UserId"" text NOT NULL,
            ""Score"" numeric(4,1) NOT NULL CHECK (""Score"" >= 1 AND ""Score"" <= 10)
        );
    ");
}

app.MapOpenApi();

app.SeedDefaultUsers(builder.Configuration);
app.ApplyMigrations();

app.UseSwagger();
app.UseSwaggerUI();

// Only redirect to HTTPS in development
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors(FrontendCorsPolicy);

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
