using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NexusForever.Telemetry.Configuration.Model;

namespace NexusForever.Telemetry.Metric
{
    public class MetricFactory : IMeticFactory
    {
        #region Dependency Injection

        private readonly IOptions<TelemetryOptions> options;
        private readonly IServiceProvider serviceProvider;

        public MetricFactory(
            IOptions<TelemetryOptions> options,
            IServiceProvider serviceProvider)
        {
            this.options         = options;
            this.serviceProvider = serviceProvider;
        }

        #endregion

        public T Resolve<T>() where T : IMetric
        {
            if (options.Value.Metrics?.Enable is false)
                return default;

            return serviceProvider.GetService<T>();
        }
    }
}
