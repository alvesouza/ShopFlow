using ShopFlow.Domain.Enums;
using ShopFlow.Domain.Exceptions;
using ShopFlow.Domain.Const;
public class Order
{
    public Guid Id { get; }
    public string CustomerId { get; }
    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public decimal Total => _items.Sum( i => i.Price * i.Quantity );
    public OrderStatus Status { get; private set; } // "pending" "shipped" etc
    public DateTimeOffset CreatedAt { get; }
    public Order( string _customerId )
        : this( _customerId, new List<OrderItem>(), DateTimeOffset.UtcNow)
    {
    }
    public Order( string _customerId, IEnumerable<OrderItem> items )
        : this( _customerId, items, DateTimeOffset.UtcNow )
    {        
    }
    public Order( string _customerId, IEnumerable<OrderItem> items, DateTimeOffset _createdAt )
    {
        this.Id         = Guid.NewGuid();
        if( string.IsNullOrEmpty(_customerId) )
            throw new DomainException(
                ExceptionConsts.INVALID_CUSTOMER,
                "Customer cant be empty"
            );
        this.CustomerId = _customerId;
        this.CreatedAt = _createdAt;
        this.Status     = OrderStatus.Pending;
        this._items.AddRange( items );
    }
    public void Ship()
    {
        if (this.Status != OrderStatus.Pending)
            throw new DomainException( ExceptionConsts.INVALID_STATUS_TRANSITION,
                $"Cannot ship an order with status '{Status}'.");

        this.Status = OrderStatus.Shipped;
    }
    public void Cancel()
    {
        if( this.Status == OrderStatus.Shipped )
            throw new DomainException(
                ExceptionConsts.ORDER_ALREADY_SHIPPED,
                "Cannot cancel an order that has already shipped"               
            );

        if( this.Status == OrderStatus.Cancelled )
            throw new DomainException(
                ExceptionConsts.ORDER_ALREADY_CANCELLED,
                "Order is already cancelled"
            );
        this.Status = OrderStatus.Cancelled;
    }
}
public class OrderItem
{
    public Guid Id {get; private set;}
    public Guid OrderId {get; private set;}
    public string ProductId { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }

    public OrderItem( string productId, string name,
        decimal price, int quantity )
    {
        if( string.IsNullOrEmpty(productId) )
            throw new DomainException(
                ExceptionConsts.INVALID_PRODUCT,
                "Product id cannot be empty"
            );
        if( price <= 0 )
            throw new DomainException(
                ExceptionConsts.INVALID_PRICE,
                "Price must be greater than zero"
            );
        if( quantity <= 0 )
            throw new DomainException(
                ExceptionConsts.INVALID_QUANTITY,
                "Quantity must be greater than zero"
            );
        Id  = Guid.NewGuid();
        ProductId = productId;
        Name = name;
        Price = price;
        Quantity = quantity;
    }
}