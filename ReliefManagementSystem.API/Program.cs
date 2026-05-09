using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using ReliefManagementSystem.API.Middleware;
using ReliefManagementSystem.Application;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure;
using ReliefManagementSystem.Infrastructure.Data;
using ReliefManagementSystem.Infrastructure.Security;
using ReliefManagementSystem.Infrastructure.Seed;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.RateLimiting;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            message = "Too many requests. Please try again later.",
            code = "RATE_LIMIT_EXCEEDED",
            traceId = context.HttpContext.TraceIdentifier,
            statusCode = 429
        }, cancellationToken: token);
    };

    // Auth endpoints: chặt hơn
    options.AddPolicy("auth", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = $"auth:{ip}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: key,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    // Tạo rescue request: giới hạn vừa phải
    options.AddPolicy("rescue-create", httpContext =>
    {
        var userId = httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = !string.IsNullOrWhiteSpace(userId)
            ? $"rescue-create:user:{userId}"
            : $"rescue-create:ip:{ip}";

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: key,
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4,
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});
builder.Services.AddHealthChecks();
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "ReliefCare:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Relief Management API",
        Version = "v1"
    });

    c.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập: Bearer {JWT token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    c.EnableAnnotations();
});



builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.Configure<CentrifugoSettings>(
    builder.Configuration.GetSection("Centrifugo"));

builder.Services.Configure<GoogleSetting>(
    builder.Configuration.GetSection("AuthenticationGoogle"));

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.Configure<PayOsSettings>(
    builder.Configuration.GetSection("PayOs"));

builder.Services.Configure<GoongSettings>(
    builder.Configuration.GetSection("Goong"));

builder.Services.Configure<WeatherApiSettings>(
    builder.Configuration.GetSection("WeatherApi"));

builder.Services.Configure<VisualCrossingSettings>(
    builder.Configuration.GetSection("VisualCrossing"));

builder.Services.Configure<DisasterAnalysisSettings>(
    builder.Configuration.GetSection("DisasterAnalysis"));

builder.Services.Configure<LlmProviderSettings>(
    builder.Configuration.GetSection("LlmProvider"));

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
var googleSettings = builder.Configuration.GetSection("AuthenticationGoogle").Get<GoogleSetting>();
var cloudSetting = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
var brevoEmailSetting = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings!.Issuer,
            ValidAudience = jwtSettings.Audience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key)
            )
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";

                var traceId = context.HttpContext.TraceIdentifier;

                var result = new
                {
                    message = "You are not authenticated.",
                    code = "AUTH_UNAUTHORIZED",
                    traceId = traceId,
                    statusCode = 401
                };

                await context.Response.WriteAsJsonAsync(result);
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";

                var traceId = context.HttpContext.TraceIdentifier;

                var result = new
                {
                    message = "You do not have permission to perform this action.",
                    code = "AUTH_FORBIDDEN",
                    traceId = traceId,
                    statusCode = 403
                };

                await context.Response.WriteAsJsonAsync(result);
            }
        };


    })
    .AddGoogle(options =>
{
    options.ClientId =
         builder.Configuration["AuthenticationGoogle:Google:ClientId"]!;

    options.ClientSecret =
        builder.Configuration["AuthenticationGoogle:Google:ClientSecret"]!;
});


builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS
var allowedOrigins = builder.Configuration
    .GetSection("CorsSettings:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        //.AllowCredentials();
    });
});

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    var context = services.GetRequiredService<ApplicationDbContext>();
    
    // Auto-migrate database on startup (useful for Docker/Development)
    try
    {
        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw; // Fail fast if migration fails
    }
    
    // Seed data
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    
    await RoleSeeder.SeedAsync(roleManager);
    await UserSeeder.SeedAsync(userManager, context);
    await SkillSeeder.SeedAsync(context);
    await TeamSeeder.SeedAsync(context);
    await LocationExcelSeeder.SeedAsync(context);
    await ReliefStationSeeder.SeedAsync(context);
    await SupplyItemSeeder.SeedAsync(context);
    await VehicleTypeSeeder.SeedAsync(context);
    await VehicleSeeder.SeedAsync(context);
    await CampaignSeeder.SeedAsync(context);
    await ReliefPackageTestCampaignSeeder.SeedAsync(context);
    await ManagerProfileSeeder.SeedAsync(context);
    await PriorityCriteriaSeeder.SeedAsync(context);
    logger.LogInformation("Database seeding completed.");
}

// Configure the HTTP request pipeline.
// Enable Swagger for Development and Staging
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionMiddleware>();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" })).AllowAnonymous();
app.MapHealthChecks("/healthz");

app.Run();
