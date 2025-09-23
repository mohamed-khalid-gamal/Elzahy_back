using Elzahy.Services;
using Elzahy.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

namespace Elzahy
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure MySQL Database
            var connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING") 
                ?? builder.Configuration.GetConnectionString("MySqlConnection")
                ?? builder.Configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
                throw new Exception("Database connection string is missing. Set MYSQL_CONNECTION_STRING environment variable or configure ConnectionStrings:MySqlConnection in appsettings.json");

            // Use MySQL if connection string contains "mysql" or "Server=", otherwise use SQL Server
            if (connectionString.Contains("mysql", StringComparison.OrdinalIgnoreCase) || connectionString.Contains("Server=") && !connectionString.Contains("Trusted_Connection"))
            {
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
            }
            else
            {
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(connectionString));
            }

            // Configure DataProtection
            var dataProtectionKeysPath = Environment.GetEnvironmentVariable("DOTNET_DATAPROTECTION_KEYS") 
                ?? builder.Configuration["DataProtection:KeysPath"] 
                ?? "./keys";

            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
                .SetApplicationName("ElzahyPortfolio");

            // Configure CORS
            builder.Services.AddCors(options =>
            {
                var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") 
                    ?? builder.Configuration["App:FrontendUrl"] 
                    ?? "http://localhost:4200";

                options.AddPolicy("Default", policy =>
                {
                    policy.WithOrigins(frontendUrl, "https://angular-example-app.netlify.app")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // Configure JWT Authentication
            var jwtSecretKey = Environment.GetEnvironmentVariable("DOTNET_JWT_KEY") 
                ?? Environment.GetEnvironmentVariable("JWT__Key")
                ?? builder.Configuration["JwtSettings:SecretKey"];

            if (string.IsNullOrEmpty(jwtSecretKey))
                throw new Exception("JWT SecretKey is missing. Set DOTNET_JWT_KEY or JWT__Key environment variable.");

            var key = Encoding.UTF8.GetBytes(jwtSecretKey);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role
                };
            });

            // Configure Authorization
            builder.Services.AddAuthorization();

            // Register Services
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IProjectService, ProjectService>();
            builder.Services.AddScoped<IAwardService, AwardService>();
            builder.Services.AddScoped<IContactMessageService, ContactMessageService>();

            // Add Controllers
            builder.Services.AddControllers();

            // Configure Swagger/OpenAPI - Always add it, but conditionally enable UI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "Elzahy Portfolio API", 
                    Version = "v1",
                    Description = "A production-ready API with 2FA authentication for Elzahy Portfolio"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' followed by your JWT token.\n\nExample: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            });

            var app = builder.Build();

            // Log environment information early
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Application starting...");
            logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
            logger.LogInformation("Is Development: {IsDevelopment}", app.Environment.IsDevelopment());

            // Configure the HTTP request pipeline
            // Enable Swagger in Development and optionally in other environments
            if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwagger", false))
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Elzahy Portfolio API V1");
                    c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
                    c.DocumentTitle = "Elzahy Portfolio API";
                });
                
                logger.LogInformation("Swagger UI enabled at root path (/)");
            }
            else
            {
                logger.LogInformation("Swagger UI is disabled in {Environment} environment", app.Environment.EnvironmentName);
            }

            // Security Headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
                
                if (!app.Environment.IsDevelopment())
                {
                    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
                    context.Response.Headers.Add("Content-Security-Policy", 
                        "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;");
                }
                
                await next();
            });

            // Enforce HTTPS in production
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseCors("Default");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Add a simple health check endpoint
            app.MapGet("/health", () => Results.Ok(new { 
                Status = "Healthy", 
                Environment = app.Environment.EnvironmentName,
                Timestamp = DateTime.UtcNow 
            }));

            // Add a root endpoint that redirects to Swagger in development
            if (app.Environment.IsDevelopment())
            {
                app.MapGet("/swagger", () => Results.Redirect("/"));
            }

            // Database initialization and seeding
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                    var scopedLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                    // Ensure database is created (in production, use migrations)
                    if (app.Environment.IsDevelopment())
                    {
                        await dbContext.Database.EnsureCreatedAsync();
                    }
                    else
                    {
                        // In production, use migrations
                        await dbContext.Database.MigrateAsync();
                    }

                    // Seed default admin user
                    await authService.SeedDefaultAdminAsync();
                    
                    scopedLogger.LogInformation("Database initialization completed successfully");
                }
                catch (Exception ex)
                {
                    var scopedLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    scopedLogger.LogError(ex, "An error occurred while initializing the database");
                    
                    // In production, you might want to throw here to prevent startup with a broken database
                    if (!app.Environment.IsDevelopment())
                    {
                        throw;
                    }
                }
            }

            // Logging startup information
            logger.LogInformation("Elzahy Portfolio API started successfully");
            logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
            logger.LogInformation("Database: {DatabaseType}", 
                connectionString.Contains("mysql", StringComparison.OrdinalIgnoreCase) ? "MySQL" : "SQL Server");

            await app.RunAsync();
        }
    }
}
