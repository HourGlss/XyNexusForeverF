using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Proc;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Proc;
using NexusForever.Shared;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.Damage)]
    public class SpellEffectDamageHandler : ISpellEffectApplyHandler<ISpellEffectDamageData>
    {
        #region Dependency Injection

        private readonly IFactory<IProcParameters> procParameterFactory;

        public SpellEffectDamageHandler(
            IFactory<IProcParameters> procParameterFactory)
        {
            this.procParameterFactory = procParameterFactory;
        }

        #endregion

        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Apply(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDamageData data)
        {
            if (!target.CanAttack(spell.Caster))
                return;

            // TODO: Merge DamageCalculator, uncomment below lines, and delete the hardcoded values before target takes damage.

            // uint damage = 0;
            // damage += DamageCalculator.Instance.GetBaseDamageForSpell(caster, info.Entry.ParameterType00, info.Entry.ParameterValue00);
            // damage += DamageCalculator.Instance.GetBaseDamageForSpell(caster, info.Entry.ParameterType01, info.Entry.ParameterValue01);
            // damage += DamageCalculator.Instance.GetBaseDamageForSpell(caster, info.Entry.ParameterType02, info.Entry.ParameterValue02);
            // damage += DamageCalculator.Instance.GetBaseDamageForSpell(caster, info.Entry.ParameterType03, info.Entry.ParameterValue03);

            // DamageCalculator.Instance.CalculateDamage(caster, target, this, info, (DamageType)info.Entry.DamageType, damage);

            info.AddDamage((DamageType)info.Entry.DamageType, 50);
            info.Damage.ShieldAbsorbAmount = 25;
            info.Damage.AdjustedDamage = 50;

            if (info.Damage.CombatResult == CombatResult.Critical)
            {
                IProcParameters parameters = procParameterFactory.Resolve();
                parameters.Target = target;
                spell.Caster.ProcManager.TriggerProc(ProcType.CriticalDamage, parameters);
            }

            // TODO: Deal damage
            target.TakeDamage(spell.Caster, info.Damage);
        }
    }
}
