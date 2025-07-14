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

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add environment variables to configuration
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();

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
    connectionString = $"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPassword};Port={port};SSL Mode={sslMode};Trust Server Certificate={trustServerCertificate};Pooling=true;MinPoolSize=1;MaxPoolSize=20;ConnectionIdleLifetime=300;ConnectionPruningInterval=10;Timeout=30;CommandTimeout=30;InternalCommandTimeout=60;Multiplexing=false";
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

// Add ASP.NET Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<HabitChainDbContext>()
.AddDefaultTokenProviders();

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

// Seed the database
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<HabitChainDbContext>();
        Console.WriteLine("Attempting to seed database...");
        
        // Ensure database is created and migrations are applied
        await context.Database.EnsureCreatedAsync();
        
        // Seed data with retry logic
        var maxRetries = 3;
        var retryCount = 0;
        
        while (retryCount < maxRetries)
        {
            try
            {
                await DbSeeder.SeedAsync(context);
                Console.WriteLine("Database seeding completed successfully.");
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                Console.WriteLine($"Database seeding attempt {retryCount} failed: {ex.Message}");
                
                if (retryCount >= maxRetries)
                {
                    Console.WriteLine($"Database seeding failed after {maxRetries} attempts. Continuing without seeding.");
                    Console.WriteLine($"Exception type: {ex.GetType().Name}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                }
                else
                {
                    // Wait before retrying
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during database initialization: {ex.Message}");
        Console.WriteLine($"Exception type: {ex.GetType().Name}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        }
        // Don't throw here - let the application continue even if seeding fails
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    // Validate AutoMapper configuration in development
    var mapper = app.Services.GetRequiredService<IMapper>();
    mapper.ConfigurationProvider.AssertConfigurationIsValid();
}

// Configure HTTPS redirection only if we have a valid certificate
if (!app.Environment.IsDevelopment())
{
    // Only use HTTPS redirection if we have a valid certificate
    try
    {
        app.UseHttpsRedirection();
    }
    catch (InvalidOperationException)
    {
        Console.WriteLine("HTTPS redirection disabled - no valid certificate found");
    }
}

app.UseCors("AllowAll");

// Add token expiration handling middleware
app.UseTokenExpirationHandling();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add a simple health check endpoint
app.MapGet("/health", () => "Healthy");

app.Run();
