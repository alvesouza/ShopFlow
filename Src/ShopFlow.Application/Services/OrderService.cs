using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using ShopFlow.Domain.Entities;
using ShopFlow.Domain.Enums;
using ShopFlow.Domain.Exceptions;
using ShopFlow.Domain.Const;
using ShopFlow.Application.Interfaces;
using System.Runtime.CompilerServices;

namespace ShopFlow.Application.Services;
public class OrderService
{
    private readonly IOrderRepository   _orderRepo;
    private readonly IProductRepository _productRepo;
    private readonly IEmailSender       _emailSender;
    private readonly IPromoEngine       _promoEngine;

    // dependencies injected -- never newed inside
    public OrderService(
        IOrderRepository   orderRepo,
        IProductRepository productRepo,
        IEmailSender       emailSender,
        IPromoEngine       promoEngine)
    {
        _orderRepo   = orderRepo;
        _productRepo = productRepo;
        _emailSender = emailSender;
        _promoEngine = promoEngine;
    }

    public async Task<Guid> PlaceOrderAsync(string? customerId,
        List<(Guid ProdutId, int Quantity)> lines,  string? promoCode,
            CancellationToken ct = default)
    {
        if (customerId == null || customerId == "")
            throw new DomainException( 
                ExceptionConsts.INVALID_CUSTOMER,
                "customerId must have a value"
            ); // Exception("Bad customer");
        if (lines == null || lines.Count == 0)
            throw new DomainException( 
                ExceptionConsts.EMPTY_ORDER,
                "There is nothing being ordered"
            );
        // resolve products -- repository call, not an inline list
        var items = new List<OrderItem>();
        foreach (var (productId, quantity) in lines)
        {
            var product = await _productRepo.GetByIdAsync(
                productId,
                ct
            ) ?? throw new NotFoundException(
                    ExceptionConsts.INVALID_PRODUCT,
                    productId.ToString()
                );
            
            product.Reserve(quantity);
            
            items.Add(
                new OrderItem(
                    product.Id.ToString(),
                    product.Name,
                    product.Price,
                    quantity
                )
            );
        }
        // create order
        var order = new Order(customerId, items);
        await _orderRepo.AddAsync(order, ct);
        await _orderRepo.SaveChangesAsync(ct);

        var total = _promoEngine.CalculatePromo(order.Total, promoCode );
        try
        {
            _emailSender.SendEmail(
                customerId + "@customers.shopflow.io",
                "Your order " + order.Id,
                "Thanks! Total: " + total + ". Items: " + items.Count
            );
        }
        catch { /* best-effort: email failure must not abort a placed order */ }
        Console.WriteLine("[" + DateTimeOffset.Now + "] Order placed: "
        + order.Id + " customer=" + customerId + " total=" + total);
        return order.Id;
    }
    public async Task<Order> GetOrderAsync(Guid orderId,
        CancellationToken ct = default )
    {
        return await _orderRepo.GetByIdAsync(orderId, ct)
            ?? throw new NotFoundException("Order", orderId.ToString());
    }
    public async Task CancelOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, ct)
            ?? throw new NotFoundException(
                ExceptionConsts.Order,
                $"Order {orderId} not found"
            );
        
        order.Cancel();

        foreach( var item in order.Items )
        {
            var product = await _productRepo.GetByIdAsync(
                Guid.Parse( item.ProductId ),
                ct
            );

            product?.Restock(item.Quantity);
        }

        await _orderRepo.SaveChangesAsync(ct);

        // send email directly
        // string emailTo, string subject, string body
        _emailSender.SendEmail(
            order.CustomerId + "@customers.shopflow.io",
            "Order " + orderId + " cancelled",
            "Your order has been cancelled."
        );
    }
    public async Task<IReadOnlyList<Order>> GetOrdersByCustomerAsync(
        string customerId,
        CancellationToken ct = default )
    {
        var orders = await _orderRepo.GetByCustomerAsync( customerId, ct );
        return orders;
    }
    public async Task<decimal> GetRevenueReportAsync()
    {
        return await _orderRepo.GetRevenue();
    }
}