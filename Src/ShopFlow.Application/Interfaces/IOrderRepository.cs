using System.Data.SqlTypes;
using ShopFlow.Domain.Entities;
namespace ShopFlow.Application.Interfaces;
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByCustomerAsync(string customerId, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);

    Task<decimal> GetRevenue();
}