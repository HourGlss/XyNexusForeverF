# NexusTogether Milestone TODO

Current milestone: `XYF-1.2`

Goal: build the server to the behavior expected by the analyzed WildStar client.
The `xywikif` SQLite database and Ghidra evidence are authoritative when they
conflict with the current C# server. The C# server remains the implementation
target.

This file keeps the old theory map, but converts it into bite-size milestones.
Spells are first because they touch combat, rewards, action sets, cooldowns,
movement, entities, and PvP. PvP follows after spell packet/result behavior is
less speculative.

## Evidence Rules

- Record useful discoveries in `/home/xyf/NCSOFT/tooling/xywikif/wiki/wiki.sqlite`.
- Link facts to their evidence: asset row, table schema, localized text, UI
  document, Ghidra string/function, packet capture, server code, or runtime
  snapshot.
- Confidence levels are `low`, `medium`, `high`, and `confirmed`.
- Implement behavior when confidence is high enough for the blast radius.
- Use packet captures or runtime before/after snapshots for client-visible
  result values, state transitions, rewards, combat math, and persistence.
- Keep code aligned with `README.md`: gameplay rules in `NexusForever.Game`,
  packet models in `NexusForever.Network.World`, handlers in
  `NexusForever.WorldServer`, and shared constants in `NexusForever.Game.Static`
  or relevant network static folders.

## Milestone Tag Rule

- `XYF-1.2` is the active milestone tag.
- The active tag must appear in the hard-coded server MOTD path and console
  logging output.
- Warnings, errors, and fatal logs must include the active tag without relying on
  operator configuration.
- When the active milestone advances, update the C# milestone constant, the
  NLog console layouts, and this file together.

## Source Data Backlog

Collect these as soon as possible because they unlock multiple milestones:

- Packet captures for login, map transfer, chat, combat, spell casts, duel
  invite/accept/decline/forfeit, battleground queue/match, housing visits, path
  missions, vendors, store purchases, resurrection, movement splines, and common
  client error cases.
- Full game-table extracts for the touched systems, especially `Spell4*`,
  prerequisites, target mechanics, `Spell4CastResult`, `Spell4Thresholds`,
  `GameFormula`, `Creature2`, `Item2`, `ItemDisplay`, `WorldSocket`, path
  mission tables, housing plug/decor tables, reward tracks, storefront tables,
  achievement/objective tables, `Spline2`, and `Spline2Node`.
- Known-good database rows from working branches/servers for world entities,
  public event entities, path mission persistence, account rewards, store
  purchases, housing, guild/community residences, PvP state, and character
  spell/action-set state.
- Before/after snapshots for player inventory, currencies, reputation, titles,
  achievements, quest objectives, path missions, path XP, account unlocks,
  spell cooldowns, duel state, PvP ratings, and battleground rewards.
- Combat logs and packet traces for damage, healing, shields, CC, interrupts,
  procs, channel ticks, proxy spells, forced movement, death, resurrection,
  threat, pets, summons, vehicles, stealth, and PvP kills.
- Branch references with exact commit/branch names and test notes from Krakal,
  kirmmin, derdotte, Googletone, and other known working forks.

## XYF-1.x Spell Milestones

### XYF-1.1 Spell Evidence Baseline

Status: completed 2026-04-23. The active milestone has advanced to `XYF-1.2`.

Purpose: turn the spell theory pile into a ranked implementation map.

Inputs already available:

- Wiki tables: `Spell4` 66383 rows, `Spell4Effects` 131010 rows,
  `Spell4CastResult` 327 rows, `Spell4Thresholds` 622 rows,
  `Spell4TargetMechanics` 61 rows, `Spell4Prerequisites` 11 rows, and
  `GameFormula` 1217 rows.
- Existing facts: `Fact/Spell/ActionSetPacketValidation`,
  `Fact/Spell/GlobalCooldownEnumRange`, and
  `Fact/Spell/ClientSpellAnchorBaseline`.
- Ghidra anchors: `SpellCastFailed`, `AddSpellShortcut`,
  `ClearSpellThreshold`, `CombatLogDamage`, `CombatLogDamageShields`, and the
  broader handoff anchors for combat logs and resurrection.
- Server targets: `Source/NexusForever.Game/Spell/`,
  `Source/NexusForever.WorldServer/Network/Message/Handler/Spell/`, and
  `Source/NexusForever.Network.World/Combat/`.

Deliverables:

