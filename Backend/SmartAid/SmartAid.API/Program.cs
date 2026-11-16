
using BLL.MappingProfiles;
using BLL.Services;
using BLL.Services.Interfaces;
using BLL.Services.ExternalServices;
using DAL;
using DAL.Repositories;
using DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using SmartAid.API.Filters;
using SmartAid.API.BackgroundServices;
using SmartAid.API.Hubs;
using System.Text;
using SmartAid.API.Services;

var builder = WebApplication.CreateBuilder(args);

// API Keys diagnostics
Console.WriteLine(
    $"TomTom key loaded: {!string.IsNullOrWhiteSpace(builder.Configuration["TomTom:ApiKey"])}");

var geminiApiKey = builder.Configuration["Gemini:ApiKey"];
Console.WriteLine(
    $"Gemini API key loaded: {!string.IsNullOrWhiteSpace(geminiApiKey)}");

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"UserSecretsId: {builder.Configuration["UserSecretsId"]}");

// JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "JWT Key is missing or too short. Configure Jwt__Key.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        // Allow SignalR to receive the JWT from the connection query string.
        // This is restricted to the notifications Hub only.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/notifications"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClient", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://smartaid-client.runasp.net"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Images folder
var imagesFolder = Path.GetFullPath(Path.Combine(
    builder.Environment.ContentRootPath,
    builder.Configuration["ImagesFolder"] ?? "../Images"));

Directory.CreateDirectory(imagesFolder);

// Database
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connStr))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is missing. " +
        "Set it with an environment variable.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connStr));

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVolunteerRepository, VolunteerRepository>();
builder.Services.AddScoped<IEquipmentTypeRepository, EquipmentTypeRepository>();
builder.Services.AddScoped<IHelpTypeRepository, HelpTypeRepository>();
builder.Services.AddScoped<IVolunteerEquipmentRepository, VolunteerEquipmentRepository>();
builder.Services.AddScoped<IHelpRequestRepository, HelpRequestRepository>();
builder.Services.AddScoped<IRequestEquipmentRepository, RequestEquipmentRepository>();
builder.Services.AddScoped<IRequestOfferRepository, RequestOfferRepository>();
builder.Services.AddHostedService<OfferExpirationBackgroundService>();

// Services
builder.Services.AddScoped<IVolunteerService, VolunteerService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHelpRequestService, HelpRequestService>();
builder.Services.AddScoped<IRequestEquipmentService, RequestEquipmentService>();
builder.Services.AddScoped<IRequestOfferService, RequestOfferService>();
builder.Services.AddScoped<IEquipmentTypeService, EquipmentTypeService>();

builder.Services.AddScoped<
    IVolunteerMatchingService,
    VolunteerMatchingService>();

builder.Services.AddScoped<IVolunteerEquipmentService, VolunteerEquipmentService>();

// TomTom
builder.Services.AddHttpClient<
    ITomTomRoutingService,
    TomTomRoutingService>(client =>
    {
        client.BaseAddress = new Uri("https://api.tomtom.com/");
    });

// AI Image Analysis
builder.Services.AddHttpClient<
    IImageAnalysisService,
    ImageAnalysisService>();

// Image service
builder.Services.AddSingleton<IImageService>(
    new ImageService(imagesFolder));

// Authentication services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IPasswordHasher<DAL.Entities.User>,
    PasswordHasher<DAL.Entities.User>>();

// Controllers
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

builder.Services.AddEndpointsApiExplorer();

// SignalR
builder.Services.AddSignalR();

builder.Services.AddMemoryCache();

builder.Services.AddScoped<EstimatedArrivalService>();

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});
builder.Services.AddScoped<IVolunteerNotificationService, VolunteerNotificationService>();
var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Static images
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesFolder),
    RequestPath = "/images"
});

app.UseHttpsRedirection();

// CORS before authentication and authorization
app.UseCors("ReactClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR notifications Hub
app.MapHub<NotificationsHub>("/hubs/notifications");

// TomTom debug endpoint
if (app.Environment.IsDevelopment())
{
    app.MapGet("/debug/tomtom", async (
        ITomTomRoutingService tomTom) =>
    {
        var origins = new[]
        {
            new TomTomLocation(32.0809, 34.7806)
        };

        var destination = new TomTomLocation(
            32.0740,
            34.7925);

        var results = await tomTom.CalculateRoutesAsync(
            origins,
            destination);

        return Results.Ok(results);
    }).AllowAnonymous();
}

app.Run();
