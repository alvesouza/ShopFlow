namespace ShopFlow.Tests.Unit.Domain;

using FluentAssertions;
using ShopFlow.Domain.Const;
using ShopFlow.Domain.Exceptions;
using ShopFlow.Tests.Unit.Helper;

public class ProductTests
{
    [Fact]
    public void Reserve_SufficientStock_DecreasesStock()
    {
        var product = DomainFactory.CreateProduct(initialStock: 10);

        product.Reserve(3);
        product.Stock.Should().Be(7);
    }

    [Fact]
    public void Reserve_InsufficientStock_ThrowsDomain()
    {
        var product = DomainFactory.CreateProduct(initialStock: 10);

        var act = () => product.Reserve(12);

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be(ExceptionConsts.INSUFFICIENT_STOCK);
    }

    [Fact]
    public void Reserve_ZeroQuantity_ThrowsDomainException()
    {
        var product = DomainFactory.CreateProduct();

        var act = () => product.Reserve(0);

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be(ExceptionConsts.INVALID_QUANTITY);
    }

    [Fact]
    public void Reserve_Restock()
    {
        var product = DomainFactory.CreateProduct(initialStock: 12);

        product.Restock(15);

        product.Stock.Should().Be(27);
    }

    [Fact]
    public void Reserve_Negative_Restock()
    {
        var product = DomainFactory.CreateProduct();
        var act = () => product.Restock(0);

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be(ExceptionConsts.INVALID_QUANTITY);
        act = () => product.Restock(-1);

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be(ExceptionConsts.INVALID_QUANTITY);
    }

    [Fact]
    public void Deactivate()
    {
        var product = DomainFactory.CreateProduct();

        product.Deactivate();
        product.IsActive.Should().Be(false);
    }
    [Fact]
    public void Deactivate_Deactivated()
    {
        var product = DomainFactory.CreateProduct();

        product.Deactivate();
        var act = () => product.Deactivate();
        act.Should().Throw<DomainException>().Which
            .Code.Should().Be(ExceptionConsts.ALREADY_INACTIVE);
    }
}