- List missing or partial `SpellEffectType` handlers by table frequency, player
  impact, and testability.
- Map cast-result rows to existing `CastResult` values and flag unknown or
  mismatched values.
- Trace `SpellCastFailed`, `AddSpellShortcut`, and `ClearSpellThreshold` xrefs
  far enough to identify the next concrete packet/model questions.
- Add DB facts for any verified cast-result, threshold, action-set, or GCD
  behavior.
- Add or extend smoke tests for spell handler registration, action-set packet
  validation, and safe no-crash dispatch paths.
- Keep unknown target mechanics, effect data bits, proxy behavior, and combat
  formulas as capture-required unless client flow and assets agree.

Exit criteria:

- A ranked spell gap list exists in the DB or this file.
- Each top gap has evidence refs and a server target.
- Tests cover all behavior changed during the milestone.

Completion evidence:

- DB facts: `Fact/Spell/EffectHandlerGapRanking`,
  `Fact/Spell/CastResultEnumParity`,
  `Fact/Spell/ClientSpellEventNextQuestions`, and
  `Fact/Milestone/XYF-1.1`.
- Tests: `SpellEvidenceTests` verifies the current client `Spell4CastResult`
  envelope, key cast-result ids used by spell-failure paths, spell effect
  delegate construction, and no-throw/no-handler behavior for high-frequency
  missing handlers.
- Ghidra anchors traced: `AddSpellShortcut`, `ClearSpellThreshold`,
  `SpellCastFailed`, `DashCastSuccess`, `CombatLogDamage`,
  `CombatLogHeal`, and `CombatLogHealingAbsorption`.

Top missing spell effect handler gaps by current `Spell4Effects` rows:

| Rank | Effect | Rows | Server target | Next milestone |
| --- | --- | ---: | --- | --- |
| 1 | `RavelSignal` | 5262 | `NexusForever.Game/Spell/Effect/Handler` | `XYF-1.3`/capture |
| 2 | `NpcExecutionDelay` | 3288 | spell timing/script bridge | `XYF-1.5` |
| 3 | `SummonCreature` | 1951 | summon/entity ownership | `XYF-1.6` |
| 4 | `ItemVisualSwap` | 1542 | visuals/equipment packets | `XYF-1.3` |
| 5 | `GiveSchematic` | 1171 | reward/account/character unlocks | `XYF-1.3` |
| 6 | `DespawnUnit` | 855 | spell-created entity cleanup | `XYF-1.6` |
| 7 | `FacilityModification` | 771 | housing/warplot facility state | later queue |
| 8 | `UnitStateSet` | 752 | unit state/aura lifecycle | `XYF-1.5` |
| 9 | `Absorption` | 581 | shields/combat logs | `XYF-1.4` |
| 10 | `SetBusy` | 571 | unit state/UI state | `XYF-1.5` |

Top implemented-but-partial spell areas by current `Spell4Effects` rows:

| Area | Rows | Why it remains capture-required |
| --- | ---: | --- |
| `Damage` | 29445 | formula branches and `CombatLogDamage` fields need combat-log traces. |
| `UnitPropertyModifier` | 20105 | 11385 duration rows and 472 persistence rows need lifetime validation. |
| `Proxy` | 16929 | child spell timing, tick selection, and cancellation are not fully proven. |
| `Fluff` | 10790 | current handler is a no-op; client-visible side effects need proof. |
| `CCStateSet` | 7185 | CC packet/result parity belongs with combat and proc validation. |
| `Proc` | 3498 | trigger source, reset, and expiration semantics need runtime traces. |
| `Heal` | 3098 | formula branches and `CombatLogHeal` fields need combat-log traces. |

Cast-result baseline:

- Wiki `Spell4CastResult` has 327 rows, id range 0 through 331.
- Server `CastResult` has the same 327 wire ids and writes them as 9-bit
  packet values in `ServerSpellCastResult`.
- The five absent ids in both sources are `61`, `62`, `110`, `111`, and `324`.
- Name spelling differs in places, but no current client id is unmapped.

Next concrete packet/model questions:

- `AddSpellShortcut` and `RemoveSpellShortcut` include shortcut type, object id,
  and contract/path update side effects; keep action-set packet work tied to
  this path.
- `ClearSpellThreshold` is suppressed for spells whose client spell flags include
  the observed `0x40` mask; validate `ServerSpellThresholdClear.Unknown0` and
  threshold flag naming before changing behavior.
