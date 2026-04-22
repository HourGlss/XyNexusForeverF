# NexusTogether Completion TODO

Goal: track unfinished gameplay/server work by the source data needed to finish it safely. Prefer small, testable fixes when behavior is obvious from existing code. When behavior depends on WildStar client/server semantics, collect source data first instead of guessing.

Audit date: 2026-04-22

## Source Data To Collect First

These unlock the most future work:

- Client/server packet captures for login, map transfer, chat, combat, spell casts, housing visits, path missions, vendors, store purchases, resurrection, movement splines, and common error cases.
- Full matching game-table extracts for the systems being touched, especially `Spell4*`, prerequisites, target mechanics, `GameFormula`, `Creature2`, `Item2`, `ItemDisplay`, `WorldSocket`, path mission tables, housing plug/decor tables, reward tracks, storefront tables, and achievement/objective tables.
- Known-good database rows from a working branch/server for world entities, public event entities, path mission persistence, account rewards, store purchases, housing, guild/community residences, and character spell/action-set state.
- Before/after state snapshots for player inventory, currencies, reputation, titles, achievements, quest objectives, path missions, path XP, account unlocks, and spell cooldowns when using representative spells or commands.
- Combat logs and packet traces for damage, healing, shields, CC, interrupts, procs, channel ticks, proxy spells, forced movement, death, resurrection, threat, pets, summons, vehicles, and stealth.
- Branch references with exact commit/branch names and test notes from Krakal, kirmmin, derdotte, Googletone, and any other known working forks.
- A small regression-test harness or smoke-test scripts for handlers, packet serialization, map load, chat format round trips, spell effect dispatch, and database migration startup.

## Things Still Safe To Do Without More Data

These are mostly mechanical hardening or wiring gaps where the intended behavior is already visible in code:

- Add null guards around factory-created entities before map/public-event spawn loops call `Initialise()`.
- Replace packet-facing `NotImplementedException` paths with `InvalidPacketValueException` or a client result packet when an enum value is unsupported.
- Add reflection smoke checks for DI registrations: chat formatters, spell effect handlers, movement command handlers, entity models, prerequisite checks, and keyed packet models.
- Add round-trip tests for chat format conversion and action-set shortcut validation.
- Add map-load smoke tests that instantiate every `EntityType` with a minimal model and assert `BuildEntityModel()` does not throw.
- Add spell handler smoke tests for already implemented reward/cooldown/objective handlers using fake managers and table rows.

## Spell System Data Needs

Targeting and prerequisites:

- `Spell4TargetMechanics`, `Spell4Prerequisites`, target flags, explicit target type, implicit target type, telegraph type, and cast-method rows for representative self, target, ground, cone, field, channeled, proxy, and vehicle spells.
- Packet captures for cast success, cast failure, target failure, persistence expiry, target moving out of range, caster moving/rotating during telegraph, and non-player casters.
- Expected `CastResult`/`SpellCastResult` values for failed prerequisites and invalid targets.
- Source examples for caster persistence and target persistence checks during spell lifetime, not only at cast start.

Effect handlers:

- For each missing or partial `SpellEffectType`, collect spell ids, `Spell4Effects` rows, all data-bit meanings, expected target kind, before/after player or unit state, and expected combat log or client packet.
- Reward effects need source rows for inventory/full-bag handling, currency caps, reputation limits, title/achievement/objective updates, path mission increments, account unlocks, and duplicate ownership.
- Cooldown/charge effects need examples for normal cooldowns, global cooldown groups, ability charges, reset behavior, and active recharge timers.
- Summon/pet/vehicle effects need entity type, ownership, despawn rules, seat/passenger data, visuals, spell auras, and cleanup packets.
- Proxy effects need child spell timing, proxy unit placement, tick schedule, target selection, and cancellation behavior.

Combat and procs:

- `GameFormula` rows and live combat logs for each damage type, heal type, shield interaction, glance, deflect, crit deflect, strikethrough, armor pierce, lifesteal, reflect, multi-hit, mitigation offsets, vulnerability, and proc trigger.
- Proc trigger source data for damage dealt, damage taken, heal, shield, interrupt, CC, movement, kill, death, periodic tick, and aura expiration.
- Threat samples for NPC combat, pets, scripted encounters, taunts, threat transfer, and aggro immunity.

