using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Chat;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Static;

namespace NexusForever.Game.Spell.Telemetry
{
    public class SpellDiagnostics : ISpellDiagnostics
    {
        private readonly Counter<long> spellCastCounter = SpellDiagnosticsTelemetry.Meter.CreateCounter<long>("nexusforever.spell.cast.count");
        private readonly Counter<long> spellEffectCounter = SpellDiagnosticsTelemetry.Meter.CreateCounter<long>("nexusforever.spell.effect.count");
        private readonly Counter<long> spellPrerequisiteCounter = SpellDiagnosticsTelemetry.Meter.CreateCounter<long>("nexusforever.spell.prerequisite.count");
        private readonly Counter<long> spellTestMarkerCounter = SpellDiagnosticsTelemetry.Meter.CreateCounter<long>("nexusforever.spell.test.marker.count");

        private readonly ConcurrentDictionary<ulong, SpellDiagnosticSession> activeSessions = new();

        private readonly ILogger<SpellDiagnostics> log;
        private readonly IOptions<SpellDiagnosticsOptions> options;

        public SpellDiagnostics(
            ILogger<SpellDiagnostics> log,
            IOptions<SpellDiagnosticsOptions> options)
        {
            this.log     = log;
            this.options = options;
        }

        public Activity StartCast(ISpell spell)
        {
            if (!ShouldRecord(spell))
                return null;

            TagList tags = GetSpellTags(spell, includeCastId: true, includePlayer: true);
            Activity activity = SpellDiagnosticsTelemetry.ActivitySource.StartActivity("spell.cast", ActivityKind.Internal);
            if (activity != null)
            {
                foreach (KeyValuePair<string, object> tag in tags)
                    activity.SetTag(tag.Key, tag.Value);
            }

            spellCastCounter.Add(1, GetMetricTags(spell, ("result", "started")));
            log.LogInformation("Spell diagnostic cast started: spell4={Spell4Id} base={Spell4BaseId} tier={Tier} method={CastMethod} casting={CastingId} class={PlayerClass} player={PlayerName} test={TestLabel}",
                spell.Parameters.SpellInfo.Entry.Id,
                spell.Parameters.SpellInfo.Entry.Spell4BaseIdBaseSpell,
                spell.Parameters.SpellInfo.Entry.TierIndex,
                spell.CastMethod,
                spell.CastingId,
                GetPlayerClass(spell),
                GetPlayerName(spell),
                GetActiveSession(spell)?.Label);

            return activity;
        }

        public void StopCast(ISpell spell, Activity activity, string outcome, CastResult? result = null)
        {
            if (!ShouldRecord(spell) && activity == null)
                return;

            TagList tags = GetSpellTags(spell, includeCastId: true, includePlayer: true);
            tags.Add("nf.spell.outcome", outcome);
            if (result != null)
                tags.Add("nf.spell.cast_result", result.Value.ToString());

            AddEvent(activity, "spell.cast.stop", tags);
            activity?.SetTag("nf.spell.outcome", outcome);
            if (result != null)
                activity?.SetTag("nf.spell.cast_result", result.Value.ToString());

            spellCastCounter.Add(1, GetMetricTags(spell, ("result", result?.ToString() ?? outcome)));
            log.LogInformation("Spell diagnostic cast stopped: spell4={Spell4Id} base={Spell4BaseId} tier={Tier} outcome={Outcome} result={CastResult} casting={CastingId} class={PlayerClass} player={PlayerName} test={TestLabel}",
                spell.Parameters.SpellInfo.Entry.Id,
                spell.Parameters.SpellInfo.Entry.Spell4BaseIdBaseSpell,
                spell.Parameters.SpellInfo.Entry.TierIndex,
                outcome,
                result,
                spell.CastingId,
                GetPlayerClass(spell),
                GetPlayerName(spell),
                GetActiveSession(spell)?.Label);

            activity?.Stop();
        }

