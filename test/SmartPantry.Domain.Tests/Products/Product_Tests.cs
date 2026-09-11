using System;
using Shouldly;
using Xunit;

namespace SmartPantry.Products;

public class Product_Tests
{
    [Fact]
    public void Should_Create_Valid_Product_And_Normalize_Text()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "  Arroz Integral  ";
        var brand = "  Gallo  ";

        // Act
        var product = new Product(id, name, brand);

        // Assert
        product.Id.ShouldBe(id);
        product.Name.ShouldBe("Arroz Integral");
        product.Brand.ShouldBe("Gallo");
    }

    [Theory]
    [InlineData("", "Gallo")]
    [InlineData("   ", "Gallo")]
    [InlineData(null, "Gallo")]
    public void Should_Throw_Exception_When_Name_Is_Null_Or_WhiteSpace(string? invalidName, string validBrand)
    {
        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() =>
        {
            new Product(Guid.NewGuid(), invalidName!, validBrand);
        });
    }

    [Theory]
    [InlineData("Arroz", "")]
    [InlineData("Arroz", "   ")]
    [InlineData("Arroz", null)]
    public void Should_Throw_Exception_When_Brand_Is_Null_Or_WhiteSpace(string validName, string? invalidBrand)
    {
        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() =>
        {
            new Product(Guid.NewGuid(), validName, invalidBrand!);
        });
    }
}
