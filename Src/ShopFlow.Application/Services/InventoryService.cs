using System;
using System.Collections.Generic;
using System.Linq;
using ShopFlow.Domain.Entities;
using ShopFlow.Domain.Exceptions;
using ShopFlow.Domain.Const;
using ShopFlow.Application.Interfaces;
public class InventoryService
{
    // completely separate list from the one in OrderService
    // stock changes here are invisible to OrderService and vice versa
    private IProductRepository _productRepo;
    private readonly IEmailSender _emailSender;

    public InventoryService(
        IProductRepository productRepo,
        IEmailSender emailSender
    )
    {
        _productRepo = productRepo;
        _emailSender = emailSender;
    }

    public async Task AddProductAsync( string name,
                    decimal price, int initialStock,
                        CancellationToken ct = default )
    {
        var product = new Product( name, price, initialStock );
        await _productRepo.AddAsync( product, ct );
        await _productRepo.SaveChangesAsync(ct);
    }
    public async Task AdjustStock(Guid productId, int delta,
        CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdAsync(
            productId, ct
        ) ?? 
            throw new NotFoundException( 
                ExceptionConsts.INVALID_PRODUCT,
                productId.ToString()
            );
        if( delta > 0 )
            product.Restock(delta);
        else if(delta < 0)
            product.Reserve(-delta);

        await _productRepo.SaveChangesAsync(ct);
    }
    public async Task<List<Product>> GetLowStockProducts(int threshold,
        CancellationToken ct = default)
    {
        // queries AND sends an email -- SRP violation
        /*
        // send email directly
        // string emailTo, string subject, string body
        _emailSender.SendEmail(
            customerId + "@customers.shopflow.io",
            "Your order " + order.Id,
            "Thanks! Total: " + total +
                ". Items: " + items.Count
        );
        */
        var all = await _productRepo.GetAllAsync(ct);
        var low = all.Where(p => p.Stock < threshold).ToList();
        if (low.Count > 0)
        {
            _emailSender.SendEmail(
                "warehouse@shopflow.io",
                "Low stock alert",
                low.Count + " products low."
            );
        }
        return low;
    }
    public async Task DeactivateProduct(
        Guid productId, CancellationToken ct = default )
    {
        var product = await _productRepo.GetByIdAsync(productId, ct)
            ?? 
            throw new NotFoundException(
                ExceptionConsts.INVALID_PRODUCT,
                productId.ToString()
            );
        product.Deactivate();

        await _productRepo.SaveChangesAsync(ct);
    }
}