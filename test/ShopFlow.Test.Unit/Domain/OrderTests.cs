namespace ShopFlow.Tests.Unit.Domain;

using FluentAssertions;
using ShopFlow.Domain.Const;
using ShopFlow.Domain.Entities;
using ShopFlow.Domain.Exceptions;
using ShopFlow.Tests.Unit.Helper;
using ShopFlow.Domain.Enums;

public class OrderTests
{
    [Fact]
    public void Ship()
    {
        var order = DomainFactory.CreateOrder();

        order.Ship();
        order.Status.Should().Be( OrderStatus.Shipped );
    }

    [Fact]
    public void DoubleShip()
    {
        var order = DomainFactory.CreateOrder();
        order.Ship();

        var act = () => order.Ship();

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be(ExceptionConsts.INVALID_STATUS_TRANSITION);
    }

    [Fact]
    public void Constructor_ValidData_CreatesOrderWithPendingStatus()
    {
        var order = DomainFactory.CreateOrder();
        order.Status.Should().Be(OrderStatus.Pending);
        order.Id.Should().NotBeEmpty();
        order.CreatedAt.Should().BeCloseTo(
            DateTimeOffset.UtcNow,
            TimeSpan.FromSeconds(5)
        );
    }
    [Fact]
    public void Constructor_EmptyCustomerId_ThrowsDomainException()
    {
        var act = ()=>DomainFactory.CreateOrder(customerId:"");

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be(ExceptionConsts.INVALID_CUSTOMER);
    }

}

public class OrderItemTest
{
    
    [Fact]
    public void Constructor_EmptyProductId_ThrowsDomainException()
    {
        var act = () => DomainFactory.CreateOrderItem(productId: "");

        act.Should().Throw<DomainException>().
            Which.Code.Should().Be(ExceptionConsts.INVALID_PRODUCT);
    }
    [Fact]
    public void Contructor_NegativePrice_ThrowsDomainException()
    {
        var act = () => DomainFactory.CreateOrderItem(price: -1);

        act.Should().Throw<DomainException>().
            Which.Code.Should().Be(ExceptionConsts.INVALID_PRICE);
        
    }
    [Fact]
    public void Contructor_ZeroPrice_ThrowsDomainException()
    {
        var act = () => DomainFactory.CreateOrderItem(price: 0);

        act.Should().Throw<DomainException>().
            Which.Code.Should().Be(ExceptionConsts.INVALID_PRICE);
        
    }
    [Fact]
    public void Contructor_NegativeQuantity_ThrowsDomainException()
    {
        var act = () => DomainFactory.CreateOrderItem(quantity: -1);

        act.Should().Throw<DomainException>().
            Which.Code.Should().Be(ExceptionConsts.INVALID_QUANTITY);
        
    }
    [Fact]
    public void Contructor_ZeroQuantity_ThrowsDomainException()
    {
        var act = () => DomainFactory.CreateOrderItem(quantity: 0);

        act.Should().Throw<DomainException>().
            Which.Code.Should().Be(ExceptionConsts.INVALID_QUANTITY);
        
    }
}