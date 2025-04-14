using NexusForever.Game.Static.Combat.CrowdControl;

namespace NexusForever.Game.Abstract.Spell.Effect.Data
{
    public interface ISpellEffectCCStateBreakData : ISpellEffectData
    {
        CCState CCState { get; }
    }
}
