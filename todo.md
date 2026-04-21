# Spell System TODO

Goal: make spell work easy to pick up in small wins first, while still tracking the deeper combat, target, aura, proxy, and missing-effect work.

Audit date: 2026-04-21

Scope checked:

- `Source/NexusForever.Game/Spell/**`
- `Source/NexusForever.Game/Entity/SpellManager.cs`
- `Source/NexusForever.Game/Entity/UnitEntity.cs`
- `Source/NexusForever.Game/Combat/DamageCalculator.cs`
- `Source/NexusForever.WorldServer/Network/Message/Handler/Spell/**`
- `Source/NexusForever.Game.Static/Spell/SpellEffectType.cs`

Current inventory:

- Implemented spell effect handlers: 28.
- Non-`UNUSED` spell effect enum values: 136.
- Missing effect handlers: 108.
- No dedicated spell tests were found, so every fix below should include a small regression test once a test project exists.

Priority legend:

- `P0`: likely broken at runtime or blocks whole spell categories.
- `P1`: localized fix with high gameplay impact.
- `P2`: partial/stubbed behavior that needs data research or packet validation.
- `P3`: larger systems work.

## Start Here: Smallest High-Impact Fixes

1. `P0` Fix loaded and updated spell tiers.
   File: `Source/NexusForever.Game/Spell/CharacterSpell.cs:40`
   Problem: the `Tier` setter calls `BaseInfo.GetSpellInfo(tier)` before assigning the new value, so it reloads the old tier. The DB constructor also calls `baseInfo.GetSpellInfo(tier)` before `tier = model.Tier`, so loaded character spells appear to use tier 1 spell data even when the saved tier is higher.
   First fix: assign `tier` first or call `BaseInfo.GetSpellInfo(value)`, and load `model.Tier` before resolving `SpellInfo`.

2. `P0` Stop mutating active spell dictionaries while iterating.
   File: `Source/NexusForever.Game/Entity/UnitEntity.cs:150`
   Problem: `spells.Remove(spell.CastingId)` runs inside `foreach (ISpell spell in spells.Values)`, which can throw `InvalidOperationException` as soon as a spell finishes during update.
   First fix: collect finished spell ids into a list and remove them after the loop.

3. `P0` Stop mutating delayed effect dictionaries while iterating.
   File: `Source/NexusForever.Game/Spell/Spell.cs:154`
   Problem: `delayedEffects.Remove(effect)` happens inside a `foreach` over `delayedEffects`.
   First fix: collect elapsed effects, remove after iteration, then execute them.

4. `P0` Implement or disable `SpellChanneledField`.
   File: `Source/NexusForever.Game/Spell/Type/SpellChanneledField.cs:10`
   Problem: this class only inherits base `Spell.Cast()`. It never sets `status = Casting`, never schedules `Execute()`, and likely stays in `Initiating` forever.
   First fix: either implement channel-field timing based on `SpellChanneled`, or map this cast method to a safe fallback until real behavior is known.

5. `P0` Fix multiphase completion.
   File: `Source/NexusForever.Game/Spell/Type/SpellMultiphase.cs:49`
   Problem: the lambda checks `if (i == Parameters.SpellInfo.Phases.Count - 1)` but captures the loop variable `i`. After the loop, this condition will not represent the phase that scheduled the event. The unused `index` local was probably intended for this.
   First fix: use the captured `index` local in the lambda.

6. `P0` Fix proxy tick scheduling.
   File: `Source/NexusForever.Game/Spell/Proxy.cs:67`
   Problem: `for (int i = 1; i >= Data.Entry.DurationTime / tickTime; i++)` almost never runs. `TickingEvent()` recursively creates a new event but never enqueues or returns it to the scheduler.
   First fix: change the bounded loop to `<=`, and make repeating proxy ticks explicitly re-enqueue the next event.

