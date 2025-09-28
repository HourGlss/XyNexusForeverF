using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Arena.TheSlaughterdome
{
    [ScriptFilterOwnerId(209)]
    public class TheSlaughterdomeSubEventScript : ArenaSubEventScript<PublicEventObjective>
    {
        public override PublicEventObjective PrepareForBattleObjective => PublicEventObjective.PrepareForBattle;
        public override PublicEventObjective ParticipateInArenaObjective => PublicEventObjective.ParticipateInArena;
    }
}
