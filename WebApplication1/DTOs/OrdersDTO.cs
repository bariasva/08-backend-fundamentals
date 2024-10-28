public class OrderCreateDto
{
    public required string Name { get; set; }
    public int EmployeeId { get; set; }
    public ICollection<OrderDetailCreateDto> OrderDetails { get; set; } = new List<OrderDetailCreateDto>();
}

public class OrderReadDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int EmployeeId { get; set; }
    public decimal TotalValue { get; set; }
    public string Status { get; set; }
    public ICollection<OrderDetailReadDto> OrderDetails { get; set; } = new List<OrderDetailReadDto>();
}