- `SpellCastFailed` looks up `Spell4CastResult`, formats
  `SpellCastFailed("iiUUSS", ...)`, treats `Queued` (`317`) specially, and still
  needs packet captures for the two unknown integer/string fields.
- `DashCastSuccess` also emits `DashCastFail`; dash validation belongs in
  `XYF-1.2` alongside cast result packets.
- `CombatLogDamage`, `CombatLogHeal`, and `CombatLogHealingAbsorption` expose
  client field names such as `nDamageAmount`, `nHealAmount`, `nAmount`,
  `bTargetVulnerable`, `bTargetKilled`, and `bPeriodic`; full field parity moves
  to `XYF-1.4`.

### XYF-1.2 Cast Validation And Result Packets

Purpose: make cast start/failure behavior client-shaped before expanding
effects.

Deliverables:

- Compare `Spell4TargetMechanics`, `Spell4Prerequisites`, explicit/implicit
  target types, telegraph type, cast method, and `Spell4CastResult` rows against
  current validators.
- Verify result values for invalid target, target out of range, failed
  prerequisite, movement/rotation during telegraph, dead caster, non-player
  caster, vehicle caster, and unknown spell id.
- Implement only confirmed or high-confidence result paths.
- Add handler/model tests for malformed spell packets and failed cast responses.

Capture required:

- Cast success, cast failure, target failure, target moving out of range,
  caster movement during telegraph, and non-player caster examples.

### XYF-1.3 Reward, Cooldown, And Ownership Effects

Purpose: finish lower-risk player-state effects with before/after snapshots.

Deliverables:

- Implement or correct reward effects for inventory, currency, reputation,
  title, achievement, objective, path mission, path XP, and account unlocks.
- Validate full-bag handling, currency caps, reputation limits, duplicate
  ownership, duplicate unlocks, and partial reward failure.
- Verify cooldown groups, global cooldown groups, ability charges, reset
  behavior, and active recharge timers.
- Add tests with fake managers and representative table rows for every changed
  effect.

Capture required:

- Before/after snapshots for rewards, cooldowns, action sets, inventory,
  achievements, objectives, currencies, reputation, account unlocks, and titles.

### XYF-1.4 Combat Formulas, Combat Logs, Shields, And CC

Purpose: bring combat output closer to client assets and combat-log packets.

Deliverables:

- Map `GameFormula` rows to current damage, heal, shield, crit, deflect,
  strikethrough, armor pierce, lifesteal, reflect, vulnerability, and mitigation
  code.
- Trace `CombatLogDamage`, `CombatLogHeal`, `CombatLogInterrupted`,
  `CombatLogLifeSteal`, `CombatLogResurrect`, and `CombatLogVitalModifier`
  anchors for packet expectations.
- Fix combat-log packet fields only when Ghidra, assets, and server models line
  up.
- Add replay-style tests for representative damage, heal, shield, interrupt, CC,
  and death/resurrection flows.

Capture required:

- Combat logs and packet traces for every formula branch changed.

### XYF-1.5 Procs, Auras, Proxy Spells, And Persistence

Purpose: make ongoing spell behavior stable instead of cast-start-only.

Deliverables:

- Validate proc triggers for damage dealt, damage taken, heal, shield,
  interrupt, CC, movement, kill, death, periodic tick, and aura expiration.
- Map aura lifetime, stack, refresh, dispel, stealth, and expiration behavior to
  packet/model changes.
- Validate proxy spell placement, child spell timing, tick schedule, target
  selection, cancellation, and cleanup.
- Persist any spell state that the client expects to survive relog, transfer, or
  death.

Capture required:

- Proxy ticks, aura expiry, channel ticks, stealth changes, proc triggers, and
  relog/transfer snapshots.

### XYF-1.6 Summons, Pets, Vehicles, And Cleanup

Purpose: resolve spell-created entity ownership and lifetime rules.

Deliverables:

- Map summon, pet, vehicle, and temporary entity effects to `Creature2`, visuals,
  ownership, seats/passengers, despawn rules, auras, and cleanup packets.
- Confirm player death, logout, map transfer, owner despawn, and effect removal
  cleanup behavior.
- Add tests for owner tracking, despawn safety, and packet emission where
  evidence is strong.

Capture required:

- Summon/pet/vehicle creation, ownership, seat entry/exit, death, logout,
  transfer, and cleanup traces.

### XYF-1.7 Spell Regression Gate