7. `P0` Add missing `TitleGrant` data implementation and DI registration.
   Files: `Source/NexusForever.Game/Spell/Effect/Handler/SpellEffectTitleGrantHandler.cs:12`, `Source/NexusForever.Game/Spell/Effect/ServiceCollectionExtensions.cs:15`
   Problem: `SpellEffectTitleGrantHandler` requires `ISpellEffectTitleGrantData`, but no concrete `SpellEffectTitleGrantData` class exists and the interface is not registered. The handler will resolve no data and never grant titles.
   First fix: add `SpellEffectTitleGrantData`, populate `TitleId` from the correct data bit, and register it.

8. `P1` Fix vanity pet unlock packet and null handling.
   File: `Source/NexusForever.Game/Spell/Effect/Handler/SpellEffectUnlockVanityPetHandler.cs:40`
   Problem: vanity pet unlock sends `ServerUnlockMount`. It also does not null-check the `Spell4Entry` before using it.
   First fix: send the correct vanity-pet unlock packet if one exists, or document/client-verify the expected packet. Add null/duplicate guards.

9. `P1` Fix vanity pet despawn state.
   File: `Source/NexusForever.Game/Spell/Effect/Handler/SpellEffectSummonVanityPetHandler.cs:47`
   Problem: removed vanity pets set `player.VanityPetGuid = 0u` instead of `null`, but other call sites check `!= null`, so later code can keep trying to interact with guid `0`.
   First fix: set `VanityPetGuid = null`.

10. `P1` Report actual remaining effect duration.
    File: `Source/NexusForever.Game/Spell/Target/SpellTargetEffectInfo.cs:100`
    Problem: `Build()` and `ServerSpellUpdateEffectDuration` use `duration.Duration`, not `duration.Time`, so the client receives the original duration instead of remaining time.
    First fix: use `duration.Time` for remaining time.

11. `P1` Fix rapid transport rotation.
    File: `Source/NexusForever.Game/Spell/Effect/Handler/SpellEffectRapidTransportHandler.cs:85`
    Problem: quaternion construction uses `Facing0` twice and skips `Facing1`.
    First fix: use `(Facing0, Facing1, Facing2, Facing3)`.

12. `P1` Dispose spells that fail during cast setup.
    File: `Source/NexusForever.Game/Entity/UnitEntity.cs:443`
    Problem: `spell.Initialise()` creates script collections. If `spell.Cast()` returns false or the spell fails before being added to `pendingSpells`, it is not disposed.
    First fix: call `spell.Dispose()` before returning on failed cast paths.

## Combat and Effect Correctness

1. `P1` Check damage eligibility from caster to target, not target to caster.
   File: `Source/NexusForever.Game/Spell/Effect/Handler/SpellEffectDamageHandler.cs:39`
   Problem: damage checks `target.CanAttack(executionContext.Spell.Caster)`. The readable intent is `caster.CanAttack(target)`. This is especially risky around one-way validity rules, scripted factions, duels, and pets.
   First fix: switch the call direction and validate duel/PvE behavior.

2. `P1` Fix physical mitigation property.
   File: `Source/NexusForever.Game/Combat/DamageCalculator.cs:268`
   Problem: physical damage adds `DamageMitigationPctOffsetMagic`; `Property.DamageMitigationPctOffsetPhysical` exists and is probably intended.
   First fix: use the physical offset for `DamageType.Physical`.

3. `P1` Add a real `Heal` effect handler.
   Missing type: `SpellEffectType.Heal`
   Problem: `DamageCalculator` already has heal-aware formula branches and `CombatLogHeal` exists, but no `Heal` handler is registered. Healing spells currently hit the no-handler path.
   First fix: mirror the damage calculation path with `DamageType.Heal`, call `ModifyHealth(..., DamageType.Heal, caster)`, emit `CombatLogHeal`, and update public event healing stats if available.

4. `P1` Add shield heal/damage handlers.
   Missing types: `HealShields`, `DamageShields`
   Problem: shield-related combat math and vitals exist, but these effect types have no handlers.
   First fix: implement direct shield delta with clamps and combat logs. These should be easier than full damage because they can avoid target health/death flow initially.

5. `P1` Improve `VitalModifier`.
   File: `Source/NexusForever.Game/Spell/Effect/Handler/SpellEffectVitalModifierHandler.cs:14`
   Problem: directly calls `ModifyVital` with no combat log, no sign/data validation, and no clamping policy beyond the underlying vital setter.
   First fix: add `CombatLogVitalModifier`, validate signed/float interpretation of `DataBits01`, and confirm whether some effects use negative values encoded in uint bits.

