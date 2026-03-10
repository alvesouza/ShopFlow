namespace ShopFlow.Domain.Exceptions;

public class NotFoundException: Exception
{
    public NotFoundException( string entityName, string id )
        : base($"{entityName} with the id '{id}' was not found.")
    {}
}