public class CompanyCreateDto
{
    public required string Name { get; set; }
}

public class CompanyUpdateDto
{
    public required string Name { get; set; }
}

public class CompanyReadDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<EmployeeDto> Employees { get; set; } = new();
    public List<ArticleDto> Articles { get; set; } = new();
}

public class EmployeeDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Salary { get; set; }
}

public class ArticleDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Value { get; set; }
}
