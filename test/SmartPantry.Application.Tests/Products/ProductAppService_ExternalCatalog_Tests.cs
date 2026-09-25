using System;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SmartPantry.Products;

public class ProductAppService_ExternalCatalog_Tests
{
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IExternalProductCatalogClient _externalCatalogClient;
    private readonly ProductAppService _appService;

    public ProductAppService_ExternalCatalog_Tests()
    {
        _productRepository = Substitute.For<IRepository<Product, Guid>>();
        _externalCatalogClient = Substitute.For<IExternalProductCatalogClient>();
        _appService = new ProductAppService(_productRepository, _externalCatalogClient);
    }

    [Fact]
    public async Task Should_Return_Found_When_Product_Exists()
    {
        // Arrange
        const string barcode = "3017620422003";
        var expectedProduct = new ExternalProductDto
        {
            Barcode = barcode,
            Name = "Nutella",
            Brand = "Ferrero",
            ImageUrl = "https://images.openfoodfacts.org/images/products/301/762/042/2003/front.jpg",
            Ingredients = "Sucre, huile de palme, noisettes",
            NutriScore = "E"
        };

        _externalCatalogClient.GetByBarcodeAsync(barcode).Returns(expectedProduct);

        // Act
        var result = await _appService.GetByBarcodeAsync(new GetProductByBarcodeDto { Barcode = barcode });

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe(ExternalProductLookupStatus.Found);
        result.Product.ShouldNotBeNull();
        result.Product.Barcode.ShouldBe(barcode);
        result.Product.Name.ShouldBe("Nutella");
        result.Product.Brand.ShouldBe("Ferrero");
        result.Product.NutriScore.ShouldBe("E");
    }

    [Fact]
    public async Task Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        // Arrange
        const string barcode = "0000000000000";
        _externalCatalogClient.GetByBarcodeAsync(barcode).Returns((ExternalProductDto?)null);

        // Act
        var result = await _appService.GetByBarcodeAsync(new GetProductByBarcodeDto { Barcode = barcode });

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe(ExternalProductLookupStatus.NotFound);
        result.Product.ShouldBeNull();
        result.Message.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Return_Found_With_Missing_Fields_Without_Inventing_Values()
    {
        // Arrange - RF-09: Datos incompletos deben permanecer ausentes
        const string barcode = "12345678";
        var productWithMissingData = new ExternalProductDto
        {
            Barcode = barcode,
            Name = null,
            Brand = null,
            ImageUrl = null,
            Ingredients = null,
            NutriScore = null
        };

        _externalCatalogClient.GetByBarcodeAsync(barcode).Returns(productWithMissingData);

        // Act
        var result = await _appService.GetByBarcodeAsync(new GetProductByBarcodeDto { Barcode = barcode });

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe(ExternalProductLookupStatus.Found);
        result.Product.ShouldNotBeNull();
        result.Product.Barcode.ShouldBe(barcode);
        result.Product.Name.ShouldBeNull();
        result.Product.Brand.ShouldBeNull();
        result.Product.ImageUrl.ShouldBeNull();
        result.Product.Ingredients.ShouldBeNull();
        result.Product.NutriScore.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Return_RateLimitExceeded_When_Rate_Limit_Is_Reached()
    {
        // Arrange - Simulación de HTTP 429
        const string barcode = "3017620422003";
        _externalCatalogClient.GetByBarcodeAsync(barcode)
            .ThrowsAsync(new ExternalCatalogRateLimitException("Límite de solicitudes alcanzado."));

        // Act
        var result = await _appService.GetByBarcodeAsync(new GetProductByBarcodeDto { Barcode = barcode });

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe(ExternalProductLookupStatus.RateLimitExceeded);
        result.Product.ShouldBeNull();
        result.Message.ShouldNotBeNull();
        result.Message.ShouldContain("límite");
    }

    [Fact]
    public async Task Should_Return_ServiceUnavailable_When_External_Service_Fails()
    {
        // Arrange - Simulación de caída del servicio / timeout / 503
        const string barcode = "3017620422003";
        _externalCatalogClient.GetByBarcodeAsync(barcode)
            .ThrowsAsync(new ExternalCatalogUnavailableException("Open Food Facts no responde."));

        // Act
        var result = await _appService.GetByBarcodeAsync(new GetProductByBarcodeDto { Barcode = barcode });

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe(ExternalProductLookupStatus.ServiceUnavailable);
        result.Product.ShouldBeNull();
        result.Message.ShouldNotBeNullOrWhiteSpace();
    }
}
