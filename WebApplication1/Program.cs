using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mi API con JWT", Version = "v1" });

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseMiddleware<MidlewareTest>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();


// Función para generar el JWT
string GenerateJwtToken()
{
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, "test"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("User","Mi usuario")
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("vainitaOMGclavelargaysegura_a234243423423awda"));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: "yourdomain.com",
        audience: "yourdomain.com",
        claims: claims,
        expires: DateTime.Now.AddMinutes(30),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}


// Endpoint de login para generar el JWT
app.MapPost("/login", (UserLogin login) =>
{
    if (login.Username == "test" && login.Password == "pass") // Validar credenciales
    {
        var token = GenerateJwtToken();
        return Results.Ok(new { token });
    }
    return Results.Unauthorized();
});

//          **** COMPANY ENDPOINTS ****           //

// CREATE: Add a new company
app.MapPost("/companies", async (ApplicationDbContext dbContext, Company company) =>
{
    dbContext.Companies.Add(company);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/companies/{company.id}", company);
}).RequireAuthorization();

// READ: Get all companies
app.MapGet("/companies", async (ApplicationDbContext dbContext) =>
{
    return await dbContext.Companies.ToListAsync();
}).RequireAuthorization();

// READ: Get a single company by ID
app.MapGet("/companies/{id}", async (ApplicationDbContext dbContext, int id) =>
{
    var company = await dbContext.Companies.FindAsync(id);
    return company != null ? Results.Ok(company) : Results.NotFound();
}).RequireAuthorization();

