using Microsoft.Extensions.DependencyInjection;

namespace Salr.UI.Services
{
    public static class UIServiceExtensions
    {
        public static IServiceCollection AddUIServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<Db>();
            return serviceCollection;
        }
    }
}