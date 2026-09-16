using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
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
        var input = new CreateUpdateProductDto
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
    public async Task Should_Get_List_Of_Products()
    {
        // Arrange
        await _productAppService.CreateAsync(new CreateUpdateProductDto
        {
            Name = "Yerba Mate",
            Brand = "Playadito"
        });

        await _productAppService.CreateAsync(new CreateUpdateProductDto
        {
            Name = "Azúcar",
            Brand = "Ledesma"
        });

        // Act
        var result = await _productAppService.GetListAsync(new PagedAndSortedResultRequestDto
        {
            MaxResultCount = 10,
            Sorting = "Name ASC"
        });

        // Assert
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
        result.Items.ShouldNotBeEmpty();
        result.Items.ShouldContain(p => p.Name == "Yerba Mate" && p.Brand == "Playadito");
        result.Items.ShouldContain(p => p.Name == "Azúcar" && p.Brand == "Ledesma");
    }

    [Fact]
    public async Task Should_Update_Product()
    {
        // Arrange
        var created = await _productAppService.CreateAsync(new CreateUpdateProductDto
        {
            Name = "Arroz",
            Brand = "Gallo"
        });

        var updateInput = new CreateUpdateProductDto
        {
            Name = "Arroz Doble Carolina",
            Brand = "Molinos Ala"
        };

        // Act
        var updated = await _productAppService.UpdateAsync(created.Id, updateInput);

        // Assert
        updated.ShouldNotBeNull();
        updated.Id.ShouldBe(created.Id);
        updated.Name.ShouldBe("Arroz Doble Carolina");
        updated.Brand.ShouldBe("Molinos Ala");

        var retrieved = await _productAppService.GetAsync(created.Id);
        retrieved.Name.ShouldBe("Arroz Doble Carolina");
        retrieved.Brand.ShouldBe("Molinos Ala");
    }

    [Fact]
    public async Task Should_Delete_Product()
    {
        // Arrange
        var created = await _productAppService.CreateAsync(new CreateUpdateProductDto
        {
            Name = "Leche Entera",
            Brand = "La Serenísima"
        });

        // Act - Delete
        await _productAppService.DeleteAsync(created.Id);

        // Assert - Getting the deleted entity throws EntityNotFoundException
        await Assert.ThrowsAnyAsync<EntityNotFoundException>(async () =>
        {
            await _productAppService.GetAsync(created.Id);
        });
    }

    [Fact]
    public async Task Should_Not_Create_Product_Without_Name()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _productAppService.CreateAsync(new CreateUpdateProductDto
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
            await _productAppService.CreateAsync(new CreateUpdateProductDto
            {
                Name = "Fideos",
                Brand = ""
            });
        });

        exception.ValidationErrors
            .ShouldContain(err => err.MemberNames.Any(mem => mem == "Brand"));
    }

    [Fact]
    public async Task Should_Not_Update_Product_Without_Name()
    {
        // Arrange
        var created = await _productAppService.CreateAsync(new CreateUpdateProductDto
        {
            Name = "Fideos",
            Brand = "Matarazzo"
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _productAppService.UpdateAsync(created.Id, new CreateUpdateProductDto
            {
                Name = "",
                Brand = "Matarazzo"
            });
        });

        exception.ValidationErrors
            .ShouldContain(err => err.MemberNames.Any(mem => mem == "Name"));
    }

    [Fact]
    public async Task Should_Not_Update_Product_Without_Brand()
    {
        // Arrange
        var created = await _productAppService.CreateAsync(new CreateUpdateProductDto
        {
            Name = "Fideos",
            Brand = "Matarazzo"
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _productAppService.UpdateAsync(created.Id, new CreateUpdateProductDto
            {
                Name = "Fideos Tallarines",
                Brand = ""
            });
        });

        exception.ValidationErrors
            .ShouldContain(err => err.MemberNames.Any(mem => mem == "Brand"));
    }
}

