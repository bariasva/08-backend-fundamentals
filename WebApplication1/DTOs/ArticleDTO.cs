public class ArticleCreateDto
{
    public required string Name { get; set; }
    public decimal Value { get; set; }
    public int CompanyId { get; set; }
}

public class ArticleUpdateDto
{
    public required string Name { get; set; }
    public decimal Value { get; set; }
}

public class ArticleReadDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Value { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
}
