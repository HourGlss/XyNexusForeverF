using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Quest;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.QuestAdvanceObjective)]
    public class SpellEffectQuestAdvanceObjectiveHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target is not IPlayer player)
                return SpellEffectExecutionResult.PreventEffect;

            if (data.DataBits00 == 0u)
                return SpellEffectExecutionResult.PreventEffect;

            if (data.DataBits02 != 0u)
                player.QuestManager.ObjectiveUpdate((QuestObjectiveType)data.DataBits00, data.DataBits01, data.DataBits02);
            else
                player.QuestManager.ObjectiveUpdate(data.DataBits00, data.DataBits01 == 0u ? 1u : data.DataBits01);

            return SpellEffectExecutionResult.Ok;
        }
    }
}
