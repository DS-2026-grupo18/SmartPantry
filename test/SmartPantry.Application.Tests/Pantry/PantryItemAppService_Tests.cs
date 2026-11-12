using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SmartPantry.Products;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Xunit;
using Volo.Abp.DependencyInjection;

namespace SmartPantry.Pantry;

public class PantryItemAppService_Tests
{
    private readonly IRepository<PantryItem, Guid> _pantryRepo;
    private readonly IRepository<Product, Guid> _productRepo;
    private readonly ICurrentUser _currentUser;
    private readonly PantryItemAppService _appService;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _otroUserId = Guid.NewGuid();

    public PantryItemAppService_Tests()
    {
        _pantryRepo = Substitute.For<IRepository<PantryItem, Guid>>();
        _productRepo = Substitute.For<IRepository<Product, Guid>>();
        _currentUser = Substitute.For<ICurrentUser>();

        _currentUser.Id.Returns(_userId);
        _currentUser.IsAuthenticated.Returns(true);

        var lazyServiceProvider = Substitute.For<IAbpLazyServiceProvider>();
        lazyServiceProvider.LazyGetRequiredService<ICurrentUser>().Returns(_currentUser);

        _appService = new PantryItemAppService(_pantryRepo, _productRepo)
        {
            LazyServiceProvider = lazyServiceProvider
        };
    }

    [Fact]
    public async Task Debe_Listar_Solo_Items_Del_Usuario_Actual()
    {
        var prodId = Guid.NewGuid();
        var producto = new Product(prodId, "Arroz", "Lucchetti");
        _productRepo.FindAsync(prodId).Returns(producto);

        var misItems = new List<PantryItem>
        {
            new PantryItem(Guid.NewGuid(), _userId, prodId),
            new PantryItem(Guid.NewGuid(), _otroUserId, prodId) // Item de otro usuario
        };

        _pantryRepo.GetQueryableAsync().Returns(misItems.AsQueryable());

        var input = new GetPantryItemsInputDto { SkipCount = 0, MaxResultCount = 10 };
        var resultado = await _appService.GetListAsync(input);

        resultado.TotalCount.ShouldBe(1);
        resultado.Items.Count.ShouldBe(1);
        resultado.Items[0].UserId.ShouldBe(_userId);
        resultado.Items[0].ProductName.ShouldBe("Arroz");
    }

    [Fact]
    public async Task Debe_Actualizar_Cantidad_Y_Unidad_De_Item_Propio()
    {
        var itemId = Guid.NewGuid();
        var prodId = Guid.NewGuid();
        var item = new PantryItem(itemId, _userId, prodId);
        _pantryRepo.FindAsync(itemId).Returns(item);

        var input = new UpdatePantryItemDto
        {
            Quantity = 5,
            Unit = "Kilos",
            ExpirationDate = new DateTime(2026, 12, 1)
        };

        var resultado = await _appService.UpdateAsync(itemId, input);

        resultado.Quantity.ShouldBe(5);
        resultado.Unit.ShouldBe("Kilos");
        item.Quantity.ShouldBe(5);
        item.Unit.ShouldBe("Kilos");
        await _pantryRepo.Received(1).UpdateAsync(item);
    }

    [Fact]
    public async Task Debe_Marcar_Item_Como_Consumido()
    {
        var itemId = Guid.NewGuid();
        var item = new PantryItem(itemId, _userId, Guid.NewGuid());
        _pantryRepo.FindAsync(itemId).Returns(item);

        var resultado = await _appService.ConsumeAsync(itemId);

        resultado.Status.ShouldBe(PantryItemStatus.Consumed);
        item.Status.ShouldBe(PantryItemStatus.Consumed);
        await _pantryRepo.Received(1).UpdateAsync(item);
    }

    [Fact]
    public async Task Debe_Eliminar_Item_Propio()
    {
        var itemId = Guid.NewGuid();
        var item = new PantryItem(itemId, _userId, Guid.NewGuid());
        _pantryRepo.FindAsync(itemId).Returns(item);

        await _appService.DeleteAsync(itemId);

        await _pantryRepo.Received(1).DeleteAsync(item);
    }

    [Fact]
    public async Task No_Debe_Permitir_Acceder_A_Item_De_Otro_Usuario()
    {
        var itemId = Guid.NewGuid();
        var itemAjeno = new PantryItem(itemId, _otroUserId, Guid.NewGuid());
        _pantryRepo.FindAsync(itemId).Returns(itemAjeno);

        // Debe lanzar EntityNotFoundException (que ABP traduce a 404)
        await Should.ThrowAsync<EntityNotFoundException>(async () =>
        {
            await _appService.UpdateAsync(itemId, new UpdatePantryItemDto { Quantity = 2 });
        });

        await Should.ThrowAsync<EntityNotFoundException>(async () =>
        {
            await _appService.ConsumeAsync(itemId);
        });

        await Should.ThrowAsync<EntityNotFoundException>(async () =>
        {
            await _appService.DeleteAsync(itemId);
        });
    }
}
