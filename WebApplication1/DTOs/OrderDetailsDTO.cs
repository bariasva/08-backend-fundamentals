public class OrderDetailCreateDto
{
    public int OrderId { get; set; }
    public int ArticleId { get; set; }
    public int Quantity { get; set; }
}

public class OrderDetailReadDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ArticleId { get; set; }
    public int Quantity { get; set; }
}
