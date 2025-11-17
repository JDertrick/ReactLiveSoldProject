using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

//AutoMapper
builder.Services.AddAutoMapper(c =>
{
    c.AddProfile<MappingProfile>();
});

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

    options.AddPolicy("EmployeeOnly", policy =>
        policy.RequireRole("Seller", "Owner")); // Solo empleados de organizaciones, NO SuperAdmin

    options.AddPolicy("EmployeeOrCustomer", policy =>
        policy.RequireRole("Seller", "Owner", "SuperAdmin", "Customer")); // Empleados y Customers
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            // En desarrollo, permitir cualquier localhost
            if (builder.Environment.IsDevelopment())
            {
                return origin.StartsWith("http://localhost:") || origin.StartsWith("https://localhost:");
            }
            // En producción, solo permitir orígenes específicos
            return origin == "http://localhost:5173" || origin == "https://localhost:5173";
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ISalesOrderService, SalesOrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStockMovementService, StockMovementService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ILocationService, LocationService>();

// Helpers
builder.Services.AddScoped<ReactLiveSoldProject.ServerBL.Helpers.JwtHelper>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configurar enums para que se serialicen como strings en lugar de números
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

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
