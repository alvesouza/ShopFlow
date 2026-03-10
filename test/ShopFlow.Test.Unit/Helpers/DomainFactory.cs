using ShopFlow.Domain.Entities;
namespace ShopFlow.Tests.Unit.Helper;
public static class DomainFactory
{
    public static Product CreateProduct(
        string name         = "Widget",
        decimal price       = 10m,
        int initialStock    = 100
    ) => new Product(name, price, initialStock);

    public static OrderItem CreateOrderItem(
        string? productId   = null,
        string name         = "Widget",
        decimal price       = 10m,
        int quantity        = 1
    ) => new OrderItem( productId ?? Guid.NewGuid().ToString(), name, price, quantity );

    public static Order CreateOrder(
        string customerId   = "cust-1",
        int itemCount       = 1
    )
    {
        var items = Enumerable
            .Range( 1, itemCount )
            .Select( i => CreateOrderItem() )
            .ToList();

        return new Order(customerId, items);
    }
}