using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Combat;

namespace NexusForever.Game.Abstract.Spell
{
    public interface ISpellExecutionContext
    {
        ISpell Spell { get; }
        ISpellTargetCollection TargetCollection { get; }
        bool IsDelayed { get; }

        /// <summary>
        /// Initialise the spell execution context with the supplied <see cref="ISpell"/>.
        /// </summary>
        void Initialise(ISpell spell, bool isDelayed = false);

        /// <summary>
        /// Add <see cref="Spell4EffectsEntry"/> to the spell execution.
        /// </summary>
        void AddSpellEffect(Spell4EffectsEntry entry);

        /// <summary>
        /// Get all <see cref="Spell4EffectsEntry"/> for spell execution.
        /// </summary>
        IEnumerable<Spell4EffectsEntry> GetSpellEffects();

        /// <summary>
        ///  Add <see cref="IProxy"/> to the spell execution.
        /// </summary>
        void AddProxy(IProxy proxy);

        /// <summary>
        /// Get all <see cref="IProxy"/> from spell execution.
        /// </summary>
        IEnumerable<IProxy> GetProxies();

        /// <summary>
        /// Increment effect trigger count of supplied effect id for spell execution.
        /// </summary>
        void IncrementEffectTriggerCount(uint effectId);

        /// <summary>
        /// Returns number of times a certain effect has been triggered, for this spell cast, with a given ID.
        /// </summary>
        bool GetEffectTriggerCount(uint effectId, out uint count);

        /// <summary>
        /// Add combat log to the effect.
        /// </summary>
        void AddCombatLog(ICombatLog combatLog);

        /// <summary>
        /// Get all combat logs for the spell execution.
        /// </summary>
        IEnumerable<ICombatLog> GetCombatLogs();
    }
}
