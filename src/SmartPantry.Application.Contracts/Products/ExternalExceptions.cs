using System;

namespace SmartPantry.Products;

public class ExternalCatalogRateLimitException : Exception
{
    public ExternalCatalogRateLimitException(string message = "Se ha superado el límite de solicitudes al catálogo externo.")
        : base(message)
    {
    }
}

public class ExternalCatalogUnavailableException : Exception
{
    public ExternalCatalogUnavailableException(string message = "El servicio de catálogo externo no se encuentra disponible.", Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