## Movement And Map Data Needs

- Packet captures for `SetPositionMultiSpline`, `SetRotationSpline`, `SetRotationMultiSpline`, path movement, spline rotation, negative spline speeds, reverse modes, cyclic modes, and platform/vehicle movement.
- `Spline2` and `Spline2Node` rows for single-spline and multi-spline examples, including takeoff/landing heights, formation data, flags, offsets, blend, and continuation behavior.
- Terrain collision samples for forced movement, jumps, knockbacks, pulls, fall damage, water/hoverboard movement, and map props.
- Return-location data for tutorial/content maps, resurrection holocrypts, default graveyards, and failed instance transfers.

## Entity And Content Data Needs

- World DB rows for every `EntityType`, especially trigger, trap, scanner unit, esper pet, pinata loot, lockbox, structured plug, housing harvest plug, and housing plant.
- Expected entity create packet fields for the above types: owner id, display item id, socket id, active prop id, plug id, current tier, trigger bounds, loot item/count/type, and names.
- Public event phase entity rows with activation/despawn conditions and phase transitions.
- Tutorial branch data for Dominion and Exile tutorial teleports, cinematic return points, and hologram/script interactions.
- Source rows and captures for unsupported path mission types, settler improvement tiers, and path mission reward scaling.

## Housing, Guild, And Community Data Needs

- Residence privacy result packets and client error packets for private, neighbors-only, roommates-only, public, community, missing residence, missing entrance, and invalid visit target.
- Housing plug/decor placement rows tying `WorldSocketId`, `ActivePropId`, decor ids, plug ids, and structure tiers together.
- Roommate/neighbour permission captures for decor updates, plot updates, privacy changes, crate/uncrate, remodel, vendor lists, and community donations.
- Guild/community residence persistence rows and internal message flows once GuildServer behavior is better known.

## Storefront, Rewards, And Account Data Needs

- Store catalog, entitlement, reward-track, purchase, refund, grant, and rotation data from a working source.
- Account transaction rows before and after purchases, failures, duplicate claims, insufficient currency, and service-token spending.
- Reward property and entitlement mappings for account unlocks, character unlocks, costumes, mounts, pets, dyes, titles, AMP/ability grants, and path rewards.
- Packet captures for store UI open, offer list, purchase result, claim result, account item delivery, and error handling.

## Chat, Social, And UI Data Needs

- Chat packet samples for every `ChatFormatType`, including item id, item guid, item full, quest, archive article, nav point, loot, alien, profanity, roleplay, and unknown format values.
- Item link behavior when the sender owns the item, no longer owns the item, links an inventory item by guid, or links static item data by id.
- Social flows for channels, whispers, ignore/block checks, guild/community chat, cross-world relay, and chat server persistence.

## Current Audit Notes

Completed in the 2026-04-22 pass:

- Registered the existing chat formatter implementations instead of only the item/quest subset.
- Added item-guid chat round-trip support and fixed the internal item-guid format type.
- Made missing chat formatter registrations drop unsupported formatting instead of crashing chat conversion.
- Converted several packet-facing unsupported values to invalid-packet handling.
- Added conservative entity models for several entity classes that already had packet models but threw during creation.
- Corrected `PinataLootEntity.Type`.
- Hardened unfinished movement spline/rotation entry points so they fall back safely instead of throwing.
- Fixed the rotation command resynchronization typo from position multi-spline to rotation multi-spline.
- Replaced a housing privacy default throw with public fallback.
- Replaced a costume mannequin save throw with `InvalidMannequinIndex`.
- Made item display fallback return the table display id instead of throwing.
- Stopped the Exile tutorial combat hologram script from throwing for unsupported factions.

High-risk areas intentionally left for source data:

- Full multi-spline movement and spline rotation runtime commands.
- Settler improvement tiers beyond tier 0.
- Unsupported path mission types.
- Housing plug/plant/structured plug packet field correctness beyond conservative ids.
- Trigger bounds and trigger entity model details.
- Holocrypt resurrection behavior and tutorial/content return locations.
- Marketplace auction filters and Who query parameter semantics.
- Deep spell target persistence, proxy behavior, summon/vehicle effects, threat/proc completeness, and combat formula parity.
