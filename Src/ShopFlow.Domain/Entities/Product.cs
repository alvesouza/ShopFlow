public class Product
{
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public void Reserve(int quantity)
    {
        Stock -= quantity; // no guard: stock can go negative
    }
    public void Restock(int quantity)
    {
        Stock += quantity;
    }
}