        public void RecordCastFailure(ISpell spell, Activity activity, CastResult result, string stage)
        {
            if (!ShouldRecord(spell) && activity == null)
                return;

            TagList tags = GetSpellTags(spell, includeCastId: true, includePlayer: true);
            tags.Add("nf.spell.stage", stage);
            tags.Add("nf.spell.cast_result", result.ToString());
            AddEvent(activity, "spell.cast.failure", tags);

            spellCastCounter.Add(1, GetMetricTags(spell, ("result", result.ToString()), ("stage", stage)));
            log.LogInformation("Spell diagnostic cast failed: spell4={Spell4Id} base={Spell4BaseId} tier={Tier} stage={Stage} result={CastResult} casting={CastingId} class={PlayerClass} player={PlayerName} test={TestLabel}",
                spell.Parameters.SpellInfo.Entry.Id,
                spell.Parameters.SpellInfo.Entry.Spell4BaseIdBaseSpell,
                spell.Parameters.SpellInfo.Entry.TierIndex,
                stage,
                result,
                spell.CastingId,
                GetPlayerClass(spell),
                GetPlayerName(spell),
                GetActiveSession(spell)?.Label);
        }

        public void RecordStatusChange(ISpell spell, Activity activity, SpellStatus previousStatus, SpellStatus currentStatus)
        {
            if (!ShouldRecord(spell) && activity == null)
                return;

            TagList tags = GetSpellTags(spell, includeCastId: true, includePlayer: false);
            tags.Add("nf.spell.previous_status", previousStatus.ToString());
            tags.Add("nf.spell.current_status", currentStatus.ToString());
            AddEvent(activity, "spell.status.change", tags);
        }

        public void RecordExecutionStart(ISpell spell, Activity activity, ISpellExecutionContext executionContext)
        {
            if (!ShouldRecord(spell) && activity == null)
                return;

            TagList tags = GetSpellTags(spell, includeCastId: true, includePlayer: false);
            tags.Add("nf.spell.effect_count", executionContext.GetSpellEffects().Count());
            tags.Add("nf.spell.delayed", executionContext.IsDelayed);
            AddEvent(activity, "spell.execution.start", tags);
        }

        public void RecordTargetsSelected(ISpell spell, Activity activity, ISpellExecutionContext executionContext)
        {
            if (!ShouldRecord(spell) && activity == null)
                return;

            TagList tags = GetSpellTags(spell, includeCastId: true, includePlayer: false);
            tags.Add("nf.spell.targets.caster", executionContext.TargetCollection.GetTargets(SpellEffectTargetFlags.Caster).Count());
            tags.Add("nf.spell.targets.explicit", executionContext.TargetCollection.GetTargets(SpellEffectTargetFlags.ExplicitTarget).Count());
            tags.Add("nf.spell.targets.implicit", executionContext.TargetCollection.GetTargets(SpellEffectTargetFlags.ImplicitTarget).Count());
            AddEvent(activity, "spell.targets.selected", tags);
        }

        public void RecordEffectSkipped(ISpell spell, Activity activity, Spell4EffectsEntry effect, string reason)
        {
            if (!Options.IncludeEffectResults || (!ShouldRecord(spell) && activity == null))
                return;

            TagList tags = GetEffectTags(spell, effect);
            tags.Add("nf.spell.effect.result", "skipped");
            tags.Add("nf.spell.effect.skip_reason", reason);
            AddEvent(activity, "spell.effect.skipped", tags);

            spellEffectCounter.Add(1, GetEffectMetricTags(spell, effect, "skipped", reason));
        }

        public void RecordEffectResult(ISpell spell, Activity activity, Spell4EffectsEntry effect, ISpellTarget target, SpellEffectExecutionResult result)
        {
            if (!Options.IncludeEffectResults || (!ShouldRecord(spell) && activity == null))
                return;

            TagList tags = GetEffectTags(spell, effect);
            tags.Add("nf.spell.effect.result", result.ToString());
            tags.Add("nf.spell.effect.target_flags", target.Flags.ToString());
            AddEvent(activity, "spell.effect.result", tags);

            spellEffectCounter.Add(1, GetEffectMetricTags(spell, effect, result.ToString()));
        }

