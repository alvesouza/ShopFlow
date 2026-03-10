namespace ShopFlow.Tests.Unit.Application;

using FluentAssertions;
using ShopFlow.Application.Services;
using ShopFlow.Domain.Entities;
using ShopFlow.Domain.Exceptions;

public class PromoEngineTests
{
    public PromoEngine Sut()
    {
        var promo = new PromoEngine();
        promo.SetupDefaultPromos();
        return promo;
    }

    [Theory]
    [InlineData("SAVE10", 100, 90)]
    [InlineData("SAVE20", 100, 80)]
    [InlineData("HALFOFF", 100, 50)]
    [InlineData(null, 50, 50)]
    public void Apply_KnownPromoCode_REturnsDiscountedTOtal(
        string? code, decimal input, decimal expected
    )
    {
        var promo = Sut();
        var total = promo.CalculatePromo(input, code );

        total.Should().Be( expected );
    }

    [Theory]
    [InlineData("SAVE10", 100, 0.8, 80)]
    [InlineData("SAVE20", 100, 0.65, 65)]
    [InlineData("HALFOFF", 100, 0.2, 20)]
    [InlineData("test01", 50, 0.7, 35)]
    public void Apply_NewSamePromoCode_ReturnsDiscountedTOtal(
        string code, decimal input, decimal newPromo, decimal expected
    )
    {
        var promo = Sut();
        promo.NewPromo(code, ( decimal x )  => x * newPromo);
        var total = promo.CalculatePromo(input, code );

        total.Should().Be( expected );
    }

    [Fact]
    public void Apply_Calculate_Exception()
    {
        var promo = Sut();
        var input = 70;
        var code = "testError";
        var act = () => promo.CalculatePromo(input, code );

        act.Should().Throw<NotFoundException>();
    }   
    [Fact]
    public void Apply_CalculateFlat_Promo()
    {
        var promo = Sut();
        var input = 70;
        var code = "testError";
        var expected = 20;
        promo.NewPromo( code, ( decimal x )  => x - 50 );
        var total = promo.CalculatePromo(input, code );

        total.Should().Be( expected );
    }    
}