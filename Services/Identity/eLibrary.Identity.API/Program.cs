using System.Text;
using eLibrary.Identity.Application.Commands.Auth;
using eLibrary.Identity.Application.Interfaces.Repositories;
using eLibrary.Identity.Application.Interfaces.Services;
using eLibrary.Identity.Domain.Entities;
using eLibrary.Identity.Infrastructure.Persistence;
using eLibrary.Identity.Infrastructure.Repositories;
using eLibrary.Identity.Infrastructure.Services;
using eLibrary.Common.Api;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1) API basics
builder.Services.AddControllers().AddApiErrorHandling();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Paste your JWT access token without the 'Bearer ' prefix. Swagger adds it automatically.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT"
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

// 2) Database and repositories
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.MigrationsHistoryTable("__IdentityMigrationsHistory")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMongoMediaStore, MongoMediaStore>();

// 3) Identity and auth services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// 4) MediatR registration for command/handler flow
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));

// 5) JWT settings for token validation
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
var jwtKey = jwtSettingsSection["Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
var jwtIssuer = jwtSettingsSection["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured.");
var jwtAudience = jwtSettingsSection["Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured.");
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseApiErrorHandling();

if (app.Environment.IsDevelopment() && !string.IsNullOrWhiteSpace(builder.Configuration["IdentitySeed:AdminPassword"]))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    var adminUsername = builder.Configuration["IdentitySeed:AdminUsername"] ?? "admin";
    var adminEmail = builder.Configuration["IdentitySeed:AdminEmail"] ?? "admin@elibrary.local";

    if (!await context.Users.AnyAsync(user => user.Username == adminUsername))
    {
        var admin = new User
        {
            Username = adminUsername,
            Email = adminEmail,
            Role = "Admin"
        };
        admin.PasswordHash = passwordHasher.HashPassword(admin, builder.Configuration["IdentitySeed:AdminPassword"]!);
        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
