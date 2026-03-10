using System.Reflection.Metadata.Ecma335;
using ShopFlow.Application.Interfaces;
using ShopFlow.Domain.Exceptions;
using ShopFlow.Domain.Const;
namespace ShopFlow.Domain.Entities;

public class PromoEngine: IPromoEngine
{
    public Dictionary<string, Func<decimal, decimal>>
        DictPromoFunc{get;set;}

    public PromoEngine()
    {
        DictPromoFunc = new Dictionary<string, Func<decimal, decimal>>();
    }
    public PromoEngine( Dictionary<string, Func<decimal, decimal>> dict )
    {
        DictPromoFunc = dict;
    }

    public void NewPromo( string promo, Func<decimal, decimal> func )
    {
        DictPromoFunc[promo] = func;
    }
    public void SetupDefaultPromos()
    {
        DictPromoFunc["SAVE10"] = (decimal total) => total * 0.9m;
        DictPromoFunc["SAVE20"] = (decimal total) => total * 0.8m;
        DictPromoFunc["HALFOFF"] = (decimal total) => total / 2;
    }

    public decimal CalculatePromo( decimal total, string? promo )
    {
        if(promo == null)
            return total;
        else if( DictPromoFunc.TryGetValue(promo, out var func ) )
        {
            return func( total );
        }
        else
            throw new NotFoundException( ExceptionConsts.INVALID_PROMO, promo );
    }
}