        public void RecordEffectHandlerResult(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, SpellEffectExecutionResult result, string handlerName)
        {
            ISpell spell = executionContext?.Spell;
            if (spell == null)
                return;

            if (!Options.IncludeEffectResults || !ShouldRecord(spell))
                return;

            using Activity activity = SpellDiagnosticsTelemetry.ActivitySource.StartActivity("spell.effect.handler", ActivityKind.Internal);
            TagList tags = GetEffectTags(spell, info.Entry);
            tags.Add("nf.spell.effect.result", result.ToString());
            tags.Add("nf.spell.effect.handler", handlerName);
            tags.Add("nf.spell.effect.target_kind", target?.GetType().Name ?? "");
            if (activity != null)
            {
                foreach (KeyValuePair<string, object> tag in tags)
                    activity.SetTag(tag.Key, tag.Value);
            }

            if (result != SpellEffectExecutionResult.Ok)
                log.LogInformation("Spell diagnostic effect handler result: spell4={Spell4Id} effect={SpellEffectId} effectType={EffectType} handler={Handler} result={Result} class={PlayerClass} player={PlayerName} test={TestLabel}",
                    spell.Parameters.SpellInfo.Entry.Id,
                    info.Entry.Id,
                    info.Entry.EffectType,
                    handlerName,
                    result,
                    GetPlayerClass(spell),
                    GetPlayerName(spell),
                    GetActiveSession(spell)?.Label);
        }

        public void RecordEffectHandlerMissing(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, string reason)
        {
            ISpell spell = executionContext?.Spell;
            if (spell == null)
                return;

            if (!ShouldRecord(spell))
                return;

            TagList tags = GetEffectTags(spell, info.Entry);
            tags.Add("nf.spell.effect.result", "missing_handler");
            tags.Add("nf.spell.effect.missing_reason", reason);
            AddEvent(Activity.Current, "spell.effect.missing_handler", tags);

            spellEffectCounter.Add(1, GetEffectMetricTags(spell, info.Entry, "missing_handler", reason));
            log.LogInformation("Spell diagnostic effect missing handler: spell4={Spell4Id} effect={SpellEffectId} effectType={EffectType} reason={Reason} class={PlayerClass} player={PlayerName} test={TestLabel}",
                spell.Parameters.SpellInfo.Entry.Id,
                info.Entry.Id,
                info.Entry.EffectType,
                reason,
                GetPlayerClass(spell),
                GetPlayerName(spell),
                GetActiveSession(spell)?.Label);
        }

        public void RecordPrerequisiteResult(IPlayer player, uint prerequisiteId, PrerequisiteType type, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters, bool met)
        {
            if (!Options.Enable || (met && !Options.IncludePrerequisiteSuccess))
                return;

            bool activeTest = activeSessions.ContainsKey(player.CharacterId);
            bool spellMatches = parameters?.SpellInfo != null && ShouldRecord(player, parameters.SpellInfo.Entry.Id, parameters.SpellInfo.Entry.Spell4BaseIdBaseSpell);
            if (!activeTest && !Options.TraceAll && !spellMatches)
                return;

            TagList tags = new();
            tags.Add("nf.prerequisite.id", prerequisiteId);
            tags.Add("nf.prerequisite.type", type.ToString());
            tags.Add("nf.prerequisite.comparison", comparison.ToString());
            tags.Add("nf.prerequisite.value", value);
            tags.Add("nf.prerequisite.object_id", objectId);
            tags.Add("nf.prerequisite.met", met);
            tags.Add("nf.player.class", player.Class.ToString());
            tags.Add("nf.player.name", player.Name);
            AddSpellInfoTags(tags, parameters?.SpellInfo);
            AddSessionTags(tags, GetActiveSession(player));

            AddEvent(Activity.Current, "spell.prerequisite.result", tags);
            spellPrerequisiteCounter.Add(1, GetPrerequisiteMetricTags(player, type, met));

            if (!met)
                log.LogInformation("Spell diagnostic prerequisite failed: prerequisite={PrerequisiteId} type={Type} comparison={Comparison} value={Value} object={ObjectId} castResult={CastResult} spell4={Spell4Id} base={Spell4BaseId} tier={Tier} class={PlayerClass} player={PlayerName} test={TestLabel}",
                    prerequisiteId,
                    type,
                    comparison,
                    value,
                    objectId,
                    parameters?.CastResult,
                    parameters?.SpellInfo?.Entry.Id,
                    parameters?.SpellInfo?.Entry.Spell4BaseIdBaseSpell,
                    parameters?.SpellInfo?.Entry.TierIndex,
                    player.Class,
                    player.Name,
                    GetActiveSession(player)?.Label);
        }

