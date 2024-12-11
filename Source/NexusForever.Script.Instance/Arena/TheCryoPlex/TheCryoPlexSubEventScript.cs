using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Arena.TheCryoPlex
{
    [ScriptFilterOwnerId(582)]
    public class TheCryoPlexSubEventScript : ArenaSubEventScript<PublicEventObjective>
    {
        public override PublicEventObjective PrepareForBattleObjective => PublicEventObjective.PrepareForBattle;
        public override PublicEventObjective ParticipateInArenaObjective => PublicEventObjective.ParticipateInArena;
    }
}
