using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map;

namespace NexusForever.Game.Map
{
    public class GridActionVisibilityUpdate : IGridActionVisibilityUpdate
    {
        public IGridEntity Entity { get; init; }
        public OnVisibilityUpdateDelegate Callback { get; init; }
        public OnExceptionDelegate Exception { get; init; }
    }
}
