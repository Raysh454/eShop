namespace Catalog.Domain.Exceptions;

// <summary> Raised when a Catalog business rule is violated. Distinguishes a
// broken invariant from an infrastructure fault so the API can map it to 400. </summary>

public class CatalogDomainException : Exception
{
    public CatalogDomainException(string message) : base(message)
    {
    }

    public CatalogDomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
