using NexusForever.Game.Abstract.Combat.CrowdControl;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Combat.CrowdControl;

namespace NexusForever.Game.Combat.CrowdControl
{
    public interface ICrowdControlManager
    {
        void Initialise(IUnitEntity entity);

        /// <summary>
        /// Checks if the specified <see cref="CCState"/> can be applied to the target.
        /// </summary>
        ICrowdControlApplyResult CanApplyCCEffect(CCState ccState);

        /// <summary>
        /// Adds the specified <see cref="CCState"/> to the target.
        /// </summary>
        void AddCCEffect(CCState state, ISpellTargetEffectInfo effect);

        /// <summary>
        /// Removes the specified <see cref="CCState"/> from the target.
        /// </summary>
        void RemoveCCEffect(CCState state);

        /// <summary>
        /// Returns the <see cref="ISpellTargetEffectInfo"/> for the specified <see cref="CCState"/>.
        /// </summary>
        ISpellTargetEffectInfo GetCCEffect(CCState state);

        /// <summary>
        /// Returns all <see cref="CCState"/>'s currently affecting the target.
        /// </summary>
        IEnumerable<CCState> GetCCStates();

        /// <summary>
        /// Returns the total amount of temporary interrupt armour on the target.
        /// </summary>
        uint GetTemporaryInterruptArmour();

        /// <summary>
        /// Add temporary interrupt armour for the specified <see cref="ISpellTargetEffectInfo"/>.
        /// </summary>
        void AddTemporaryInterruptArmour(ISpellTargetEffectInfo info, uint amount);

        /// <summary>
        /// Remove the temporary interrupt armour granted from the specified <see cref="ISpellTargetEffectInfo"/>.
        /// </summary>
        void RemoveTemporaryInterruptArmour(ISpellTargetEffectInfo info);

        /// <summary>
        /// Removes the specified amount of interrupt armour from the target.
        /// </summary>
        /// <remarks>
        /// This will first remove temporary interrupt armour, then permanent interrupt armour.
        /// </remarks>
        void RemoveInterruptArmour(ref uint amount);
    }
}
