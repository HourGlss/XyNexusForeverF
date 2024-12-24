using System;
using Microsoft.Extensions.DependencyInjection;

namespace NexusForever.Shared
{
    public static class ServiceProviderExtensions
    {
        /// <remarks>
        /// Temporary fix for missing GetKeyedService method in Microsoft.Extensions.DependencyInjection, see https://github.com/dotnet/runtime/issues/102816
        /// </remarks>
        public static object GetKeyedService(this IServiceProvider provider, Type serviceType, object serviceKey)
        {
            if (provider is IKeyedServiceProvider keyedServiceProvider)
                return keyedServiceProvider.GetKeyedService(serviceType, serviceKey);

            return null;
        }
    }
}
