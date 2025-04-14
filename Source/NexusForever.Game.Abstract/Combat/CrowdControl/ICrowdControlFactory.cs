using NexusForever.Game.Static.Combat.CrowdControl;

namespace NexusForever.Game.Abstract.Combat.CrowdControl
{
    public interface ICrowdControlFactory
    {
        /// <summary>
        /// Return the <see cref="ICrowdControlApplyHandler"/> for the given <see cref="CCState"/>.
        /// </summary>
        ICrowdControlApplyHandler GetApplyHandler(CCState state);

        /// <summary>
        /// Return the <see cref="ICrowdControlRemoveHandler"/> for the given <see cref="CCState"/>.
        /// </summary>
        ICrowdControlRemoveHandler GetRemoveHandler(CCState state);
    }
}
