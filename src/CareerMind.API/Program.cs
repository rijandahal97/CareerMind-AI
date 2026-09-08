using CareerMind.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration);
    loggerConfiguration.WriteTo.Console();
});

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework
builder.Services.AddDbContext<CareerMindDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Settings
builder.Services.Configure<CareerMind.Application.Common.JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// JWT Authentication
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<CareerMind.Application.Common.JwtSettings>();
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings?.Issuer,
            ValidAudience = jwtSettings?.Audience,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings?.Secret ?? "CareerMind-Super-Secret-Key-Provide-In-Env"))
        };
    });

// Services
builder.Services.AddScoped<CareerMind.Application.Interfaces.ITokenService, CareerMind.Infrastructure.Identity.TokenService>();
builder.Services.AddScoped<CareerMind.Application.Interfaces.IPasswordHasher, CareerMind.Infrastructure.Identity.PasswordHasher>();
builder.Services.AddScoped<CareerMind.Application.Interfaces.IAuthService, CareerMind.Infrastructure.Identity.AuthService>();
builder.Services.AddScoped<CareerMind.Application.Interfaces.ICandidateProfileService, CareerMind.Infrastructure.Services.CandidateProfileService>();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CareerMind API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Vite and typical React ports
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Build application
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ensure database is created/migrated at startup (for dev purposes)
// Ensure database is created/migrated at startup (for dev purposes)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CareerMindDbContext>();
    // Make sure we apply migrations
    dbContext.Database.Migrate(); 
    
    // Seed Data
    try {
        CareerMind.Infrastructure.Data.Seed.CareerMindDataSeeder.SeedAsync(dbContext).Wait();
    } catch(Exception ex) {
        Console.WriteLine(ex.Message);
    }
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
