namespace ShopFlow.Domain.Entities;

using ShopFlow.Domain.Exceptions;
using ShopFlow.Domain.Const;

public class Product
{
    public Guid    Id       { get; private set; }
    public string  Name     { get; private set; }
    public decimal Price    { get; private set; }
    public int     Stock    { get; private set; }
    public bool    IsActive { get; private set; }

    // required by EF Core
    private Product(): this("empty", 0m, 0) { }

    public Product(string name, decimal price, int initialStock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(
                ExceptionConsts.INVALID_NAME, // "INVALID_NAME",
                "Product name cannot be empty.");

        if (price <= 0)
            throw new DomainException(
                ExceptionConsts.INVALID_PRICE, // "INVALID_PRICE",
                "Price must be greater than zero.");

        if (initialStock < 0)
            throw new DomainException(
                ExceptionConsts.INVALID_STOCK, // "INVALID_STOCK",
                "Initial stock cannot be negative.");

        Id       = Guid.NewGuid();
        Name     = name;
        Price    = price;
        Stock    = initialStock;
        IsActive = true;
    }

    public Product(string name)
    {
        Name = name;
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException(
                ExceptionConsts.INVALID_QUANTITY, // "INVALID_QUANTITY",
                "Reserve quantity must be positive.");

        if (Stock < quantity)
            throw new DomainException(
                ExceptionConsts.INSUFFICIENT_STOCK, // "INSUFFICIENT_STOCK",
                $"Cannot reserve {quantity} units. Only {Stock} in stock.");

        Stock -= quantity;
    }

    public void Restock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException(
                ExceptionConsts.INVALID_QUANTITY,
                "Restock quantity must be positive.");

        Stock += quantity;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException(
                ExceptionConsts.ALREADY_INACTIVE, // "ALREADY_INACTIVE",
                "Product is already inactive.");

        IsActive = false;
    }
}