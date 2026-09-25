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

    [Fact]
    public void Should_Update_Valid_Product_And_Normalize_Text()
    {
        // Arrange
        var product = new Product(Guid.NewGuid(), "Arroz", "Gallo");

        // Act
        product.Update("  Fideos Tallarines  ", "  Matarazzo  ");

        // Assert
        product.Name.ShouldBe("Fideos Tallarines");
        product.Brand.ShouldBe("Matarazzo");
    }

    [Theory]
    [InlineData("", "Matarazzo")]
    [InlineData("   ", "Matarazzo")]
    [InlineData(null, "Matarazzo")]
    public void Should_Not_Update_Product_With_Invalid_Name_And_Keep_Previous_State(string? invalidName, string validBrand)
    {
        // Arrange
        var product = new Product(Guid.NewGuid(), "Arroz", "Gallo");

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() =>
        {
            product.Update(invalidName!, validBrand);
        });

        product.Name.ShouldBe("Arroz");
        product.Brand.ShouldBe("Gallo");
    }

    [Theory]
    [InlineData("Fideos", "")]
    [InlineData("Fideos", "   ")]
    [InlineData("Fideos", null)]
    public void Should_Not_Update_Product_With_Invalid_Brand_And_Keep_Previous_State(string validName, string? invalidBrand)
    {
        // Arrange
        var product = new Product(Guid.NewGuid(), "Arroz", "Gallo");

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() =>
        {
            product.Update(validName, invalidBrand!);
        });

        product.Name.ShouldBe("Arroz");
        product.Brand.ShouldBe("Gallo");
    }
}

