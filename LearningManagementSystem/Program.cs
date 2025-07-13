
using System.Text;
using LearningManagementSystem.Data;
using LearningManagementSystem.IServices;
using LearningManagementSystem.Models;
using LearningManagementSystem.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
//using Microsoft.Extensions.Caching.StackExchangeRedis;
using Serilog;
using Serilog.Sinks.Async;
//using Microsoft.AspNetCore.SignalR;
using LearningManagementSystem.Extensions;
using LearningManagementSystem.IUOW;
using LearningManagementSystem.UOW;
using Serilog.Events;
//using StackExchange.Redis;
using LearningManagementSystem.Filters;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using HealthChecks.UI.Client;

namespace LearningManagementSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Async(a => a.Console())
            .WriteTo.Async(a => a.File("logs/lms.txt", rollingInterval: RollingInterval.Day))
            .CreateLogger();
            try
            {
                var builder = WebApplication.CreateBuilder(args);
                builder.Host.UseSerilog();
                builder.Services.AddControllers();
                //builder.Services.AddStackExchangeRedisCache(options =>
                //{
                //    options.Configuration = builder.Configuration.GetConnectionString("Redis");
                //    options.InstanceName = "LMS_";
                //});
                //            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
                //ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));
                //builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
                builder.Services.AddSignalR();
                builder.Services.AddHealthChecks()
                  .AddCheck("self", () => HealthCheckResult.Healthy("API is healthy"));
                // Add services to the container.
                builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
                builder.Services.AddScoped<ILoggingService, LoggingService>();
                builder.Services.AddScoped<LogActionFilter>();
                builder.Services.AddSingleton<PayPalService>();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                //builder.Services.AddHealthChecks().AddSqlServer(
                //    builder.Configuration.GetConnectionString("DefaultConnection"));
                //builder.Services.AddHealthChecks().AddCheck<DirectoryHealthCheck>("upload_directory_check", failureStatus: HealthStatus.Unhealthy, tags: new[] { "directory" });
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseLazyLoadingProxies().UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
                builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>(
                    name: "database",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "ready" });
                builder.Services.AddHealthChecks()
    .AddDiskStorageHealthCheck(
        setup: options => options.AddDrive("C:\\", 1024), // 1GB minimum free
        name: "disk_storage",
        failureStatus: HealthStatus.Degraded);

                builder.Services.AddHealthChecksUI(setup =>
                {
                    setup.AddHealthCheckEndpoint("API", "/health");
                    setup.SetEvaluationTimeInSeconds(30); // Check every 30 seconds
                    setup.SetMinimumSecondsBetweenFailureNotifications(60); // Notify every 60 seconds if unhealthy
                })
.AddInMemoryStorage();


                builder.Services.AddIdentity<User, Models.Role>(
                options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;

                // User settings
                options.User.RequireUniqueEmail = true;

                // Email confirmation required
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false; // Can be set to true if needed

                // Account lockout settings
                //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                //options.Lockout.MaxFailedAccessAttempts = 5;
                //options.Lockout.AllowedForNewUsers = true;

                // Token settings
                //options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                //options.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultEmailProvider;
                //options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
            }
            )
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            //ValidIssuer = builder.Configuration["Jwt:Issuer"],
            //ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

                builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                    options.AddPolicy("InstructorOnly", policy => policy.RequireRole("Instructor"));
                    options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
                    options.AddPolicy("DoctorOrStudent", policy => policy.RequireRole("Doctor", "Student"));
                    options.AddPolicy("DoctorOrAdmin", policy => policy.RequireRole("Doctor", "Admin"));
                });

                builder.Services.AddScoped<IAuthService, AuthService>();
                //builder.Services.AddScoped<IUserService, UserService>();
                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }
                app.UseSerilogRequestLogging();
                app.UseHttpsRedirection();

                app.UseAuthentication();
                app.UseAuthorization();
                app.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = async (context, report) =>
                    {
                        var result = JsonSerializer.Serialize(new
                        {
                            status = report.Status.ToString(),
                            checks = report.Entries.Select(e => new
                            {
                                name = e.Key,
                                status = e.Value.Status.ToString(),
                                description = e.Value.Description,
                                exception = e.Value.Exception?.Message
                            })
                        });

                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(result);
                    }
                });
                app.MapHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                app.MapHealthChecksUI(options =>
                {
                    options.UIPath = "/health-ui";
                    options.ApiPath = "/health-ui-api";
                });
                app.MapHealthChecks("/health/ready", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("ready")
                });

                // Liveness endpoint (minimal checks)
                app.MapHealthChecks("/health/live", new HealthCheckOptions
                {
                    Predicate = _ => false // No checks for basic liveness
                });
                
                app.MapHub<LMSHubb>("/lmshub");
                app.MapHealthChecks("/health");
                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
