using System;

namespace SmartPantry.Products;

public enum ExternalProductLookupStatus
{
    Found = 1,
    NotFound = 2,
    RateLimitExceeded = 3,
    ServiceUnavailable = 4
}

public class ExternalProductResultDto
{
    public ExternalProductLookupStatus Status { get; set; }
    public ExternalProductDto? Product { get; set; }
    public string? Message { get; set; }

    public static ExternalProductResultDto Found(ExternalProductDto product)
    {
        return new ExternalProductResultDto
        {
            Status = ExternalProductLookupStatus.Found,
            Product = product
        };
    }

    public static ExternalProductResultDto NotFound(string message = "Producto no encontrado en el catálogo externo.")
    {
        return new ExternalProductResultDto
        {
            Status = ExternalProductLookupStatus.NotFound,
            Message = message
        };
    }

    public static ExternalProductResultDto RateLimit(string message = "Se ha superado el límite de solicitudes al catálogo externo. Intente nuevamente más tarde.")
    {
        return new ExternalProductResultDto
        {
            Status = ExternalProductLookupStatus.RateLimitExceeded,
            Message = message
        };
    }

    public static ExternalProductResultDto ServiceUnavailable(string message = "El servicio de catálogo externo no se encuentra disponible.")
    {
        return new ExternalProductResultDto
        {
            Status = ExternalProductLookupStatus.ServiceUnavailable,
            Message = message
        };
    }
}
