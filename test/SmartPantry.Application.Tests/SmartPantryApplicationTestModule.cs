using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SmartPantry.Products;
using Volo.Abp.Modularity;

namespace SmartPantry;

[DependsOn(
    typeof(SmartPantryApplicationModule),
    typeof(SmartPantryDomainTestModule)
)]
public class SmartPantryApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var mockCatalogClient = Substitute.For<IExternalProductCatalogClient>();
        context.Services.AddSingleton(mockCatalogClient);
    }
}
