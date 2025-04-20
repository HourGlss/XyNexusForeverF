using NexusForever.Game.Abstract.Entity.Movement.Force;
using NexusForever.Game.Static.Entity.Movement.Command.State;

namespace NexusForever.Game.Entity.Movement.Force
{
    public class StateKeyGenerator : IStateKeyGenerator
    {
        /// <summary>
        /// Generate state key data with the supplied parameters.
        /// </summary>
        public void Calculate(TimeSpan flightTime, float gravity, float spin, out List<StateFlags> states, out List<uint> outTimes)
        {
            states   = [];
            outTimes = [];

            StateFlags flags = StateFlags.None;
            if (gravity > 0)
            {
                flags |= StateFlags.Velocity | StateFlags.Fall;
                if (spin != 0)
                    flags |= StateFlags.RotationWhileFalling;
            }

            states.Add(flags);
            outTimes.Add(0);

            states.Add(StateFlags.None);
            outTimes.Add((uint)flightTime.TotalMilliseconds);
        }
    }
}
