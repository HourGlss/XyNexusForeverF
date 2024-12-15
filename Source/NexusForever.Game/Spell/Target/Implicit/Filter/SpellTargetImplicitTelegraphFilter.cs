using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Search;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Target.Implicit;
using NexusForever.Game.Abstract.Spell.Target.Implicit.Filter;
using NexusForever.Game.Static.Spell.Target;

namespace NexusForever.Game.Spell.Target.Implicit.Filter
{
    public class SpellTargetImplicitTelegraphFilter : ISpellTargetImplicitTelegraphFilter
    {
        private ITelegraph telegraph;
        private IUnitEntity caster;

        #region Dependency Injection

        private readonly ISearchCheckTelegraph searchCheckTelegraph;

        public SpellTargetImplicitTelegraphFilter(
            ISearchCheckTelegraph searchCheckTelegraph)
        {
            this.searchCheckTelegraph = searchCheckTelegraph;
        }

        #endregion

        public void Initialise(ITelegraph telegraph, IUnitEntity caster)
        {
            if (this.telegraph != null)
                throw new InvalidOperationException("SpellTargetImplicitTelegraphFilter has already been initialised.");

            this.telegraph = telegraph;
            this.caster    = caster;

            searchCheckTelegraph.Initialise(telegraph, caster);
        }

        public void Filter(List<ISpellTargetImplicit> targets)
        {
            foreach (ISpellTargetImplicit target in targets)
                if (!searchCheckTelegraph.CheckEntity(target.Entity))
                    target.Result = SpellTargetImplicitSelectionResult.OutsideTelegraph;
        }

        // TODO: this needs more research...
        /*private bool EvaluateDamageFlagsForTarget(IGridEntity target, ISpell spell)
        {
            TelegraphDamageFlag damageFlag = (TelegraphDamageFlag)TelegraphDamage.TelegraphDamageFlags;

            // This is Invalid
            //if (damageFlag.HasFlag(TelegraphDamageFlag.SpellMustBeMultiPhase))
            //    if (spell.CastMethod != CastMethod.Multiphase)
            //        return false;

            if (damageFlag.HasFlag(TelegraphDamageFlag.CasterMustBeNPC))
                if (Caster is IPlayer)
                    return false;

            if (damageFlag.HasFlag(TelegraphDamageFlag.CasterMustBePlayer))
                if (Caster is not IPlayer)
                    return false;

            if (damageFlag.HasFlag(TelegraphDamageFlag.TargetMustBeUnit))
                if (target is not IUnitEntity)
                    return false;

            return true;
        }*/
    }
}
