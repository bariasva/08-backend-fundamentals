using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Controllers;
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public CompanyController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // CREATE: Add a new company
    [HttpPost]
    public async Task<IActionResult> CreateCompany([FromBody] CompanyCreateDto companyDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var company = new Company { name = companyDto.Name };
        _dbContext.Companies.Add(company);
        await _dbContext.SaveChangesAsync();

        var companyReadDto = new CompanyReadDto { Id = company.id, Name = company.name };
        return CreatedAtAction(nameof(GetCompanyById), new { id = company.id }, companyReadDto);
    }

    // READ: Get all companies
    [HttpGet]
    public async Task<IActionResult> GetAllCompanies()
    {
        var companies = await _dbContext.Companies
            .Include(c => c.Employees)
            .Include(c => c.Articles)
            .Select(c => new CompanyReadDto
            {
                Id = c.id,
                Name = c.name,
                Employees = c.Employees.Select(e => new EmployeeDto { Id = e.id, Name = e.name, Salary = e.salary }).ToList(),
                Articles = c.Articles.Select(a => new ArticleDto { Id = a.id, Name = a.name, Value = a.value }).ToList()
            })
            .ToListAsync();

        return Ok(companies);
    }

    // READ: Get a single company by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCompanyById(int id)
    {
        var company = await _dbContext.Companies
            .Include(c => c.Employees)
            .Include(c => c.Articles)
            .Where(c => c.id == id)
            .Select(c => new CompanyReadDto
            {
                Id = c.id,
                Name = c.name,
                Employees = c.Employees.Select(e => new EmployeeDto { Id = e.id, Name = e.name, Salary = e.salary }).ToList(),
                Articles = c.Articles.Select(a => new ArticleDto { Id = a.id, Name = a.name, Value = a.value }).ToList()
            })
            .FirstOrDefaultAsync();

        return company != null ? Ok(company) : NotFound();
    }

    // UPDATE: Update an existing company
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCompany(int id, [FromBody] CompanyUpdateDto companyDto)
    {
        var company = await _dbContext.Companies.FindAsync(id);
        if (company == null)
        {
            return NotFound();
        }

        company.name = companyDto.Name;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: Delete a company
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var company = await _dbContext.Companies.FindAsync(id);
        if (company == null)
        {
            return NotFound();
        }

        _dbContext.Companies.Remove(company);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
