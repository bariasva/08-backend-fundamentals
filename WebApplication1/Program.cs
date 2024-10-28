using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "My API",
        Version = "v1"
    });

    c.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "My API",
        Version = "v2"
    });

    // Configuración para agregar el token JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduzca 'Bearer' [espacio] seguido de su token JWT."
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
            new string[] {}
        }
    });
});

// API versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

// Versioning through annotations
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Configure Entity Framework Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthorization();

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
            ValidIssuer = "yourdomain.com",
            ValidAudience = "yourdomain.com",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("vainitaOMGclavelargaysegura_a234243423423awda"))
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "My API V2");
});


app.MapControllers();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();


app.Run();

class UserLogin
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Users> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // You can configure relationships, keys, and constraints here if needed.
        modelBuilder.Entity<Company>().ToTable("Company"); // Specify singular table name
        modelBuilder.Entity<Employee>().ToTable("Employee");
        modelBuilder.Entity<Article>().ToTable("Article");
        modelBuilder.Entity<Order>().ToTable("Order");
        modelBuilder.Entity<OrderDetail>().ToTable("OrderDetail");
        modelBuilder.Entity<Invoice>().ToTable("Invoice");
        modelBuilder.Entity<Users>().ToTable("Users");

        modelBuilder.Entity<Users>().HasKey(u => u.Id);

        base.OnModelCreating(modelBuilder);
    }
}

public class Company
{
    public int id { get; set; }
    public required string name { get; set; }
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<Article> Articles { get; set; } = new List<Article>();
}

public class Employee
{
    public int id { get; set; }
    public required string name { get; set; }
    public decimal salary { get; set; }
    public int companyId { get; set; }
    public Company Company { get; set; }  // Navigation property
}

public class Article
{
    public int id { get; set; }
    public required string name { get; set; }
    public decimal value { get; set; }
    public int companyId { get; set; }
    public Company Company { get; set; }  // Navigation property
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

public class Order
{
    public int id { get; set; }
    public required string name { get; set; }
    public int employeeId { get; set; }
    public Employee Employee { get; set; }  // Navigation property
    public decimal totalValue { get; set; }
    public required string status { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

public class OrderDetail
{
    public int id { get; set; }
    public int articleId { get; set; }
    public Article Article { get; set; }  // Navigation property
    public int orderId { get; set; }
    public Order Order { get; set; }  // Navigation property
    public int quantity { get; set; }  // Consider adding a Quantity property
}

public class Invoice
{
    public int id { get; set; }
    public int orderId { get; set; }
    public Order Order { get; set; }  // Navigation property
    public required string status { get; set; }
    public DateTime estimatedDeliveryDate { get; set; }
}

public class Users
{
    public int Id { get; set; }  // Primary Key
    public required string Username { get; set; }
    public required string Password { get; set; }
}