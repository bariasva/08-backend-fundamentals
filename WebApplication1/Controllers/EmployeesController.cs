using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public EmployeesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeReadDto>>> GetEmployees()
        {
            var employees = await _dbContext.Employees
                .Include(e => e.Company)
                .Select(e => new EmployeeReadDto
                {
                    Id = e.id,
                    Name = e.name,
                    Salary = e.salary,
                    CompanyId = e.companyId,
                    CompanyName = e.Company.name
                })
                .ToListAsync();

            return Ok(employees);
        }

        // GET: api/employees/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeReadDto>> GetEmployee(int id)
        {
            var employee = await _dbContext.Employees
                .Include(e => e.Company)
                .Where(e => e.id == id)
                .Select(e => new EmployeeReadDto
                {
                    Id = e.id,
                    Name = e.name,
                    Salary = e.salary,
                    CompanyId = e.companyId,
                    CompanyName = e.Company.name
                })
                .FirstOrDefaultAsync();

            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            return Ok(employee);
        }

        // POST: api/employees
        [HttpPost]
        public async Task<ActionResult<EmployeeReadDto>> CreateEmployee([FromBody] EmployeeCreateDto employeeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var employee = new Employee
            {
                name = employeeDto.Name,
                salary = employeeDto.Salary,
                companyId = employeeDto.CompanyId
            };

            _dbContext.Employees.Add(employee);
            await _dbContext.SaveChangesAsync();

            var employeeReadDto = new EmployeeReadDto
            {
                Id = employee.id,
                Name = employee.name,
                Salary = employee.salary,
                CompanyId = employee.companyId,
                CompanyName = (await _dbContext.Companies.FindAsync(employee.companyId))?.name
            };

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.id }, employeeReadDto);
        }

        // PUT: api/employees/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeUpdateDto employeeDto)
        {
            var employee = await _dbContext.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            employee.name = employeeDto.Name;
            employee.salary = employeeDto.Salary;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
                {
                    return NotFound("Employee not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/employees/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _dbContext.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            _dbContext.Employees.Remove(employee);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeeExists(int id)
        {
            return _dbContext.Employees.Any(e => e.id == id);
        }
    }

}
