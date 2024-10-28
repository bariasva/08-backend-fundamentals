public class InvoiceCreateDto
{
    public int OrderId { get; set; }
}

public class InvoiceUpdateDto
{
    public int OrderId { get; set; }
}

public class InvoiceReadDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string OrderDetails { get; set; } // Assuming you want to include order details in the response
}
