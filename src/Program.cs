using FluentValidation;
using GateEntryExit.BackgroundJobs;
using GateEntryExit.BackgroundJobServices.Implementations;
using GateEntryExit.BackgroundJobServices.Interfaces;
using GateEntryExit.Caching;
using GateEntryExit.DatabaseContext;
using GateEntryExit.Domain;
using GateEntryExit.Domain.Manager;
using GateEntryExit.Domain.Policy;
using GateEntryExit.Dtos.Gate;
using GateEntryExit.Helper;
using GateEntryExit.Middlewares;
using GateEntryExit.Repositories;
using GateEntryExit.Repositories.Interfaces;
using GateEntryExit.Service.Cache;
using GateEntryExit.Service.Token;
using GateEntryExit.SignalR;
using GateEntryExit.Validators;
using Hangfire;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scryber.Components;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var JWTSetting = builder.Configuration.GetSection("JWTSetting");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// https://stackoverflow.com/questions/56234504/bearer-authentication-in-swagger-ui-when-migrating-to-swashbuckle-aspnetcore-ve
builder.Services.AddSwaggerGen(c => {
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization Example : 'Bearer sd23r43ffdhg545",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement(){
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "outh2",
                Name="Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

});

// 4200 - Angular
// 5189 - MVC
// 81 - JavaScript (IIS hosted)
// 44328 - Umbraco
// 8081 - Vue

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policyBuilder =>
                      {
                          policyBuilder.WithOrigins("http://localhost:4200", "http://localhost:5189", "http://localhost:81", "https://localhost:44328/", "http://localhost:8081");
                          policyBuilder.AllowAnyHeader();
                          policyBuilder.AllowAnyMethod();
                      });
});

builder.Services.AddDbContext<GateEntryExitDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<GateEntryExitDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IGateRepository, GateRepository>();
builder.Services.AddScoped<IGateManager, GateManager>();

builder.Services.AddScoped<IGateEntryRepository, GateEntryRepository>();
builder.Services.AddScoped<IGateEntryManager, GateEntryManager>();

builder.Services.AddScoped<IGateExitRepository, GateExitRepository>();
builder.Services.AddScoped<IGateExitManager, GateExitManager>();

builder.Services.AddScoped<ISensorRepository, SensorRepository>();
builder.Services.AddScoped<ISensorManager, SensorManager>();

builder.Services.AddScoped<IGateNameUniquePolicy, GateNameUniquePolicy>();

builder.Services.AddScoped<IValidator<CreateGateDto>, GateValidator>();

builder.Services.AddScoped<IGuidGenerator, GuidGenerator>();

builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IGateEntryExitBackgroundJobService, GateEntryExitBackgroundJobService>();

builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection")));

builder.Services.AddHangfireServer();


// Serilog
configureLogging();
builder.Host.UseSerilog();

// JWT
// https://code-maze.com/authentication-aspnetcore-jwt-1/
builder.Services.AddAuthentication(opt => {
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt => {
    opt.SaveToken = true;
    opt.RequireHttpsMetadata = false;
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = JWTSetting["ValidAudience"],
        ValidIssuer = JWTSetting["ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWTSetting.GetSection("securityKey").Value!))
    };
    opt.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            // Log the error or respond with a custom message
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");

            // Optional: return a custom response
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(new
            {
                StatusCode = 401,
                Message = "Authentication failed",
                Detailed = context.Exception?.Message
            }.ToString()); // Replace with proper JSON serializer
        },
        OnMessageReceived = context =>
        {
            // 1. Try to get the access token from the query string
            var accessToken = context.Request.Query["access_token"];

            // 2. Check if the path is for a specific endpoint (like SignalR)
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
            path.StartsWithSegments("/signalRHub"))
            {
                // 3.If we found a token and it's for the expected path, set it manually so the JWT middleware can validate it.

                // This line manually assigns the token(extracted from somewhere other than the standard Authorization
                // header) to the JWT authentication context. Once set, ASP.NET Core’s JWT middleware will use this token
                // for validation — just like it would if it came from the Authorization: Bearer<token> header.
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

var cachedData = new CachedData { GateKey = "secret" };
CachedDataHelper.SaveSecretsToFile(cachedData, "secrets.cache");
// Later can load cached data like
// CachedDataHelper.LoadSecretFromFule("secrets.cache");
builder.Services.AddSingleton<ICachedDataProvider>(new CachedDataProvider(cachedData));
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();
builder.Services.AddSignalR();
var app = builder.Build();

app.UseHangfireDashboard(builder.Configuration.GetSection("Hangfire:DashboardPath").Value.ToString(), new DashboardOptions
{
    DashboardTitle = builder.Configuration.GetSection("Hangfire:DashboardTitle").Value.ToString(),
    DarkModeEnabled = false,
    DisplayStorageConnectionString = false,
    Authorization = new[]
    {
        new HangfireCustomBasicAuthenticationFilter
        {
            User = builder.Configuration.GetSection("Hangfire:DashboardUserName").Value.ToString(),
            Pass = builder.Configuration.GetSection("Hangfire:DashboardPassword").Value.ToString()
        }
    }
});

app.UseMiddleware<HttpContextMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();

//app.MapControllers();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHangfireDashboard();
    endpoints.MapHub<SignalRHub>("/signalRHub");
});

RecurringJob.AddOrUpdate<GateEntryExitBackgroundJob>("GateEntryExitJob",
    job => job.Execute(),
    builder.Configuration.GetSection("Hangfire:CronExpression").Value.ToString(),
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time")
    });

GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete });
GlobalJobFilters.Filters.Add(new PreventDuplicateEnqueueFilter());
GlobalJobFilters.Filters.Add(new PreventDuplicateProcessingFilter());
GlobalJobFilters.Filters.Add(new BackgroundJobPauseFilter());
BackgroundJobPauseFilter.IsPaused = builder.Configuration.GetValue<bool>("Hangfire:Pausejobs");

app.Run();

void configureLogging()
{
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var configuration = new ConfigurationBuilder()
                        .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile(path: $"appsettings.{environment}.json", optional: true)
                        .Build();

    Log.Logger = new LoggerConfiguration()
                 .Enrich.FromLogContext()
                 .Enrich.WithExceptionDetails()
                 .WriteTo.Debug()
                 .WriteTo.Console()
                 .WriteTo.Elasticsearch(configureElasticSink(configuration, environment))
                 .Enrich.WithProperty("Environment", environment)
                 .ReadFrom.Configuration(configuration)
                 .CreateLogger();
}

ElasticsearchSinkOptions configureElasticSink(IConfigurationRoot configuration, string environment)
{
    return new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
    {
        AutoRegisterTemplate = true,
        IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".","-")}-{environment.ToLower()}-{DateTime.UtcNow:yyyy-MM}",
        NumberOfReplicas = 1,
        NumberOfShards = 2
    };
}

public partial class Program { }
