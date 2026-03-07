using System;
using System.Collections.Generic;
using System.Linq;
public class InventoryService
{
    // completely separate list from the one in OrderService
    // stock changes here are invisible to OrderService and vice versa
    private List<Product> _products = new List<Product>();
    public void AddProduct(string id, string name,
                    decimal price, int initialStock)
    {
        if (string.IsNullOrEmpty(id))
            throw new Exception("Id required");
        if (price <= 0)
            throw new Exception("Bad price");
        _products.Add(new Product
        {
            Id = id,
            Name = name,
            Price = price,
            Stock = initialStock,
            IsActive = true
        });
    }
    public void AdjustStock(string productId, int delta)
    {
        var product = _products.FirstOrDefault(p => p.Id == productId);
        if (product == null)
            throw new Exception("Not found");
        product.Stock += delta; // no guard: stock can go negative
    }
    public List<Product> GetLowStockProducts(int threshold)
    {
        // queries AND sends an email -- SRP violation
        var low = _products.Where(p => p.Stock < threshold).ToList();
        if (low.Count > 0)
        {
            try
            {
                var client = new System.Net.Mail.SmtpClient(
                    "smtp.shopflow.io", 587);
                var msg = new System.Net.Mail.MailMessage();
                msg.From = new System.Net.Mail.MailAddress(
                    "alerts@shopflow.io");
                msg.To.Add("warehouse@shopflow.io");
                msg.Subject = "Low stock alert";
                msg.Body = low.Count + " products low.";
                client.Send(msg);
            }
            catch { } // silently swallow all errors
        }
        return low;
    }
    public bool DeactivateProduct(string productId)
    {
        var product = _products.FirstOrDefault(p => p.Id == productId);
        if (product == null) return false; // same false-for-two-reasons bug
        product.IsActive = false;
        return true;
    }
}