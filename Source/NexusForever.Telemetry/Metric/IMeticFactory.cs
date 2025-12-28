namespace NexusForever.Telemetry.Metric
{
    public interface IMeticFactory
    {
        T Resolve<T>() where T : IMetric;
    }
}
