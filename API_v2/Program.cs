using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using API_v2.Datas;
using API_v2.Helpers;
using API_v2.Middleware;
using API_v2.Repositories;
using API_v2.Repositories.IRepositories;
using API_v2.Services;
using API_v2.Services.Interfaces;
using Scalar.AspNetCore;
using System.Linq;
using API_v2.Models.DTOs;
using Serilog;
using API_v2.Hubs;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using API_v2.Models.Constants;

// Enable Serilog self-logging to standard error to capture internal errors
Serilog.Debugging.SelfLog.Enable(Console.Error);

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog programmatically (independent of appsettings.json)
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", Serilog.Events.LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({CorrelationId}) {Message:lj}{NewLine}{Exception}");

    if (context.HostingEnvironment.IsDevelopment())
    {
        loggerConfiguration.WriteTo.File(
            path: "Logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            fileSizeLimitBytes: 10485760,
            rollOnFileSizeLimit: true,
            retainedFileCountLimit: 7,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({CorrelationId}) {Message:lj} {Properties:j}{NewLine}{Exception}"
        );
    }
});

// Add services to the container.
builder.Services.AddControllers(options => options.Filters.Add<ApiErrorResponseFilter>())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            var errorMessage = string.Join(" | ", errors);
            var response = new ApiErrorResponse
            {
                ErrorCode = ErrorCodes.ValidationFailed,
                Message = "Request validation failed.",
                Errors = errors.ToList(),
                CorrelationId = context.HttpContext.TraceIdentifier
            };
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });

builder.Services.AddMemoryCache();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    options.RequireHeaderSymmetry = true;

    // ASP.NET Core trusts loopback proxies by default. Additional reverse proxy
    // addresses must be explicitly configured; never trust arbitrary XFF senders.
    foreach (var configuredProxy in builder.Configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [])
    {
        if (!IPAddress.TryParse(configuredProxy, out var proxyAddress))
        {
            throw new InvalidOperationException($"Invalid ReverseProxy:KnownProxies address: {configuredProxy}");
        }
        options.KnownProxies.Add(proxyAddress);
    }
});

// Configure Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Policy for Login: 5 requests per minute
    options.AddPolicy("login", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });

    // Policy for OTP: 3 requests per 5 minutes
    options.AddPolicy("otp", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 3,
            Window = TimeSpan.FromMinutes(5),
            QueueLimit = 0
        });
    });

    // Custom response when rate limited
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsJsonAsync(
            new ApiResponse<object>(false, "Too many requests. Please try again later.", null), 
            token);
    };
});

// Configure EF Core with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskAssignmentRepository, TaskAssignmentRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IProjectColumnRepository, ProjectColumnRepository>();
builder.Services.AddScoped<IProjectFileRepository, ProjectFileRepository>();

// Register Services
builder.Services.AddSingleton<IGoogleDriveService, GoogleDriveService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskCommentService, TaskCommentService>();
builder.Services.AddScoped<ITaskFeedService, TaskFeedService>();
builder.Services.AddScoped<ITaskActivityService, TaskActivityService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProjectFolderService, ProjectFolderService>();
builder.Services.AddScoped<IProjectFileTransferService, ProjectFileTransferService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IProjectColumnService, ProjectColumnService>();
builder.Services.AddScoped<IProjectFileService, ProjectFileService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddHttpClient<IEmailService, EmailService>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
builder.Services.AddHostedService<EmailBackgroundService>();
builder.Services.AddHostedService<CleanupBackgroundService>();
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = null;
    });

// Register JwtHelper utility
builder.Services.AddScoped<JwtHelper>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://todo-list-tutai.vercel.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure JWT Bearer Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT security key is not configured.");
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddOpenApi();

var app = builder.Build();

// Automatically apply pending database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        Log.Information("Applying Database Migrations...");
        db.Database.Migrate();
        Log.Information("Database Migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "An error occurred while applying database migrations on startup.");
        throw;
    }
}

Log.Information("========== Application Started ==========");
// Configure HTTP request pipeline middlewares in the correct order
app.UseForwardedHeaders();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>(); 
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseStatusCodePages(async statusContext =>
{
    var response = statusContext.HttpContext.Response;
    var errorCode = response.StatusCode switch
    {
        StatusCodes.Status400BadRequest => ErrorCodes.ValidationFailed,
        StatusCodes.Status401Unauthorized => ErrorCodes.Unauthorized,
        StatusCodes.Status403Forbidden => ErrorCodes.Forbidden,
        StatusCodes.Status404NotFound => ErrorCodes.ResourceNotFound,
        StatusCodes.Status409Conflict => ErrorCodes.Conflict,
        _ => ErrorCodes.InternalServerError
    };
    var payload = new ApiErrorResponse
    {
        ErrorCode = errorCode,
        Message = "The request could not be completed.",
        CorrelationId = statusContext.HttpContext.TraceIdentifier
    };
    response.ContentType = "application/json";
    await response.WriteAsJsonAsync(payload);
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Accessible at /scalar/v1
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

 app.Run();
