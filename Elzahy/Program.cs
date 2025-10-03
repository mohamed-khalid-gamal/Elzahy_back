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

            // Configure Database
            var mySqlEnvConnection = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING");
            var sqlServerConnection = builder.Configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(mySqlEnvConnection) && string.IsNullOrWhiteSpace(sqlServerConnection))
                throw new Exception("Database connection string is missing. Set MYSQL_CONNECTION_STRING environment variable for MySQL or configure ConnectionStrings:DefaultConnection in appsettings.json for SQL Server");

            if (!string.IsNullOrWhiteSpace(mySqlEnvConnection))
            {
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseMySql(mySqlEnvConnection, ServerVersion.AutoDetect(mySqlEnvConnection)));
            }
            else
            {
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(sqlServerConnection));
            }

            // Configure DataProtection
            var dataProtectionKeysPath = Environment.GetEnvironmentVariable("DOTNET_DATAPROTECTION_KEYS")
                ?? builder.Configuration["DataProtection:KeysPath"]
                ?? "./keys";

            try
            {
                Directory.CreateDirectory(dataProtectionKeysPath);
            }
            catch { }

            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
                .SetApplicationName("ElzahyPortfolio");

            // Configure CORS
            builder.Services.AddCors(options =>
            {
                // Normalize all potential frontend origins (strip trailing slashes)
                var configuredFrontendUrl = (Environment.GetEnvironmentVariable("FRONTEND_URL")
                    ?? builder.Configuration["App:FrontendUrl"]
                    ?? "https://elzahygroup.com").TrimEnd('/');

                var allowedOrigins = new[]
                {
                    configuredFrontendUrl,
                    "https://elzahygroup.com".TrimEnd('/'),
                    "http://localhost:4200".TrimEnd('/')
                }
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

                options.AddPolicy("Default", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
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

            builder.Services.AddAuthorization();

            // Register Services
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IProjectService, ProjectService>();
            builder.Services.AddScoped<IAwardService, AwardService>();
            builder.Services.AddScoped<IContactMessageService, ContactMessageService>();
            builder.Services.AddScoped<IFileStorageService, FileStorageService>(); // NEW: File storage service

            builder.Services.AddControllers();

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
                    Description = "Enter 'Bearer' followed by your JWT token. Example: Bearer eyJhbGciOiJI..."
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

            // Track DB readiness
            var dbReady = true; // optimistic
            builder.Services.AddSingleton(() => new { DbReady = dbReady });

            var app = builder.Build();

            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Application starting... Environment={Environment} Development={IsDev}", app.Environment.EnvironmentName, app.Environment.IsDevelopment());

            // Ensure upload directories exist
            var webRootPath = app.Environment.WebRootPath;
            var uploadDirs = new[]
            {
                Path.Combine(webRootPath, "uploads"),
                Path.Combine(webRootPath, "uploads", "images"),
                Path.Combine(webRootPath, "uploads", "videos"),
                Path.Combine(webRootPath, "uploads", "projects")
            };

            foreach (var dir in uploadDirs)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    logger.LogInformation("Created upload directory: {Directory}", dir);
                }
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Elzahy Portfolio API V1");
                c.RoutePrefix = string.Empty;
                c.DocumentTitle = "Elzahy Portfolio API";
            });
            logger.LogInformation("Swagger UI enabled");

            // Configure static file serving for uploads
            app.UseStaticFiles(); // Default wwwroot serving
            
            // Custom static files configuration for uploads with caching
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = "/uploads",
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
                    Path.Combine(app.Environment.WebRootPath, "uploads")),
                OnPrepareResponse = ctx =>
                {
                    // Set cache headers for uploaded files
                    var headers = ctx.Context.Response.Headers;
                    headers["Cache-Control"] = "public,max-age=31536000"; // 1 year
                    headers["Expires"] = DateTime.UtcNow.AddYears(1).ToString("R");
                }
            });

            // IMPORTANT: Explicit routing before CORS / Auth so CORS headers are applied
            app.UseRouting();

            app.UseCors("Default");
            app.UseAuthentication();
            app.UseAuthorization();

            // Simple readiness endpoint (reports DB status)
            app.MapGet("/health", () => Results.Ok(new
            {
                Status = "Healthy",
                Environment = app.Environment.EnvironmentName,
                Timestamp = DateTime.UtcNow
            }));

            app.MapGet("/readiness", () =>
            {
                return dbReady
                    ? Results.Ok(new { Ready = true })
                    : Results.Problem("Database not initialized", statusCode: 503);
            });

            app.MapGet("/swagger", () => Results.Redirect("/"));

            app.MapControllers();

            // Database initialization and seeding (non-blocking resilience improvements)
            using (var scope = app.Services.CreateScope())
            {
                var scopedLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                try
                {
                    var skipMigrations = (Environment.GetEnvironmentVariable("SKIP_DB_MIGRATIONS") ?? "false").Equals("true", StringComparison.OrdinalIgnoreCase);
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

                    if (skipMigrations)
                    {
                        scopedLogger.LogWarning("SKIP_DB_MIGRATIONS=true => Skipping EnsureCreated/Migrate calls.");
                    }
                    else
                    {
                        if (app.Environment.IsDevelopment())
                        {
                            scopedLogger.LogInformation("Ensuring database is created (Development)");
                            await dbContext.Database.EnsureCreatedAsync();
                        }
                        else
                        {
                            scopedLogger.LogInformation("Applying migrations (Production/Non-Development)");
                            await dbContext.Database.MigrateAsync();
                        }
                    }

                    await authService.SeedDefaultAdminAsync();
                    scopedLogger.LogInformation("Database initialization completed successfully");
                }
                catch (Exception ex)
                {
                    dbReady = false; // mark not ready
                    var inner = ex.InnerException?.Message;
                    scopedLogger.LogError(ex, "Database initialization failed. Inner={Inner}", inner);
                    // Do NOT rethrow in production to avoid process crash & connection reset.
                    if (app.Environment.IsDevelopment())
                    {
                        throw; // In dev we still want to know immediately.
                    }
                }
            }

            logger.LogInformation("Elzahy Portfolio API started. Provider={Provider}", string.IsNullOrWhiteSpace(mySqlEnvConnection) ? "SqlServer" : "MySql");
            await app.RunAsync();
        }
    }
}
