using System.Text;
using eLibrary.API.Configurations;
using eLibrary.Application.Commands.Auth;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Application.Interfaces.Services;
using eLibrary.Domain.Entities;
using eLibrary.Infrastructure.Data;
using eLibrary.Infrastructure.Repositories;
using eLibrary.Infrastructure.Services;
using eLibrary.Shared;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Bind SmtpSettings section from appsettings.json
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));

// Register your EmailService for DI
builder.Services.AddSingleton<EmailService>();

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileStorage, LocalFileStorage>();


//CQRS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});




//for auto token service registration
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.WriteIndented = true; // 👈 pretty print
});

builder.Services.AddControllers();

builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

// Register validators from your assembly
var assembly = typeof(Program).Assembly;
foreach (var validatorType in assembly.GetTypes()
    .Where(t => t.IsClass && !t.IsAbstract && 
               t.GetInterfaces().Any(i => i.IsGenericType && 
               i.GetGenericTypeDefinition() == typeof(IValidator<>))))
{
    var interfaceType = validatorType.GetInterfaces()
        .First(i => i.IsGenericType && 
               i.GetGenericTypeDefinition() == typeof(IValidator<>));
    builder.Services.AddScoped(interfaceType, validatorType);
}

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IOtpService, OtpService>();
//File service dependency
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<INotificationService, NotificationService>();//notification service dependency

// Configure Swagger with JWT Authentication support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "📚 Evidya – E-Library Management", Version = "v1" });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

// Connection string to the database
builder.Services.AddDbContext<EBookDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBorrowRepository, BorrowRepository>();

// PasswordHasher
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// MediatR (single call for multiple assemblies if needed)
builder.Services.AddMediatR(typeof(Program).Assembly, typeof(RegisterUserCommand).Assembly);

// JWT settings from appsettings.json
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);
var jwtKey = jwtSettingsSection["Key"] ?? throw new InvalidOperationException("JWT Key is not configured in appsettings.json");
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.Configure<ResetTokenSettings>(
    builder.Configuration.GetSection("ResetTokenSettings"));
    
    // In-Memory Caching
builder.Services.AddMemoryCache();


// Authentication with JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Change to true in production!
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero ,// ✅ No grace period — expires exactly on time
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettingsSection["Issuer"],
        ValidAudience = jwtSettingsSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Role-based access
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseCors("AllowReactApp");

// HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "📚 Evidya API");
    });
}

app.UseHttpsRedirection();

// Add Authentication middleware before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseStaticFiles();

app.Run();
