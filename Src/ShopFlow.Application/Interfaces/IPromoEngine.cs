public interface IPromoEngine
{
    public void NewPromo( string promo, Func<decimal, decimal> func );
    public void SetupDefaultPromos();

    public decimal CalculatePromo( decimal total, string? promo );
}