6. `P2` Complete damage calculator combat mechanics.
   File: `Source/NexusForever.Game/Combat/DamageCalculator.cs`
   Gaps: strikethrough, armor pierce, multi-hit, lifesteal, reflect, crit deflect, critical mitigation, glance combat logging, defensive modifiers, proc queueing, and robust null handling when a `GameFormulaEntry` is missing.
   First fix: add focused tests around current damage math before changing formulas.

7. `P2` Replace `new Random()` in combat chance checks.
   File: `Source/NexusForever.Game/Combat/DamageCalculator.cs:280`
   Problem: repeated `new Random()` can create correlated rolls under high-frequency combat.
   First fix: use `Random.Shared` or inject an RNG for deterministic tests.

## Targeting, Prerequisites, and Lifecycle

1. `P0` Decide how threshold data maps to spell classes.
   File: `Source/NexusForever.Game/Spell/Spell.cs:115`
   Problem: any non-`SpellThreshold` spell with threshold rows throws `NotImplementedException` during initialise.
   First fix: log/report affected spell ids from game tables, then either support those cast methods or prevent them cleanly without crashing.

2. `P1` Apply target cast and persistence prerequisites.
   File: `Source/NexusForever.Game/Spell/Spell.cs:321`
   Problem: target cast prerequisites, caster persistence prerequisites, and target persistence prerequisites are loaded but empty or partially implemented. Target persistence is explicitly TODO.
   First fix: evaluate explicit target first. If prerequisite system only supports players, add safe non-player behavior and log unsupported cases.

3. `P1` Fix target count ordering.
   File: `Source/NexusForever.Game/Spell/Target/Implicit/Filter/SpellTargetImplicitConstraintFilter.cs:25`
   Problem: target count is applied before `OrderForSelectionType()`, so `Closest`, `Furthest`, `Random`, and health-based selection can cull the wrong units.
   First fix: filter range/angle first, sort, then apply `TargetCount`.

4. `P1` Implement AOE angle constraints.
   File: `Source/NexusForever.Game/Spell/Target/Implicit/Filter/SpellTargetImplicitConstraintFilter.cs:45`
   Problem: `constraints.Angle` is checked but ignored.
   First fix: calculate angle from caster/telegraph facing to candidate target and mark `AngleConstraintFailed`.

5. `P2` Use target mechanics for implicit target search positions.
   File: `Source/NexusForever.Game/Spell/Target/Implicit/SpellTargetImplicitSelector.cs:51`
   Problem: selector always searches around `initialPosition`; target type flags are not used to derive primary-target, caster, cone, or field origins.
   First fix: implement the most common target mechanics first: self, primary target, caster position, and positional unit.

6. `P2` Add valid target, hit result, caster/target condition, and CC-condition validators.
   Files: `Source/NexusForever.Game/Spell/Info/SpellBaseInfo.cs`, `Source/NexusForever.Game/Spell/Info/SpellInfo.cs`, `Source/NexusForever.Game/Spell/Validator/**`
   Problem: data is loaded but much of it is not enforced in `CheckCast()` or effect application.
   First fix: add validator classes rather than growing `Spell.CheckCast()`.

7. `P2` Handle moving/rotating telegraphs.
   File: `Source/NexusForever.Game/Spell/Spell.cs:238`
   Problem: NPC telegraphs are initialized as if non-player casters stand still. Comments note moving/rotating unit telegraphs are wrong.
   First fix: add a telegraph attachment/update path for unit-attached telegraphs.

8. `P2` Fix `SpellBaseInfo` tier storage assumptions.
   File: `Source/NexusForever.Game/Spell/Info/SpellBaseInfo.cs`
   Problem: the comment says spell tiers are not always sequential, but the array size uses `spellEntries[0].TierIndex` and lookup is `tier - 1`.
   First fix: sort entries and size by max tier, or use a dictionary keyed by tier.

