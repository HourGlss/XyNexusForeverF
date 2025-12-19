namespace NexusForever.Game.Abstract.Map
{
    public interface IGridActionVisibilityUpdate : IGridAction
    {
        OnVisibilityUpdateDelegate Callback { get; init; }
    }
}