// UPDATE: Update an existing company
app.MapPut("/companies/{id}", async (ApplicationDbContext dbContext, int id, Company updatedCompany) =>
{
    var company = await dbContext.Companies.FindAsync(id);
    if (company == null)
    {
        return Results.NotFound();
    }

    company.name = updatedCompany.name;

    await dbContext.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

// DELETE: Delete a company
app.MapDelete("/companies/{id}", async (ApplicationDbContext dbContext, int id) =>
{
    var company = await dbContext.Companies.FindAsync(id);
    if (company == null)
    {
        return Results.NotFound();
    }

    dbContext.Companies.Remove(company);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

//              **** EMPLOYEES ****              //

// CREATE: Add a new employee
app.MapPost("/employees", async (ApplicationDbContext dbContext, Employee employee) =>
{
    dbContext.Employees.Add(employee);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/employees/{employee.id}", employee);
}).RequireAuthorization();

// READ: Get all employees
app.MapGet("/employees", async (ApplicationDbContext dbContext) =>
{
    return await dbContext.Employees.ToListAsync();
}).RequireAuthorization();

// READ: Get a single employee by ID
app.MapGet("/employees/{id}", async (ApplicationDbContext dbContext, int id) =>
{
    var employee = await dbContext.Employees.FindAsync(id);
    return employee != null ? Results.Ok(employee) : Results.NotFound();
}).RequireAuthorization();

// UPDATE: Update an existing employee
app.MapPut("/employees/{id}", async (ApplicationDbContext dbContext, int id, Employee updatedEmployee) =>
{
    var employee = await dbContext.Employees.FindAsync(id);
    if (employee == null)
    {
        return Results.NotFound();
    }

    employee.name = updatedEmployee.name;
    employee.salary = updatedEmployee.salary;
    employee.companyId = updatedEmployee.companyId;

    await dbContext.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

// DELETE: Delete an employee
app.MapDelete("/employees/{id}", async (ApplicationDbContext dbContext, int id) =>
{
    var employee = await dbContext.Employees.FindAsync(id);
    if (employee == null)
    {
        return Results.NotFound();
    }

    dbContext.Employees.Remove(employee);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

//              **** ARTICLES ****              //

// CREATE: Add a new article
app.MapPost("/articles", async (ApplicationDbContext dbContext, Article article) =>
{
    dbContext.Articles.Add(article);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/articles/{article.id}", article);
}).RequireAuthorization();

// READ: Get all articles
app.MapGet("/articles", async (ApplicationDbContext dbContext) =>
{
    return await dbContext.Articles.ToListAsync();
}).RequireAuthorization();

// READ: Get a single article by ID
app.MapGet("/articles/{id}", async (ApplicationDbContext dbContext, int id) =>
{
    var article = await dbContext.Articles.FindAsync(id);
    return article != null ? Results.Ok(article) : Results.NotFound();
}).RequireAuthorization();

// UPDATE: Update an existing article
app.MapPut("/articles/{id}", async (ApplicationDbContext dbContext, int id, Article updatedArticle) =>
{
    var article = await dbContext.Articles.FindAsync(id);
    if (article == null)
    {
        return Results.NotFound();
    }

    article.name = updatedArticle.name;
    article.value = updatedArticle.value;
    article.companyId = updatedArticle.companyId;

    await dbContext.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

// DELETE: Delete an article
app.MapDelete("/articles/{id}", async (ApplicationDbContext dbContext, int id) =>
{
    var article = await dbContext.Articles.FindAsync(id);
    if (article == null)
    {
        return Results.NotFound();
    }

    dbContext.Articles.Remove(article);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

//              **** ARTICLES ****              //

// CREATE: Add a new order
app.MapPost("/orders", async (ApplicationDbContext dbContext, Order order) =>
{
    // Ensure the order is not being created with a 'complete' status
    if (order.status == "complete")
    {
        return Results.BadRequest("Order cannot be marked as 'complete' at the time of creation.");
    }

    // Validate that all articles belong to the same company as the employee
    var employee = await dbContext.Employees.FindAsync(order.employeeId);
    if (employee == null)
    {
        return Results.NotFound("Employee not found.");
    }

    foreach (var detail in order.OrderDetails)
    {
        var article = await dbContext.Articles.FindAsync(detail.articleId);
        if (article == null)
        {
            return Results.NotFound($"Article with ID {detail.articleId} not found.");
        }

        if (article.companyId != employee.companyId)
        {
            return Results.BadRequest("All articles must belong to the same company as the employee.");
        }
    }

    // Add the new order
    dbContext.Orders.Add(order);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/orders/{order.id}", order);
}).RequireAuthorization();

// READ: Get all orders
app.MapGet("/orders", async (ApplicationDbContext dbContext) =>
{
    return await dbContext.Orders.ToListAsync();
}).RequireAuthorization();

// READ: Get a single order by ID
app.MapGet("/orders/{id}", async (ApplicationDbContext dbContext, int id) =>
{
    var order = await dbContext.Orders.FindAsync(id);
    return order != null ? Results.Ok(order) : Results.NotFound();
}).RequireAuthorization();

// UPDATE: Update an existing order
app.MapPut("/orders/{id}", async (ApplicationDbContext dbContext, int id, Order updatedOrder) =>
{
    var order = await dbContext.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.id == id);
    if (order == null)
    {
        return Results.NotFound();
    }

    // Verify that all articles belong to the same company as the employee
    var employee = await dbContext.Employees.FindAsync(updatedOrder.employeeId);
    if (employee == null)
    {
        return Results.NotFound("Employee not found.");
    }

    foreach (var detail in updatedOrder.OrderDetails)
    {
        var article = await dbContext.Articles.FindAsync(detail.articleId);
        if (article == null)
        {
            return Results.NotFound($"Article with ID {detail.articleId} not found.");
        }

        if (article.companyId != employee.companyId)
        {
            return Results.BadRequest("All articles must belong to the same company as the employee.");
        }
    }

    // Prevent completing an order without any articles
    if (updatedOrder.status == "complete" && (order.OrderDetails == null || order.OrderDetails.Count == 0))
    {
        return Results.BadRequest("Cannot mark the order as complete without any associated articles.");
    }

    // Check if the status is being updated to 'complete'
    if (updatedOrder.status == "complete" && order.status != "complete")
    {
        // Create an invoice for the completed order
        var invoice = new Invoice
        {
            orderId = order.id,
            status = "pending",  // Set the default status for the new invoice
            estimatedDeliveryDate = DateTime.Now.AddDays(5) // Example delivery date
        };

        dbContext.Invoices.Add(invoice);
    }

    // Update the order properties
    order.name = updatedOrder.name;
    order.totalValue = updatedOrder.totalValue;
    order.status = updatedOrder.status;
    order.employeeId = updatedOrder.employeeId;
    order.OrderDetails = updatedOrder.OrderDetails;

    await dbContext.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

// DELETE: Delete an order
app.MapDelete("/orders/{id}", async (ApplicationDbContext dbContext, int id) =>
{
    var order = await dbContext.Orders.FindAsync(id);
    if (order == null)
    {
        return Results.NotFound();
    }

    dbContext.Orders.Remove(order);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();


//              **** INVOICES ****              //

// READ: Get all invoices
app.MapGet("/invoices", async (ApplicationDbContext dbContext) =>
{
    return await dbContext.Invoices.ToListAsync();
}).RequireAuthorization();

// READ: Get a single invoice by ID
app.MapGet("/invoices/{id}", async (ApplicationDbContext dbContext, int id) =>
{
    var invoice = await dbContext.Invoices.FindAsync(id);
    return invoice != null ? Results.Ok(invoice) : Results.NotFound();
});

// UPDATE: Update an existing invoice
app.MapPut("/invoices/{id}", async (ApplicationDbContext dbContext, int id, Invoice updatedInvoice) =>
{
    var invoice = await dbContext.Invoices.FindAsync(id);
    if (invoice == null)
    {
        return Results.NotFound();
    }

    // Update the invoice properties
    invoice.status = updatedInvoice.status;
    invoice.estimatedDeliveryDate = updatedInvoice.estimatedDeliveryDate;

    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

// DELETE: Delete an invoice
app.MapDelete("/invoices/{id}", async (ApplicationDbContext dbContext, int id) =>
{
    var invoice = await dbContext.Invoices.FindAsync(id);
    if (invoice == null)
    {
        return Results.NotFound();
    }

    dbContext.Invoices.Remove(invoice);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // You can configure relationships, keys, and constraints here if needed.
        modelBuilder.Entity<Company>().ToTable("Company"); // Specify singular table name
        modelBuilder.Entity<Employee>().ToTable("Employee");
        modelBuilder.Entity<Article>().ToTable("Article");
        modelBuilder.Entity<Order>().ToTable("Order");
        modelBuilder.Entity<OrderDetail>().ToTable("OrderDetail");
        modelBuilder.Entity<Invoice>().ToTable("Invoice");


    }
}

public class Company
{
    public int id { get; set; }
    public string name { get; set; }
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<Article> Articles { get; set; } = new List<Article>();
}

public class Employee
{
    public int id { get; set; }
    public string name { get; set; }
    public decimal salary { get; set; }
    public int companyId { get; set; }
}

public class Article
{
    public int id { get; set; }
    public string name { get; set; }
    public decimal value { get; set; }
    public int companyId { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

public class Order
{
    public int id { get; set; }
    public string name { get; set; }
    public int employeeId { get; set; }
    public decimal totalValue { get; set; }
    public string status { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

public class OrderDetail
{
    public int id { get; set; }
    public int articleId { get; set; }
    public int orderId { get; set; }
    public int quantity { get; set; }  // Consider adding a Quantity property
}

public class Invoice
{
    public int id { get; set; }
    public int orderId { get; set; }
    public string status { get; set; }
    public DateTime estimatedDeliveryDate { get; set; }
}