9. `P2` Fix threshold cache ordering.
   File: `Source/NexusForever.Game/Spell/Info/SpellInfo.cs`
   Problem: `thresholdCache.Last()` depends on dictionary enumeration order, and `Thresholds[index]` assumes order index equals list index.
   First fix: order by `OrderIndex` and store both `SpellInfo` and `Spell4ThresholdsEntry` together.

## Implemented Handlers That Are Partial or Suspicious

1. `P1` `SpellEffectForcedMoveHandler`
   File: `Source/NexusForever.Game/Spell/Effect/Handler/SpellEffectForcedMoveHandler.cs`
   Problems: several move types and flags are TODO/no-op, velocity movement does not compute velocity, terrain handling is approximate, and `Unknown11` through `Unknown15` can return `Ok` without movement.
   First fix: finish known flag handling for `Target`, `Facing`, and velocity movement, then add data-driven examples for pull/knockback/jump spells.

2. `P1` `SpellEffectModifySpellCooldownHandler`
   File: `Source/NexusForever.Game/Spell/Effect/Handler/SpellEffectModifySpellCooldownHandler.cs`
   Problems: only `Spell4` and `SpellCooldownId` are handled. Ability charge timers explicitly say they are not adjusted by cooldown modification.
   First fix: connect cooldown modification to `CharacterSpell` charge recharge timers or move charge state into `SpellManager`.

3. `P1` `SpellEffectSummonMountHandler`
   File: `Source/NexusForever.Game/Spell/Effect/Handler/SpellEffectSummonMountHandler.cs`
   Problems: hard-coded follow-up casts `52539` and `80530`, no NPC mounting, and no duplicate/previous mount cleanup in the handler.
   First fix: replace hard-coded follow-up spells with data-driven mount aura/passive behavior or document why they are universal.

4. `P2` `SpellEffectFluffHandler`
   File: `Source/NexusForever.Game/Spell/Effect/Handler/SpellEffectFluffHandler.cs`
   Problem: pure no-op. This may be acceptable if visuals are entirely client-side, but it should be documented with known spell ids.
   First fix: add a comment/report list of fluff spell ids and confirm no server state is expected.

5. `P2` `SpellEffectFullScreenEffectHandler`
   File: `Source/NexusForever.Game/Spell/Effect/Handler/SpellEffectFullScreenEffectHandler.cs`
   Problem: pure no-op despite the name implying a client-visible effect.
   First fix: find the matching network packet or confirm `ServerSpellGo` carries enough visual data.

6. `P2` `SpellEffectTeleportHandler`
   File: `Source/NexusForever.Game/Spell/Effect/Handler/SpellEffectTeleportHandler.cs`
   Problems: invalid location, non-player target, and failed `CanTeleport()` all return `Ok` silently. Housing branch creates residences during spell handling.
   First fix: return `PreventEffect` or emit debug logs for failed teleport paths; split housing teleport into a helper.

7. `P2` `SpellEffectUnlockMountHandler`
   File: `Source/NexusForever.Game/Spell/Effect/Handler/SpellEffectUnlockMountHandler.cs`
   Problems: no null check for spell entry and duplicate `AddSpell()` can throw.
   First fix: guard missing/known spells and return a clear result.

8. `P2` `SpellEffectProcHandler` and proc runtime
   Files: `Source/NexusForever.Game/Spell/Effect/Handler/SpellEffectProcHandler.cs`, `Source/NexusForever.Game/Spell/Proc/**`
   Problems: prerequisites only work when target/caster is a player, trigger scheduling is timer based and sparse, and proc types are only triggered from a few places.
   First fix: inventory `ProcType` values and add trigger call sites for damage taken, heal, deflect, kill, shield, interrupt, and movement events.

## Missing Effect Handlers: Easiest-Looking First

These appear lower effort because nearby systems or packets already exist.

