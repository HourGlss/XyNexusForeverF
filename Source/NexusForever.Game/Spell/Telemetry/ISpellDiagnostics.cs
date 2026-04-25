using System.Diagnostics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Chat;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Static;

namespace NexusForever.Game.Spell.Telemetry
{
    public interface ISpellDiagnostics
    {
        Activity StartCast(ISpell spell);
        void StopCast(ISpell spell, Activity activity, string outcome, CastResult? result = null);
        void RecordCastFailure(ISpell spell, Activity activity, CastResult result, string stage);
        void RecordStatusChange(ISpell spell, Activity activity, SpellStatus previousStatus, SpellStatus currentStatus);
        void RecordExecutionStart(ISpell spell, Activity activity, ISpellExecutionContext executionContext);
        void RecordTargetsSelected(ISpell spell, Activity activity, ISpellExecutionContext executionContext);
        void RecordEffectSkipped(ISpell spell, Activity activity, Spell4EffectsEntry effect, string reason);
        void RecordEffectResult(ISpell spell, Activity activity, Spell4EffectsEntry effect, ISpellTarget target, SpellEffectExecutionResult result);
        void RecordEffectHandlerResult(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, SpellEffectExecutionResult result, string handlerName);
        void RecordEffectHandlerMissing(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, string reason);
        void RecordPrerequisiteResult(IPlayer player, uint prerequisiteId, PrerequisiteType type, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters, bool met);
        void RecordValidatorFailure(ISpell spell, string validatorName, CastResult result);
        void RecordChatMessage(IPlayer player, ChatChannelType channel, string message);
    }
}
