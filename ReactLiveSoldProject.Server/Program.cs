using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ReactLiveSoldProject.ServerBL.Base;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Database
builder.Services.AddDbContext<LiveSoldDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");

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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero // Eliminar el margen de 5 minutos por defecto
    };
});

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdmin", policy =>
        policy.RequireRole("SuperAdmin"));

    options.AddPolicy("OrgOwner", policy =>
        policy.RequireRole("Owner"));

    options.AddPolicy("Seller", policy =>
        policy.RequireRole("Seller", "Owner"));

    options.AddPolicy("Customer", policy =>
        policy.RequireRole("Customer"));

    options.AddPolicy("Employee", policy =>
        policy.RequireRole("Seller", "Owner", "SuperAdmin"));
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Services
builder.Services.AddScoped<ReactLiveSoldProject.ServerBL.Services.IAuthService, ReactLiveSoldProject.ServerBL.Services.AuthService>();
builder.Services.AddScoped<ReactLiveSoldProject.ServerBL.Services.IOrganizationService, ReactLiveSoldProject.ServerBL.Services.OrganizationService>();
builder.Services.AddScoped<ReactLiveSoldProject.ServerBL.Services.ICustomerService, ReactLiveSoldProject.ServerBL.Services.CustomerService>();
builder.Services.AddScoped<ReactLiveSoldProject.ServerBL.Services.IProductService, ReactLiveSoldProject.ServerBL.Services.ProductService>();
builder.Services.AddScoped<ReactLiveSoldProject.ServerBL.Services.IWalletService, ReactLiveSoldProject.ServerBL.Services.WalletService>();
builder.Services.AddScoped<ReactLiveSoldProject.ServerBL.Services.ISalesOrderService, ReactLiveSoldProject.ServerBL.Services.SalesOrderService>();

// Helpers
builder.Services.AddScoped<ReactLiveSoldProject.ServerBL.Helpers.JwtHelper>();

builder.Services.AddControllers();

// Swagger/OpenAPI with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LiveSold Platform API",
        Version = "v1",
        Description = "Multi-Tenant SaaS Platform for Live Sales and Inventory Management"
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token."
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

// Seed database on startup (development only)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<LiveSoldDbContext>();
        var seeder = new ReactLiveSoldProject.ServerBL.Helpers.DatabaseSeeder(dbContext);
        await seeder.SeedAsync();
    }
}

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "LiveSold Platform API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthentication(); // IMPORTANTE: Debe ir antes de UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