- `Heal`
- `HealShields`
- `DamageShields`
- `CooldownReset`
- `ActivateSpellCooldown`
- `ModifyAbilityCharges`
- `AddSpell`
- `SpellForceRemoveChanneled`
- `TitleRevoke`
- `GrantXP`
- `GrantLevelScaledXP`
- `GrantLevelScaledPrestige`
- `PathXpModify`
- `PathMissionIncrement`
- `AchievementAdvance`
- `QuestAdvanceObjective`
- `GiveAbilityPointsToPlayer`
- `GiveItemToPlayer`
- `GiveLootTableToPlayer`
- `ReputationModify`
- `VacuumLoot`
- `Kill`
- `SetBusy`
- `Disembark`
- `HousingEscape`
- `SupportStuck`
- `MiniMapIcon`

Suggested first batch:

1. `Heal`
2. `HealShields`
3. `CooldownReset`
4. `ModifyAbilityCharges`
5. `TitleRevoke`
6. `GrantXP`
7. `AchievementAdvance`
8. `QuestAdvanceObjective`
9. `GiveItemToPlayer`
10. `Kill`

## Missing Effect Handlers: Medium Effort

These need more gameplay rules, combat math, or existing-manager integration.

- `Absorption`
- `HealingAbsorption`
- `ClampVital`
- `SapVital`
- `ShieldOverload`
- `SpellCounter`
- `SpellDispel`
- `SpellEffectImmunity`
- `SpellImmunity`
- `AggroImmune`
- `ThreatModification`
- `ThreatTransfer`
- `UnitStateSet`
- `ForceFacing`
- `NpcForceFacing`
- `NPCForceAIMovement`
- `PetCastSpell`
- `SummonPet`
- `SummonCreature`
- `SummonTrap`
- `DespawnUnit`
- `HazardEnable`
- `HazardModify`
- `HazardSuspend`
- `PathActionExplorerDig`
- `SettlerCampfire`
- `HousingPlantSeed`
- `RestedXpDecorBonus`
- `ModifyRestedXP`
- `RewardBuffModifier`
- `RewardPropertyModifier`
- `VendorPriceModifier`
- `SetMatchingEligibility`
- `TemporarilyUnflagPvp`
- `DisallowPvP`

## Missing Effect Handlers: Hard / Needs Design

These likely require larger architecture, scripting, content research, or major packet work.

- `Script`
- `ModifySpell`
- `ModifySpellEffect`
- `AddSpellEffect`
- `SuppressSpellEffect`
- `ProxyLinearAE`
- `ProxyChannel`
- `ProxyChannelVariableTime`
- `ProxyRandomExclusive`
- `SummonVehicle`
- `ForcedAction`
- `ChangePhase`
- `ChangePlane`
- `GoMap`
- `ReturnMap`
- `WarplotTeleport`
- `WarplotPlugUpgrade`
- `FacilityModification`
- `CraftItem`
- `TradeSkillProfession`
- `GiveSchematic`
- `GiveAugmentPowerToPlayer`
- `UnlockInlaidAugment`
- `UnlockActionBar`
- `ActionBarSet`
- `ApplyLASChanges`
- `ModifyCreatureFlags`
- `ChangeDisplayName`
- `MimicDisplayName`
- `MimicDisguise`
- `DisguiseOutfit`
- `ChangeIcon`
- `ItemVisualSwap`
- `RavelSignal`
- `SharedHealthPool`
- `UnitPropertyConversion`
- `PersonalDmgHealMod`
- `DelayDeath`
- `DistanceDependentDamage`
- `DistributedDamage`
- `Scale`
- `Transference`

## Complete Missing Handler List

All non-`UNUSED` enum values without a handler as of this audit:

`Absorption`, `AchievementAdvance`, `ActionBarSet`, `ActivateSpellCooldown`, `AddSpell`, `AddSpellEffect`, `AggroImmune`, `ApplyLASChanges`, `ChangeDisplayName`, `ChangeIcon`, `ChangePhase`, `ChangePlane`, `ClampVital`, `CooldownReset`, `CraftItem`, `DamageShields`, `DelayDeath`, `DespawnUnit`, `DisallowPvP`, `Disembark`, `DisguiseOutfit`, `DistanceDependentDamage`, `DistributedDamage`, `FacilityModification`, `ForceFacing`, `ForcedAction`, `GiveAbilityPointsToPlayer`, `GiveAugmentPowerToPlayer`, `GiveItemToPlayer`, `GiveLootTableToPlayer`, `GiveSchematic`, `GoMap`, `GrantLevelScaledPrestige`, `GrantLevelScaledXP`, `GrantXP`, `HazardEnable`, `HazardModify`, `HazardSuspend`, `Heal`, `HealShields`, `HealingAbsorption`, `HousingEscape`, `HousingPlantSeed`, `HousingTeleport`, `ItemVisualSwap`, `Kill`, `MimicDisguise`, `MimicDisplayName`, `MiniMapIcon`, `ModifyAbilityCharges`, `ModifyCreatureFlags`, `ModifyRestedXP`, `ModifySpell`, `ModifySpellEffect`, `NPCForceAIMovement`, `NpcExecutionDelay`, `NpcForceFacing`, `NpcLootTableModify`, `PathActionExplorerDig`, `PathMissionIncrement`, `PathXpModify`, `PersonalDmgHealMod`, `PetCastSpell`, `ProxyChannel`, `ProxyChannelVariableTime`, `ProxyLinearAE`, `ProxyRandomExclusive`, `QuestAdvanceObjective`, `RavelSignal`, `ReputationModify`, `RestedXpDecorBonus`, `ReturnMap`, `RewardBuffModifier`, `RewardPropertyModifier`, `SapVital`, `Scale`, `Script`, `SetBusy`, `SetMatchingEligibility`, `SettlerCampfire`, `SharedHealthPool`, `ShieldOverload`, `SpellCounter`, `SpellDispel`, `SpellEffectImmunity`, `SpellForceRemoveChanneled`, `SpellImmunity`, `SummonCreature`, `SummonPet`, `SummonTrap`, `SummonVehicle`, `SupportStuck`, `SuppressSpellEffect`, `TemporarilyUnflagPvp`, `ThreatModification`, `ThreatTransfer`, `TitleRevoke`, `TradeSkillProfession`, `Transference`, `UnitPropertyConversion`, `UnitStateSet`, `UnlockActionBar`, `UnlockInlaidAugment`, `VacuumLoot`, `VectorSlide`, `VendorPriceModifier`, `WarplotPlugUpgrade`, `WarplotTeleport`.

## Regression Tests To Add Before Big Refactors

1. Reflection test: every `[SpellEffectHandler]` generic data interface has a concrete registered data implementation.
2. Reflection test: report effect enum values with no handler, but allow an explicit ignored list.
3. Character spell tier test: constructing from DB model tier 4 resolves tier 4 `SpellInfo`.
4. Spell update test: finishing a spell during `UnitEntity.Update()` does not mutate the active dictionary during enumeration.
5. Delayed effect test: delayed effects execute once after delay and do not mutate `delayedEffects` during enumeration.
6. Proxy periodic test: finite duration/tick schedules the expected number of child casts.
7. Multiphase test: final phase transitions the spell to finishing/finished.
8. Duration packet test: `EffectInfo.TimeRemaining` decreases after update.
9. Damage direction test: caster-target faction/duel rules are checked from caster to target.
10. Handler smoke tests for the first easy batch: `Heal`, `HealShields`, `CooldownReset`, `ModifyAbilityCharges`, `TitleRevoke`, `GrantXP`, `AchievementAdvance`, `QuestAdvanceObjective`, `GiveItemToPlayer`, and `Kill`.

## Suggested Work Order

1. Fix the P0 lifecycle/runtime exceptions first: spell tiers, dictionary mutation, channeled field, multiphase completion, proxy ticks, and TitleGrant data.
2. Fix the P1 one-line or localized correctness bugs: vanity pet packet/state, rapid transport rotation, remaining duration, damage direction, and physical mitigation.
3. Add reflection/regression tests so new handlers do not silently resolve to `NoHandler`.
4. Implement the first easy missing handlers, starting with `Heal` and cooldown/charge effects.
5. Move into target/prerequisite correctness, because many “spell feels wrong” bugs will come from target selection rather than individual handlers.
6. Tackle hard proxy/scripting/summon/vehicle/warplot effects after the basic combat and reward handlers are stable.
