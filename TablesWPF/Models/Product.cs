namespace TablesWPF.Models;

/// <summary>
/// Product DTO for product list.
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
