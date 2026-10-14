using System;
using System.Threading.Tasks;
using SmartPantry.Products;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace SmartPantry.Pantry;

public class PantryDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<PantryItem, Guid> _pantryItemRepository;
    private readonly IGuidGenerator _guidGenerator;

    // Identificador de usuario de prueba para la despensa (sin depender de sesión HTTP ni ICurrentUser)
    public static readonly Guid DemoUserId = Guid.Parse("4a1f94c0-2134-4b51-92b0-08db36605a90");

    public PantryDataSeedContributor(
        IRepository<Product, Guid> productRepository,
        IRepository<PantryItem, Guid> pantryItemRepository,
        IGuidGenerator guidGenerator)
    {
        _productRepository = productRepository;
        _pantryItemRepository = pantryItemRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _pantryItemRepository.GetCountAsync() > 0)
        {
            return;
        }

        var milk = await GetOrCreateProductAsync("Leche Entera", "La Serenísima");
        var rice = await GetOrCreateProductAsync("Arroz Gallo Oro", "Gallo");
        var yogurt = await GetOrCreateProductAsync("Yogur Frutilla", "Yogs");
        var tuna = await GetOrCreateProductAsync("Atún en Aceite", "La Campagnola");
        var pasta = await GetOrCreateProductAsync("Fideos Tallarines", "Matarazzo");

        var today = DateTime.UtcNow.Date;

        // 1. Ítem próximo a vencer (dentro de 2 días)
        await _pantryItemRepository.InsertAsync(
            new PantryItem(_guidGenerator.Create(), DemoUserId, milk.Id, today.AddDays(2)),
            autoSave: true);

        // 2. Ítem en el día límite (dentro de 3 días)
        await _pantryItemRepository.InsertAsync(
            new PantryItem(_guidGenerator.Create(), DemoUserId, yogurt.Id, today.AddDays(3)),
            autoSave: true);

        // 3. Ítem con vencimiento lejano / fuera de umbral (dentro de 30 días)
        await _pantryItemRepository.InsertAsync(
            new PantryItem(_guidGenerator.Create(), DemoUserId, tuna.Id, today.AddDays(30)),
            autoSave: true);

        // 4. Ítem sin fecha de vencimiento
        await _pantryItemRepository.InsertAsync(
            new PantryItem(_guidGenerator.Create(), DemoUserId, pasta.Id, null),
            autoSave: true);

        // 5. Ítem ya consumido
        await _pantryItemRepository.InsertAsync(
            new PantryItem(_guidGenerator.Create(), DemoUserId, rice.Id, today.AddDays(1), PantryItemStatus.Consumed),
            autoSave: true);
    }

    private async Task<Product> GetOrCreateProductAsync(string name, string brand)
    {
        var product = await _productRepository.FindAsync(p => p.Name == name && p.Brand == brand);
        if (product != null)
        {
            return product;
        }

        return await _productRepository.InsertAsync(
            new Product(_guidGenerator.Create(), name, brand),
            autoSave: true);
    }
}
