using LostUAL.Api.Seed;
using LostUAL.Data.Identity;
using LostUAL.Data.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LostUAL API", Version = "v1" });

    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega SOLO el token (sin 'Bearer ')"
    };

    c.AddSecurityDefinition("Bearer", bearerScheme);

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});





builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWeb", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins("https://localhost:7211");
    });
});
builder.Services.AddDbContext<LostUALDbContext>(options =>
{
   
    var dbPath = Path.Combine(builder.Environment.ContentRootPath, "lostual.db");
    options.UseSqlite($"Data Source={dbPath}");
});

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
       
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole>()               
    .AddEntityFrameworkStores<LostUALDbContext>()
    .AddSignInManager();

builder.Services.AddHostedService<LostUAL.Api.Services.ClaimAutoResolveService>();


var jwtSection = builder.Configuration.GetSection("Jwt");

var key = (jwtSection["Key"] ?? "").Trim();
var issuer = (jwtSection["Issuer"] ?? "").Trim();
var audience = (jwtSection["Audience"] ?? "").Trim();

if (string.IsNullOrWhiteSpace(key) ||
    string.IsNullOrWhiteSpace(issuer) ||
    string.IsNullOrWhiteSpace(audience))
{
    throw new InvalidOperationException("Faltan Jwt:Key / Jwt:Issuer / Jwt:Audience en la configuración.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.IncludeErrorDetails = true; 

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

        ClockSkew = TimeSpan.FromSeconds(30),
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            
            var logger = ctx.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("JWT");

            var auth = ctx.Request.Headers.Authorization.ToString();
            logger.LogInformation("Authorization header: {Auth}", auth);

           
            logger.LogInformation("Request: {Method} {Path}{Query}",
                ctx.HttpContext.Request.Method,
                ctx.HttpContext.Request.Path,
                ctx.HttpContext.Request.QueryString);
            
            return Task.CompletedTask;

        },
        /*
        OnTokenValidated = ctx =>
        {
            var logger = ctx.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("JWT");

            logger.LogInformation("JWT OK. User: {Name}", ctx.Principal?.Identity?.Name);
            return Task.CompletedTask;
        },*/

        OnTokenValidated = async ctx =>
        {
            var logger = ctx.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("JWT");

            var userId = ctx.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                ctx.Fail("Missing NameIdentifier");
                return;
            }

            var userManager = ctx.HttpContext.RequestServices
                .GetRequiredService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByIdAsync(userId);

            if (user is null)
            {
                ctx.Fail("User not found");
                return;
            }

            if (await userManager.IsLockedOutAsync(user))
            {
                var end = await userManager.GetLockoutEndDateAsync(user);
                logger.LogWarning("JWT rejected: user {UserId} locked out until {End}", userId, end);

                ctx.Fail("User locked out");
                return;
            }

            logger.LogInformation("JWT OK. User: {Name}", ctx.Principal?.Identity?.Name);
        },


        OnAuthenticationFailed = ctx =>
        {
            var logger = ctx.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("JWT");

            logger.LogError(ctx.Exception, "JWT auth failed");
            return Task.CompletedTask;
        },

        OnChallenge = ctx =>
        {
            
            var logger = ctx.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("JWT");

            logger.LogWarning("JWT challenge. Error: {Error} Desc: {Desc}", ctx.Error, ctx.ErrorDescription);
            return Task.CompletedTask;
        }
    };

});

builder.Services.AddAuthorization();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await LostUAL.Api.Seed.IdentitySeeder.SeedAsync(scope.ServiceProvider, app.Configuration);
    var db = scope.ServiceProvider.GetRequiredService<LostUALDbContext>();
    await DbSeeder.SeedAsync(db);

}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
    app.UseCors("AllowWeb");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();
}

