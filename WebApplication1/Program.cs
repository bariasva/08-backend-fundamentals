var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


var company = new List<Company>
{
    new() {Id = 1, Name= "Garbage"},
    new() {Id = 2, Name= "Kitchen"},
    new() {Id = 3, Name= "Solutions"}
};

var employees = new List<Employee>
{
    new() {Id= 1, Name="Selena Gomez", Department="HR", Company="Kitchen"},
    new() {Id= 2, Name="Ariana Grande", Department="HR", Company="Kitchen"},
    new() {Id= 3, Name="Demi Lovato", Department="HR", Company="Kitchen"},
    new() {Id= 4, Name="Bridgit Mendler", Department="CEO", Company="Solutions"},
    new() {Id= 5, Name="Sohara Hinata", Department="IT", Company="Solutions"},
};


// GET METHODS

// Companies
app.MapGet("/company", () =>
{
    if (company != null)
    {
        return Results.Ok(company);
    }
    else
    {
        return Results.NoContent();
    }

});

// Employees
app.MapGet("/employee", () =>
{
    if (employees != null)
    {
        return Results.Ok(employees);
    }
    else
    {
        return Results.NoContent();
    }
});

// Employee by company
app.MapGet("/employee/{company}", (string company) =>
{
    var companyEmployee = employees.FindAll(i => i.Company.ToLower() == company.ToLower());

    if (companyEmployee.Count > 0)
    {
        return Results.Ok(companyEmployee);
    }
    else
    {
        return Results.NoContent();
    }

});

// POST METHODS


// New company
app.MapPost("/company", (Company newCompany) =>
{
    newCompany.Id = company.Count + 1;
    company.Add(new() { Id = newCompany.Id, Name = newCompany.Name });

    return Results.Created($"/company/{newCompany.Id}", newCompany);
});

app.MapPost("/employee", (Employee newEmployee) =>
{
    newEmployee.Id = employees.Count + 1;
    company.Add(new() { Id = newEmployee.Id, Name = newEmployee.Name });

    return Results.Created($"/company/{newEmployee.Id}", newEmployee);
});

/*
// Edit an existing product with PUT
app.MapPut("/product/{id}", (int id, Producto newProduct) =>
{
    id--;
    // Check if the id is within the valid range
    if (id < 0 || id > productos.Count)
    {
        return Results.NotFound($"Product with ID {id} not found.");
    }

    // Update the product with the new values
    var existingProduct = productos[id];
    existingProduct.Name = newProduct.Name;
    existingProduct.Price = newProduct.Price;

    // Return 200 OK with the updated product
    return Results.Ok(existingProduct);
});

app.MapDelete("/product/{id}", (int id) =>
{
    id--;
    productos.Remove(productos[id]);
    return Results.Ok($"Element ID #{id++} deleted");
});
*/
app.Run();

class Company
{
    public required int Id { get; set; }
    public required string Name { get; set; }
}

class Employee
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Department { get; set; }
    public required string Company { get; set; }
}