        public void RecordValidatorFailure(ISpell spell, string validatorName, CastResult result)
        {
            if (!ShouldRecord(spell))
                return;

            TagList tags = GetSpellTags(spell, includeCastId: true, includePlayer: false);
            tags.Add("nf.spell.validator", validatorName);
            tags.Add("nf.spell.cast_result", result.ToString());
            AddEvent(Activity.Current, "spell.validator.failure", tags);
        }

        public void RecordChatMessage(IPlayer player, ChatChannelType channel, string message)
        {
            if (!Options.Enable || !Options.ChatMarkers)
                return;

            if (SpellDiagnosticChatMarker.TryParse(message, out SpellDiagnosticChatMarker marker))
            {
                if (marker.Action == SpellDiagnosticChatMarkerAction.Start)
                {
                    SpellDiagnosticSession session = new()
                    {
                        Id        = $"{player.CharacterId:x}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds():x}",
                        Label     = marker.Label,
                        StartedAt = DateTimeOffset.UtcNow
                    };
                    activeSessions[player.CharacterId] = session;
                    spellTestMarkerCounter.Add(1, GetTestMarkerTags(player, "start", channel, session));
                    log.LogInformation("Spell diagnostic test started: test={TestId} label={TestLabel} channel={Channel} class={PlayerClass} player={PlayerName}",
                        session.Id,
                        session.Label,
                        channel,
                        player.Class,
                        player.Name);
                }
                else
                {
                    activeSessions.TryRemove(player.CharacterId, out SpellDiagnosticSession session);
                    spellTestMarkerCounter.Add(1, GetTestMarkerTags(player, "end", channel, session, marker.Label));
                    log.LogInformation("Spell diagnostic test ended: test={TestId} label={TestLabel} requestedLabel={RequestedLabel} channel={Channel} class={PlayerClass} player={PlayerName}",
                        session?.Id,
                        session?.Label,
                        marker.Label,
                        channel,
                        player.Class,
                        player.Name);
                }

                return;
            }

            SpellDiagnosticSession activeSession = GetActiveSession(player);
            if (activeSession == null)
                return;

            log.LogInformation("Spell diagnostic test note: test={TestId} label={TestLabel} channel={Channel} class={PlayerClass} player={PlayerName} note={Note}",
                activeSession.Id,
                activeSession.Label,
                channel,
                player.Class,
                player.Name,
                Truncate(message, Options.MaxChatMessageLength));
        }

        private bool ShouldRecord(ISpell spell)
        {
            if (!Options.Enable)
                return false;

            if (GetActiveSession(spell) != null)
                return true;

            if (Options.TraceAll)
                return true;

            return ShouldRecord(GetPlayer(spell), spell.Parameters.SpellInfo.Entry.Id, spell.Parameters.SpellInfo.Entry.Spell4BaseIdBaseSpell);
        }

        private bool ShouldRecord(IPlayer player, uint spell4Id, uint spell4BaseId)
        {
            if (Options.Spell4Ids.Contains(spell4Id))
                return true;

            if (Options.Spell4BaseIds.Contains(spell4BaseId))
                return true;

            if (player != null && Options.PlayerNames.Any(n => string.Equals(n, player.Name, StringComparison.OrdinalIgnoreCase)))
                return true;

            return false;
        }

        private TagList GetSpellTags(ISpell spell, bool includeCastId, bool includePlayer)
        {
            TagList tags = new();
            AddSpellInfoTags(tags, spell.Parameters.SpellInfo);
            tags.Add("nf.spell.cast_method", spell.CastMethod.ToString());
            tags.Add("nf.spell.proxy", spell.Parameters.IsProxy);
            tags.Add("nf.spell.user_initiated", spell.Parameters.UserInitiatedSpellCast);
            if (includeCastId)
                tags.Add("nf.spell.casting_id", spell.CastingId);

            SpellDiagnosticSession session = GetActiveSession(spell);
            AddSessionTags(tags, session);

            if (includePlayer && GetPlayer(spell) is IPlayer player)
            {
                tags.Add("nf.player.class", player.Class.ToString());
                tags.Add("nf.player.name", player.Name);
                tags.Add("nf.character.id", player.CharacterId);
            }

            return tags;
        }

