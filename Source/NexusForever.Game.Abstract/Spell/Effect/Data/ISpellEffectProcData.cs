using NexusForever.Game.Static.Spell.Proc;

namespace NexusForever.Game.Abstract.Spell.Effect.Data
{
    public interface ISpellEffectProcData : ISpellEffectData
    {
        ProcType ProcType { get; }
        uint SpellId { get; }
        uint TriggerTime { get; }
        uint TargetPrerequisiteId { get; }
        uint CasterPrerequisiteId { get; }
    }
}
