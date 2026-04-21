using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Entity;
using NexusForever.Game.PathContent.Static;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.PathMissionIncrement)]
    public class SpellEffectPathMissionIncrementHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        private readonly IGameTableManager gameTableManager;

        public SpellEffectPathMissionIncrementHandler(
            IGameTableManager gameTableManager)
        {
            this.gameTableManager = gameTableManager;
        }

        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target is not Player player)
                return SpellEffectExecutionResult.PreventEffect;

            if (data.DataBits00 == 0u)
                return SpellEffectExecutionResult.PreventEffect;

            uint amount = data.DataBits02 == 0u ? 1u : data.DataBits02;
            PathMissionEntry missionEntry = gameTableManager.PathMission.GetEntry(data.DataBits00);
            try
            {
                if (missionEntry != null)
                {
                    player.PathMissionManager.MissionUpdate((PathMissionType)missionEntry.PathMissionTypeEnum, missionEntry.ObjectId, amount);
                    return SpellEffectExecutionResult.Ok;
                }

                if (data.DataBits01 == 0u)
                    return SpellEffectExecutionResult.PreventEffect;

                player.PathMissionManager.MissionUpdate((PathMissionType)data.DataBits00, data.DataBits01, amount);
                return SpellEffectExecutionResult.Ok;
            }
            catch (InvalidOperationException)
            {
                return SpellEffectExecutionResult.PreventEffect;
            }
            catch (ArgumentException)
            {
                return SpellEffectExecutionResult.PreventEffect;
            }
        }
    }
}