Purpose: prevent spell fixes from breaking packet and gameplay basics.

Deliverables:

- Add a repeatable spell smoke suite that loads representative table rows and
  exercises cast validation, effect dispatch, cooldown state, combat logs, and
  persistence-safe no-op behavior.
- Add DB fact links from confirmed spell behavior to the tests that protect it.
- Mark remaining spell unknowns as `XYF-3.x+` backlog only after PvP has the
  spell foundation it needs.

Exit criteria:

- Spell milestones have a passing regression gate.
- PvP work can depend on cast result, combat log, cooldown, and death behavior
  without reopening basic spell questions first.

## XYF-2.x PvP Milestones

### XYF-2.1 PvP Evidence Baseline

Purpose: build PvP from client state and asset rows instead of server guesses.

Inputs already available:

- Existing fact: `Fact/Pvp/ClientDuelAndMatchAnchors`.
- Ghidra anchors: `DuelAccepted`, `DuelStateChanged`, `DuelLeftArea`,
  `PVPMatchTeamInfoUpdated`, `PVPMatchStateUpdated`, `PVPMatchFinished`,
  `PvpRatingUpdated`, `PvpKillNotification`, `CombatLogKillPVP`, and
  battleground public event names.
- Wiki rows: PvP chat channel, PvP achievement/title categories, battleground
  public event names, PvP capture creatures, powerups, cannon targets,
  satellites/uplinks, shields, and other `Creature2` rows.
- Server targets: `Source/NexusForever.Game/Pvp/`,
  `Source/NexusForever.WorldServer/Network/Message/Handler/Pvp/`,
  `Source/NexusForever.Network.World/Message/Model/Pvp/`, and
  `Source/NexusForever.Script.Instance/Battleground/`.

Deliverables:

- Rank duel, match queue, battleground objective, rating, reward, and stat gaps.
- Link every top PvP gap to Ghidra string/function refs, wiki rows, and server
  files.
- Identify which PvP behavior depends on unfinished spell/combat milestones.

Exit criteria:

- PvP has an evidence-backed gap list.
- Duel packet/state questions are separated from battleground objective and
  reward questions.

### XYF-2.2 Duel State And Result Packets

Purpose: make duels match client-visible packet state.

Deliverables:

- Validate duel invite, accept, decline, timeout, cancel, forfeit, out-of-area
  warning, cancel-warning, start countdown, death finish, and result reason
  values.
- Compare current `Duel`, `DuelManager`, and PvP packet models with
  `DuelAccepted`, `DuelStateChanged`, and `DuelLeftArea` client anchors.
- Confirm faction, combat, dead-player, distance, phase, ignore-duels, and
  already-dueling failure results.
- Add tests for duel state transitions and result packet selection.

Capture required:

- Client traces for invite/accept/decline/timeout/forfeit/out-of-area/death
  duel flows.

### XYF-2.3 Queue And Match Lifecycle

Purpose: align matchmaking and match-state packets with the client.

Deliverables:

- Trace and map `PVPMatchTeamInfoUpdated`, `PVPMatchStateUpdated`, and
  `PVPMatchFinished`.
- Validate queue join/leave, ready check, team assignment, match start, match
  end, deserter/cooldown, and reconnect behavior.
- Compare rating category models with `PvpRatingUpdated`, `GetPvpRatings`, and
  `GetMyPvpRatings` anchors.
- Add tests for queue/match state packets once field order and enum meanings are
  confirmed.

Capture required:

- Queue, ready-check, match start/end, rating update, and reconnect traces.

### XYF-2.4 Battleground Objectives And Map Scripts

Purpose: make battleground map scripts use asset-backed objectives and entities.

Deliverables:

- Map battleground public event types to server scripts for Walatiki Temple,
  Daggerstone Pass, Halls of the Bloodsworn, and Cannon.
- Link PvP `Creature2` rows for flags, capture points, control panels, uplinks,
  time bombs, cannons, shields, resource indicators, and powerups.
- Validate objective update packets, scoring state, phase transitions, despawn
  rules, and capture ownership.
- Add script tests or smoke checks for objective registration and entity spawn
  safety.

Capture required:

- Objective state packets, scoring changes, entity spawn/despawn, capture
  transitions, and powerup interaction traces.

### XYF-2.5 PvP Rewards, Ratings, Stats, And Achievements

Purpose: make match outcomes persist and display correctly.

Deliverables:

- Validate rating category ids, rating deltas, match stats, kill notifications,
  PvP achievements, PvP titles, rewards, contracts, currencies, and deserter
  penalties.
- Link match reward behavior to account/character DB state and relevant table
  rows.
- Add persistence tests for reward/stat/rating updates once packet and table
  semantics are confirmed.

Capture required:

- Match finish, reward claim, rating update, PvP kill notification, achievement,
  title, contract, and currency snapshots.

### XYF-2.6 PvP Regression Gate

Purpose: protect duel and battleground behavior before returning to broader
systems.

Deliverables:

- Add repeatable PvP smoke tests for duel lifecycle, queue/match packet models,
  battleground script registration, objective state, and reward persistence.
- Link confirmed PvP facts to tests.
- Move unresolved PvP questions into later milestones with explicit capture
  requirements.

## Later Milestone Queues

### XYF-3.x Movement, Maps, Splines, And Return Locations

- Finish multi-spline movement, rotation splines, forced movement, platform
  movement, graveyard/holocrypt return behavior, instance transfers, and map
  load correctness.
- Existing facts: movement spline schema, loader functions, type distribution,
  packet serialization, and runtime gaps.
- Keep multi-spline runtime semantics capture-required until taxi/platform
  traces prove timing, flags, continuation, offsets, and height fields.

### XYF-4.x Entities, Public Events, Quests, And Path Missions

- Replace conservative entity models with asset-backed packet fields for every
  `EntityType`.
- Finish trigger bounds, traps, scanner units, esper pets, pinata loot, lockbox,
  structured plug, housing harvest plug, housing plant, tutorial hologram, path
  mission, and public event state.
- Keep unsupported path mission types and settler tiers capture-required.

### XYF-5.x Housing, Guild, And Community

- Validate residence privacy result packets, visit failures, plug/decor
  placement, roommate/neighbour permissions, crate/uncrate, remodel, vendor
  lists, and community donations.
- Link `WorldSocket`, active prop, decor, plug, structure tier, guild, and
  community rows to packet fields and persistence.

### XYF-6.x Storefront, Rewards, And Account

- Validate catalog, entitlement, reward-track, purchase, refund, grant, rotation,
  service-token spending, account items, and duplicate claim behavior.
- Record before/after account transaction rows for success and failure cases.

### XYF-7.x Chat, Social, And UI

- Finish chat packet samples for every `ChatFormatType`, including item id, item
  guid, item full, quest, archive article, nav point, loot, alien, profanity,
  roleplay, and unknown format values.
- Validate channels, whispers, ignore/block checks, guild/community chat,
  cross-world relay, and chat persistence.

### XYF-8.x Auth, STS, And Launcher

- Continue STS/auth facts only when gameplay work needs account/session/client
  handoff behavior.
- Existing facts cover STS connect shape, fragmented body assembly, and method
  id names.

## Completed Baseline Work

Completed in the 2026-04-22 pass:

- Registered the existing chat formatter implementations instead of only the
  item/quest subset.
- Added item-guid chat round-trip support and fixed the internal item-guid
  format type.
- Made missing chat formatter registrations drop unsupported formatting instead
  of crashing chat conversion.
- Converted several packet-facing unsupported values to invalid-packet handling.
- Added conservative entity models for several entity classes that already had
  packet models but threw during creation.
- Corrected `PinataLootEntity.Type`.
- Hardened unfinished movement spline/rotation entry points so they fall back
  safely instead of throwing.
- Fixed the rotation command resynchronization typo from position multi-spline
  to rotation multi-spline.
- Replaced a housing privacy default throw with public fallback.
- Replaced a costume mannequin save throw with `InvalidMannequinIndex`.
- Made item display fallback return the table display id instead of throwing.
- Stopped the Exile tutorial combat hologram script from throwing for
  unsupported factions.

## Do Not Guess

These remain high-risk until evidence improves:

- Deep spell target persistence, proxy behavior, summon/vehicle effects,
  threat/proc completeness, and combat formula parity.
- Full multi-spline movement and spline rotation runtime commands.
- Duel/match result values, PvP rating categories, battleground objective state,
  and match reward persistence.
- Settler improvement tiers beyond tier 0.
- Unsupported path mission types.
- Housing plug/plant/structured plug packet field correctness beyond
  conservative ids.
- Trigger bounds and trigger entity model details.
- Holocrypt resurrection behavior and tutorial/content return locations.
- Marketplace auction filters and Who query parameter semantics.
