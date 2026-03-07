using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
public class OrderService
{
    private List<Order> _orders = new List<Order>();
    private List<Product> _products = new List<Product>();
    private string _smtpHost = "smtp.shopflow.io";
    private int _smtpPort = 587;
    private string _emailFrom = "orders@shopflow.io";
    public string PlaceOrder(string customerId,
        List<OrderItem> items,  string promoCode)
    {
        if (customerId == null || customerId == "")
            throw new Exception("Bad customer");
        if (items == null || items.Count == 0)
            throw new Exception("No items");
        // check stock inline
        foreach (var item in items)
        {
            var product = _products.FirstOrDefault(
            p => p.Id == item.ProductId);
            if (product == null)
                throw new Exception("Product not found: " + item.ProductId);
            if (product.Stock < item.Quantity)
                throw new Exception("Not enough stock for " + item.Name);
        }
        // calculate total
        decimal total = 0;
        foreach (var item in items)
            total += item.Price * item.Quantity;
        // apply promo
        if (promoCode == "SAVE10")
            total = total - (total * 0.10m);
        else if (promoCode == "SAVE20")
            total = total - (total * 0.20m);
        else if (promoCode == "HALFOFF")
            total = total / 2;
        // TODO: add more promo codes when marketing sends them
        // reserve stock inline
        foreach (var item in items)
        {
            var product = _products.First(p => p.Id == item.ProductId);
            product.Reserve(item.Quantity);
        }
        // create order
        var order = new Order();
        order.Id = Guid.NewGuid().ToString();
        order.CustomerId = customerId;
        order.Items = items;
        order.Total = total;
        order.Status = "pending";
        order.CreatedAt = DateTime.Now;
        _orders.Add(order);
        // send email directly
        try
        {
            var client = new SmtpClient(_smtpHost, _smtpPort);
            var msg = new MailMessage();
            msg.From = new MailAddress(_emailFrom);
            msg.To.Add(customerId + "@customers.shopflow.io");
            msg.Subject = "Your order " + order.Id;
            msg.Body = "Thanks! Total: " + total +
            ". Items: " + items.Count;
            client.Send(msg);
        }
        catch (Exception e)
        {
            Console.WriteLine("Email failed: " + e.Message);
        }
        Console.WriteLine("[" + DateTime.Now + "] Order placed: "
        + order.Id + " customer=" + customerId + " total=" + total);
        return order.Id;
    }
    public Order? GetOrder(string orderId)
    {
        foreach (var o in _orders)
            if (o.Id == orderId) return o;
        return null;
    }
    public bool CancelOrder(string orderId)
    {
        foreach (var o in _orders)
        {
            if (o.Id == orderId)
            {
                if (o.Status == "shipped")
                    return false; // cannot cancel
                o.Status = "cancelled";
                // refund stock inline
                foreach (var item in o.Items)
                {
                    var product = _products.FirstOrDefault(
                    p => p.Id == item.ProductId);
                    if (product != null)
                        product.Restock(item.Quantity);
                }
                // copy-paste email block
                try
                {
                    var client = new SmtpClient(_smtpHost, _smtpPort);
                    var msg = new MailMessage();
                    msg.From = new MailAddress(_emailFrom);
                    msg.To.Add(o.CustomerId + "@customers.shopflow.io");
                    msg.Subject = "Order " + orderId + " cancelled";
                    msg.Body = "Your order has been cancelled.";
                    client.Send(msg);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Email failed: " + e.Message);
                }
                return true;
            }
        }
        return false; // not found -- same return value as "cannot cancel"
    }
    public List<Order> GetOrdersByCustomer(string customerId)
    {
        var result = new List<Order>();
        foreach (var o in _orders)
            if (o.CustomerId == customerId)
                result.Add(o);
        return result;
    }
    public decimal GetRevenueReport()
    {
        decimal revenue = 0;
        foreach (var o in _orders)
            if (o.Status != "cancelled")
                revenue += o.Total;
        return revenue;
    }
}