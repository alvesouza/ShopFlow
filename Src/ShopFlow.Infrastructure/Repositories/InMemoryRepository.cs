using ShopFlow.Domain.Entities;
using ShopFlow.Application.Interfaces;
using ShopFlow.Domain.Enums;
namespace ShopFlow.Infrastructure.Repositories;
public class InMemoryProductRepository: IProductRepository
{
    // shared across both OrderService and InventoryService
    // this fixes the two-separate-lists bug from the original code
    private readonly List<Product> _products = new();

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Product> result = _products.ToList();
        return Task.FromResult(result);
    }

    public Task AddAsync(Product product, CancellationToken ct = default)
    {
        _products.Add(product);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}

public class InMemoryOrderRepository: IOrderRepository
{
    // the single shared list -- no more duplicate state
    private readonly List<Order> _orders = new();

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        return Task.FromResult(order);
    }

    public Task<IReadOnlyList<Order>> GetByCustomerAsync(
        string customerId, CancellationToken ct = default)
    {
        IReadOnlyList<Order> result = _orders
            .Where(o => o.CustomerId == customerId)
            .ToList();
        return Task.FromResult(result);
    }

    public Task AddAsync(Order order, CancellationToken ct = default)
    {
        _orders.Add(order);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        // no-op for in-memory -- nothing to flush
        return Task.CompletedTask;
    }

    public async Task<decimal> GetRevenue()
    {
        var total = _orders
            .Where( o => o.Status != OrderStatus.Cancelled )
            .Sum( o=>o.Total );

        return total;
    }
}