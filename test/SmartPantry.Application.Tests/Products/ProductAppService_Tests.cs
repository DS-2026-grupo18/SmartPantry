using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartPantry.Products;

public abstract class ProductAppService_Tests<TStartupModule> : SmartPantryApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IProductAppService _productAppService;

    protected ProductAppService_Tests()
    {
        _productAppService = GetRequiredService<IProductAppService>();
    }

    [Fact]
    public async Task Should_Create_And_Get_Product_By_Id()
    {
        // Arrange
        var input = new CreateProductDto
        {
            Name = "Aceite de Oliva",
            Brand = "Natura"
        };

        // Act - Create
        var created = await _productAppService.CreateAsync(input);

        // Assert - Creation
        created.Id.ShouldNotBe(Guid.Empty);
        created.Name.ShouldBe("Aceite de Oliva");
        created.Brand.ShouldBe("Natura");

        // Act - Get by ID
        var retrieved = await _productAppService.GetAsync(created.Id);

        // Assert - Retrieval
        retrieved.ShouldNotBeNull();
        retrieved.Id.ShouldBe(created.Id);
        retrieved.Name.ShouldBe(created.Name);
        retrieved.Brand.ShouldBe(created.Brand);
    }

    [Fact]
    public async Task Should_Not_Create_Product_Without_Name()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _productAppService.CreateAsync(new CreateProductDto
            {
                Name = "",
                Brand = "Natura"
            });
        });

        exception.ValidationErrors
            .ShouldContain(err => err.MemberNames.Any(mem => mem == "Name"));
    }

    [Fact]
    public async Task Should_Not_Create_Product_Without_Brand()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _productAppService.CreateAsync(new CreateProductDto
            {
                Name = "Fideos",
                Brand = ""
            });
        });

        exception.ValidationErrors
            .ShouldContain(err => err.MemberNames.Any(mem => mem == "Brand"));
    }
}
