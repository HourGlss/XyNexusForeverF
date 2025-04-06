using NexusForever.Game.Abstract.Entity.Creature;

namespace NexusForever.Game.Entity.Creature
{
    public class CreatureInfoStat : ICreatureInfoStat
    {
        public Static.Entity.Stat Stat { get; private set; }
        public float Value { get; private set; }

        public void Initialise(Static.Entity.Stat stat, float value)
        {
            Stat  = stat;
            Value = value;
        }
    }
}