        private static void AddSpellInfoTags(TagList tags, Abstract.Spell.Info.ISpellInfo spellInfo)
        {
            if (spellInfo == null)
                return;

            tags.Add("nf.spell.id", spellInfo.Entry.Id);
            tags.Add("nf.spell.base_id", spellInfo.Entry.Spell4BaseIdBaseSpell);
            tags.Add("nf.spell.tier", spellInfo.Entry.TierIndex);
        }

        private TagList GetEffectTags(ISpell spell, Spell4EffectsEntry effect)
        {
            TagList tags = GetSpellTags(spell, includeCastId: true, includePlayer: false);
            tags.Add("nf.spell.effect.id", effect.Id);
            tags.Add("nf.spell.effect.type", effect.EffectType.ToString());
            tags.Add("nf.spell.effect.target_flags", effect.TargetFlags.ToString());
            tags.Add("nf.spell.effect.delay_ms", effect.DelayTime);
            tags.Add("nf.spell.effect.tick_ms", effect.TickTime);
            return tags;
        }

        private TagList GetMetricTags(ISpell spell, params (string Key, string Value)[] extraTags)
        {
            TagList tags = new();
            tags.Add("spell4_id", spell.Parameters.SpellInfo.Entry.Id);
            tags.Add("spell4_base_id", spell.Parameters.SpellInfo.Entry.Spell4BaseIdBaseSpell);
            tags.Add("tier", spell.Parameters.SpellInfo.Entry.TierIndex);
            tags.Add("cast_method", spell.CastMethod.ToString());
            tags.Add("player_class", GetPlayerClass(spell)?.ToString() ?? "");
            foreach ((string key, string value) in extraTags)
                tags.Add(key, value);
            return tags;
        }

        private TagList GetEffectMetricTags(ISpell spell, Spell4EffectsEntry effect, string result, string reason = null)
        {
            TagList tags = GetMetricTags(spell, ("result", result));
            tags.Add("effect_type", effect.EffectType.ToString());
            if (!string.IsNullOrWhiteSpace(reason))
                tags.Add("reason", reason);
            return tags;
        }

        private TagList GetTestMarkerTags(IPlayer player, string action, ChatChannelType channel, SpellDiagnosticSession session, string label = null)
        {
            TagList tags = new();
            tags.Add("action", action);
            tags.Add("channel", channel.ToString());
            tags.Add("player_class", player.Class.ToString());
            tags.Add("test_label", session?.Label ?? label ?? "");
            return tags;
        }

        private static TagList GetPrerequisiteMetricTags(IPlayer player, PrerequisiteType type, bool met)
        {
            TagList tags = new();
            tags.Add("prerequisite_type", type.ToString());
            tags.Add("met", met.ToString());
            tags.Add("player_class", player.Class.ToString());
            return tags;
        }

        private static void AddEvent(Activity activity, string name, TagList tags)
        {
            activity?.AddEvent(new ActivityEvent(name, tags: ToActivityTags(tags)));
        }

        private static ActivityTagsCollection ToActivityTags(TagList tags)
        {
            ActivityTagsCollection activityTags = new();
            foreach (KeyValuePair<string, object> tag in tags)
                activityTags.Add(tag.Key, tag.Value);

            return activityTags;
        }

        private void AddSessionTags(TagList tags, SpellDiagnosticSession session)
        {
            if (session == null)
                return;

            tags.Add("nf.spell_test.id", session.Id);
            tags.Add("nf.spell_test.label", session.Label);
        }

        private SpellDiagnosticSession GetActiveSession(ISpell spell)
        {
            return GetPlayer(spell) is IPlayer player ? GetActiveSession(player) : null;
        }

        private SpellDiagnosticSession GetActiveSession(IPlayer player)
        {
            if (player == null)
                return null;

            return activeSessions.TryGetValue(player.CharacterId, out SpellDiagnosticSession session) ? session : null;
        }

        private static IPlayer GetPlayer(ISpell spell)
        {
            return spell.Caster as IPlayer;
        }

        private static Class? GetPlayerClass(ISpell spell)
        {
            return GetPlayer(spell)?.Class;
        }

        private static string GetPlayerName(ISpell spell)
        {
            return GetPlayer(spell)?.Name;
        }

        private SpellDiagnosticsOptions Options => options.Value ?? new SpellDiagnosticsOptions();

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || maxLength <= 0 || value.Length <= maxLength)
                return value;

            return value[..maxLength];
        }
    }
}
