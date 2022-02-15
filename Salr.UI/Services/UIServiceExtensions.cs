using Microsoft.Extensions.DependencyInjection;
using NNostr.UI;

namespace Salr.UI.Services
{
    public static class UIServiceExtensions
    {
        public static IServiceCollection AddUIServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<Db>();
            serviceCollection.AddSingleton<ISimilarHostedService>(provider => provider.GetService<Db>());
            return serviceCollection;
        }
    }
}