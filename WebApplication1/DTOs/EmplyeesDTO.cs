public class EmployeeCreateDto
{
    public required string Name { get; set; }
    public decimal Salary { get; set; }
    public int CompanyId { get; set; }
}

public class EmployeeUpdateDto
{
    public required string Name { get; set; }
    public decimal Salary { get; set; }
}

public class EmployeeReadDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Salary { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
}
