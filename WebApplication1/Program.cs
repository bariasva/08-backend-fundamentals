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
    new() {Id= 1, Name="Selena Gomez", Department="HR", CompanyID=2},
    new() {Id= 2, Name="Ariana Grande", Department="HR", CompanyID=2},
    new() {Id= 3, Name="Demi Lovato", Department="HR", CompanyID=2},
    new() {Id= 4, Name="Bridgit Mendler", Department="CEO", CompanyID=3},
    new() {Id= 5, Name="Sohara Hinata", Department="IT", CompanyID=3},
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
app.MapGet("/employee/{id}", (int id) =>
{
    var companyEmployee = employees.FindAll(i => i.CompanyID == id);

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

// New Employees
app.MapPost("/employee", (Employee newEmployee) =>
{
    newEmployee.Id = employees.Count + 1;
    employees.Add(new() { Id = newEmployee.Id, Name = newEmployee.Name, Department = newEmployee.Department, CompanyID = newEmployee.CompanyID });

    return Results.Created($"/company/{newEmployee.Id}", newEmployee);
});

// PUT Methods

// Edit an existing company
app.MapPut("/company/{id}", (int id, Company updatedCompany) =>
{
    var companyId = company.FindIndex(c => c.Id == id);

    // Update the product with the new values
    if (companyId >= 0)
    {
        company[companyId].Name = updatedCompany.Name;
        // Return 200 OK with the updated product
        return Results.Ok(updatedCompany);
    }
    else
    {
        return Results.NotFound("Company ID not Valid");
    }
});

// Edit an existing employee
app.MapPut("/employee/{id}", (int id, Employee updatedEmployee) =>
{
    var employeeId = employees.FindIndex(c => c.Id == id);

    // Update the product with the new values
    if (employeeId >= 0)
    {
        employees[employeeId].Name = updatedEmployee.Name;
        employees[employeeId].Department = updatedEmployee.Department;
        employees[employeeId].CompanyID = updatedEmployee.CompanyID;

        // Return 200 OK with the updated product
        return Results.Ok(updatedEmployee);
    }
    else
    {
        return Results.NotFound("Company ID not Valid");
    }
});

// DELETE METHODS

// Delete employee
app.MapDelete("/employee/{id}", (int id) =>
{
    var employeeId = employees.FindIndex(c => c.Id == id);

    var deletedEmployee = employees.FirstOrDefault(e => e.Id == id);

    if (employeeId >= 0)
    {
        employees.Remove(employees[employeeId]);

        // Return 200 OK with the updated product
        return Results.Ok($"Deleted Employee with ID #{id} \n {deletedEmployee}");
    }
    else
    {
        return Results.NotFound("Company ID not Valid");
    }
});

// Delete company
app.MapDelete("/company/{id}", (int id) =>
{
    var companyId = company.FindIndex(c => c.Id == id);
    var hasEmployee = employees.Any(e => e.CompanyID == id);
    if (hasEmployee)
    {
        return Results.Conflict("Please delete employees before deleting the company");
    }

    var deletedCompany = company.FirstOrDefault(e => e.Id == id);

    if (companyId >= 0)
    {
        company.Remove(company[companyId]);

        return Results.Ok($"Deleted Company with ID #{id} \n {deletedCompany}");
    }
    else
    {
        return Results.NotFound("Company ID not Valid");
    }
});

// Delete company with employees
app.MapDelete("/company/all/{id}", (int id) =>
{
    var companyId = company.FindIndex(c => c.Id == id);
    var hasEmployee = employees.Any(e => e.CompanyID == id);
    if (hasEmployee)
    {
        employees.RemoveAll(e => e.CompanyID == id);
    }

    var deletedCompany = company.FirstOrDefault(e => e.Id == id);

    if (companyId >= 0)
    {
        company.Remove(company[companyId]);

        return Results.Ok($"Deleted Company with ID #{id} \n {deletedCompany}");
    }
    else
    {
        return Results.NotFound("Company ID not Valid");
    }
});

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
    public required int CompanyID { get; set; }
}