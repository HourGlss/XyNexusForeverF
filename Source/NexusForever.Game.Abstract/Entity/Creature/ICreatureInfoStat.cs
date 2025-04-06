namespace NexusForever.Game.Abstract.Entity.Creature
{
    public interface ICreatureInfoStat
    {
        Static.Entity.Stat Stat { get; }
        float Value { get; }

        void Initialise(Static.Entity.Stat stat, float value);
    }
}
