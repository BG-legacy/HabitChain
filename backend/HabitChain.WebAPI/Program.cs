using HabitChain.Application.Interfaces;
using HabitChain.Application.Mappings;
using HabitChain.Application.Services;
using HabitChain.Domain.Entities;
using HabitChain.Domain.Interfaces;
using HabitChain.Infrastructure.Data;
using HabitChain.Infrastructure.Repositories;
using HabitChain.Infrastructure.Services;
using HabitChain.WebAPI.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AutoMapper;
using DotNetEnv;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to only use HTTP (no HTTPS) in production
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080); // Only HTTP on port 8080
});

// Override any HTTPS URLs from configuration
builder.WebHost.UseUrls("http://+:8080");

// Add environment variables to configuration
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Disable automatic model validation for faster responses
        options.SuppressModelStateInvalidFilter = false;
    });

// Add response compression for faster responses
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Override with environment variables if available
var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbName = Environment.GetEnvironmentVariable(builder.Environment.IsDevelopment() ? "DEV_DB_NAME" : "DB_NAME");
var dbUser = Environment.GetEnvironmentVariable("DB_USERNAME");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

// If environment variables are provided, construct connection string from them
if (!string.IsNullOrEmpty(dbHost) && !string.IsNullOrEmpty(dbName) && 
    !string.IsNullOrEmpty(dbUser) && !string.IsNullOrEmpty(dbPassword))
{
    var sslMode = Environment.GetEnvironmentVariable("DB_SSL_MODE") ?? "Require";
    var trustServerCertificate = Environment.GetEnvironmentVariable("DB_TRUST_SERVER_CERTIFICATE") ?? "true";
    var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "6543";
    
    // Use the pooler username format for Supabase
    // Disable multiplexing in connection string for better transaction support
    connectionString = $"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPassword};Port={port};SSL Mode={sslMode};Trust Server Certificate={trustServerCertificate};Pooling=true;MinPoolSize=5;MaxPoolSize=50;ConnectionIdleLifetime=60;ConnectionPruningInterval=5;Timeout=10;CommandTimeout=10;InternalCommandTimeout=30;Multiplexing=false;No Reset On Close=true;Include Error Detail=true";
}

builder.Services.AddDbContext<HabitChainDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
        
        // Add additional connection resiliency
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    }));

// Add ASP.NET Identity with performance optimizations
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Simplified password settings for faster registration
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;  // Reduced requirement
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 0;   // Reduced requirement

    // Disable lockout for faster registration (can be re-enabled later)
    options.Lockout.AllowedForNewUsers = false;
    options.Lockout.MaxFailedAccessAttempts = 10;  // Increased threshold
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);  // Reduced lockout time

    // User settings
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<HabitChainDbContext>()
.AddDefaultTokenProviders();

// Configure password hasher for ultra-fast development performance
builder.Services.Configure<PasswordHasherOptions>(options =>
{
    // Ultra-low iterations for instant development experience (use higher values in production)
    options.IterationCount = builder.Environment.IsDevelopment() ? 100 : 10000;  // 100 for dev, 10k for prod
    options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
});

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
               jwtSettings["SecretKey"] ?? 
               throw new InvalidOperationException("JWT SecretKey is not configured");
var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? 
            jwtSettings["Issuer"] ?? 
            throw new InvalidOperationException("JWT Issuer is not configured");
var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? 
              jwtSettings["Audience"] ?? 
              throw new InvalidOperationException("JWT Audience is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add repositories
builder.Services.AddScoped<IHabitRepository, HabitRepository>();
builder.Services.AddScoped<IHabitEntryRepository, HabitEntryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICheckInRepository, CheckInRepository>();
builder.Services.AddScoped<IBadgeRepository, BadgeRepository>();
builder.Services.AddScoped<IUserBadgeRepository, UserBadgeRepository>();
builder.Services.AddScoped<IEncouragementRepository, EncouragementRepository>();

// Add services
builder.Services.AddScoped<IHabitService, HabitService>();
builder.Services.AddScoped<IHabitEntryService, HabitEntryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IBadgeService, BadgeService>();
builder.Services.AddScoped<IBadgeEarningService, BadgeEarningService>();
builder.Services.AddScoped<IUserBadgeService, UserBadgeService>();
builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<IEncouragementService, EncouragementService>();
builder.Services.AddScoped<IAiRecommendationService, AiRecommendationService>();
builder.Services.AddScoped<IExportService, ExportService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Fast startup - minimal database checking
Console.WriteLine($"🚀 Environment: {app.Environment.EnvironmentName} - Fast Startup Mode");

// Background seeding - don't block application startup
_ = Task.Run(async () =>
{
    await Task.Delay(2000); // Allow app to start first
    using var scope = app.Services.CreateScope();
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<HabitChainDbContext>();
        
        // Quick badge check and seed if needed (background)
        var hasBadges = await context.Badges.AnyAsync();
        if (!hasBadges)
        {
            Console.WriteLine("🔄 Background: Seeding essential badges...");
            await DbSeeder.SeedBadgesAsync(context);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Background: Essential badges seeded.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Background seeding error: {ex.Message}");
    }
});

Console.WriteLine("⚡ Ready for instant requests!");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    // Validate AutoMapper configuration in development
    var mapper = app.Services.GetRequiredService<IMapper>();
    mapper.ConfigurationProvider.AssertConfigurationIsValid();
}

// DO NOT USE HTTPS REDIRECTION - HTTP only in production
Console.WriteLine("Application configured for HTTP only (no HTTPS redirection)");

// Enable response compression for faster responses
app.UseResponseCompression();

app.UseCors("AllowAll");

// Add token expiration handling middleware
app.UseTokenExpirationHandling();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Console.WriteLine("Starting application on http://+:8080");
app.Run();
