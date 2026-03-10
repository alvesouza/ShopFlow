namespace ShopFlow.Tests.Unit.Application;

using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;
using ShopFlow.Domain.Enums;
using Moq;
using ShopFlow.Application.Interfaces;
using ShopFlow.Application.Services;
using ShopFlow.Domain.Const;
using ShopFlow.Domain.Entities;
using ShopFlow.Domain.Exceptions;
using ShopFlow.Tests.Unit.Helper;
using Xunit.Sdk;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IEmailSender> _emailMock = new();
    private readonly Mock<IPromoEngine> _promoMock = new();

    private OrderService CreateSut() => new(
        _orderRepoMock.Object,
        _productRepoMock.Object,
        _emailMock.Object,
        _promoMock.Object
    );
    private void SetupProduct(Product product)
    {
        _productRepoMock
            .Setup(r => r.GetByIdAsync(product.Id, default))
            .ReturnsAsync(product);
    }

    private List<(Guid, int)> OneItem(Guid productId, int qty = 1)
        => new() { (productId, qty) };

    [Fact]
    public async Task PlaceOrderAsync_ValidRequest_ReturnsOrderId()
    {
        // Arrange
        var product = DomainFactory.CreateProduct(initialStock: 10);
        SetupProduct(product);
        _promoMock.Setup(p => p.CalculatePromo(It.IsAny<decimal>(), null))
                  .Returns((decimal t, string? _) => t);

        var sut = CreateSut();

        // Act
        var id = await sut.PlaceOrderAsync("cust-1", OneItem(product.Id), null);

        // Assert
        id.Should().NotBeEmpty();
        _orderRepoMock.Verify(r => r.AddAsync(It.IsAny<Order>(), default), Times.Once);
        _orderRepoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task PlaceOrderAsync_EmptyItems_ThrowsDomainException()
    {
        var sut = CreateSut();

        var act = () => sut.PlaceOrderAsync("cust-1", new(), null);

        await act.Should().ThrowAsync<DomainException>().
            Where( e => e.Code == ExceptionConsts.EMPTY_ORDER );
    }

    [Fact]
    public async Task PlaceOrderAsync_EmptyCustomer_THrowsDomainException()
    {
        var sut = CreateSut();

        var act = () => sut.PlaceOrderAsync((string?)null, new(), null );

        await act.Should().ThrowAsync<DomainException>().
            Where( e => e.Code == ExceptionConsts.INVALID_CUSTOMER );
    }

    [Fact]
    public async Task PlaceOrderAsync_ProductNotFound_ThrowsDomainException()
    {
        _productRepoMock
            .Setup( r => r.GetByIdAsync( It.IsAny<Guid>(), default ) )
            .ReturnsAsync((Product?)null);

        var sut = CreateSut();

        var act = () => sut.PlaceOrderAsync( 
            "cust-1", OneItem(Guid.NewGuid()), null );

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task PlaceOrderAsync_InsufficientStock_ThrowsDomainException()
    {
        var product = DomainFactory.CreateProduct(initialStock: 1);
        SetupProduct(product);

        var sut = CreateSut();

        var act = () => sut.PlaceOrderAsync(
            "cust-1", OneItem(product.Id, qty: 99), null);

        await act.Should().ThrowAsync<DomainException>()
                 .Where(e => e.Code == "INSUFFICIENT_STOCK");
    }

    [Fact]
    public async Task PlaceOrderAsync_WithSave10Promo_AppliesDiscount()
    {
        var product = DomainFactory.CreateProduct(price: 100m, initialStock: 10);
        SetupProduct(product);

        // promo engine returns discounted value
        _promoMock.Setup(p => p.CalculatePromo(100m, "SAVE10")).Returns(90m);

        var sut = CreateSut();

        var id = await sut.PlaceOrderAsync(
            "cust-1", OneItem(product.Id), "SAVE10");

        id.Should().NotBeEmpty();
        _promoMock.Verify(p => p.CalculatePromo(100m, "SAVE10"), Times.Once);
    }

    [Fact]
    public async Task PlaceOrderAsync_EmailFails_OrderIsStillSaved()
    {
        var product = DomainFactory.CreateProduct(initialStock: 10);
        SetupProduct(product);
        _promoMock.Setup(p => p.CalculatePromo(It.IsAny<decimal>(), null))
                  .Returns((decimal t, string? _) => t);

        // email throws
        _emailMock
            .Setup(e => e.SendEmail(It.IsAny<string>(), It.IsAny<string>(),
                                    It.IsAny<string>() ) )
            .Throws(new Exception("SMTP down"));

        var sut = CreateSut();

        var act = () => sut.PlaceOrderAsync("cust-1", OneItem(product.Id), null);

        // should NOT throw -- email failure is best-effort
        await act.Should().NotThrowAsync();

        // order was still saved
        _orderRepoMock.Verify(r => r.AddAsync(It.IsAny<Order>(), default), Times.Once);
    }

    // ── CancelOrderAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task CancelOrderAsync_PendingOrder_Succeeds()
    {
        var order = DomainFactory.CreateOrder();
        _orderRepoMock
            .Setup(r => r.GetByIdAsync(order.Id, default))
            .ReturnsAsync(order);

        var product = DomainFactory.CreateProduct(initialStock: 10);
        SetupProduct(product);

        var sut = CreateSut();

        await sut.CancelOrderAsync(order.Id);

        order.Status.Should().Be(OrderStatus.Cancelled);
        _orderRepoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_OrderNotFound_ThrowsNotFoundException()
    {
        _orderRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Order?)null);

        var sut = CreateSut();

        var act = () => sut.CancelOrderAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CancelOrderAsync_ShippedOrder_ThrowsDomainException()
    {
        var order = DomainFactory.CreateOrder();
        order.Ship();

        _orderRepoMock
            .Setup(r => r.GetByIdAsync(order.Id, default))
            .ReturnsAsync(order);

        var sut = CreateSut();

        var act = () => sut.CancelOrderAsync(order.Id);

        await act.Should().ThrowAsync<DomainException>()
                 .Where(e => e.Code == "ORDER_ALREADY_SHIPPED");
    }
}