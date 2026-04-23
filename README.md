# NexusTogether Developer Map

This branch is for working on an old C# WildStar server-emulator codebase, not
for selling the project or walking someone through hosting a public server. Use
this README as a map when you are trying to learn where code lives, how changes
flow through the solution, and which files matter on this branch.

The main solution is [Source/NexusForever.sln](Source/NexusForever.sln). Most
projects target `.NET 10`, use central package versions from
[Source/Directory.Packages.props](Source/Directory.Packages.props), and are
organized by runtime layer rather than by feature.

## Root Layout

| Path | Purpose |
| --- | --- |
| [Source/](Source/) | All active C# projects and the solution files. Start here for runtime code. |
| [README.md](README.md) | This developer orientation file. |
| [todo.md](todo.md) | Branch-local spell audit backlog, ordered so easier spell fixes can be knocked out first. |
| [CODEQUALITY.md](CODEQUALITY.md) | Notes for the local C# quality tooling and how to interpret its output. |
| [update-db.sh](update-db.sh) | Root-level helper for applying all EF database migrations on Linux, WSL, or Git Bash. |
| [update-db.ps1](update-db.ps1) | Root-level helper for applying the same EF database migrations from Windows PowerShell. |
| [eng/](eng/) | Engineering support files. Currently includes coverage runsettings. |
| [scripts/](scripts/) | Developer scripts, including quality report wrappers for Bash and PowerShell. |
| `artifacts/` | Generated local build/test/quality output. It is ignored by git. |
| [Obsolete/](Obsolete/) | Historical material. Do not assume it participates in the current solution. |
| [.editorconfig](.editorconfig) | Repository style defaults used by editors and analyzers. |

## Source Layout

### Hosts And Entry Points

These projects start processes or expose runtime endpoints:

| Project | What it does |
| --- | --- |
| [NexusForever.WorldServer](Source/NexusForever.WorldServer/) | Main world process. Owns world-session state, client packet handlers, commands, web console files, and service startup. |
| [NexusForever.AuthServer](Source/NexusForever.AuthServer/) | Authentication server host. |
| [NexusForever.StsServer](Source/NexusForever.StsServer/) | STS server host. |
| [NexusForever.Server.ChatServer](Source/NexusForever.Server.ChatServer/) | Chat server host. |
| [NexusForever.Server.GroupServer](Source/NexusForever.Server.GroupServer/) | Group/matching-adjacent server host. |
| [NexusForever.API.Character](Source/NexusForever.API.Character/) | Character API host. |
| [NexusForever.Aspire.AppHost](Source/NexusForever.Aspire.AppHost/) | Aspire orchestration project for local multi-service development. |
| [NexusForever.Aspire.Database.Migrations](Source/NexusForever.Aspire.Database.Migrations/) | Database migration runner. |
| [NexusForever.ClientConnector](Source/NexusForever.ClientConnector/) | Utility/client connector project. |
| [NexusForever.MapGenerator](Source/NexusForever.MapGenerator/) | Map generation utility. |

Look for `Program.cs` or `HostedService.cs` in these projects when you need to
understand startup flow. Look for `*.example.json`, `Logging.json`, and
`nlog.config` beside a host when you need local configuration shape.

### Gameplay Core

| Project | What it does |
| --- | --- |
| [NexusForever.Game](Source/NexusForever.Game/) | Main gameplay/domain layer: accounts, entities, movement, maps, combat, spells, quests, public events, housing, paths, PvP, storefront, and related managers. |
| [NexusForever.Game.Abstract](Source/NexusForever.Game.Abstract/) | Interfaces shared between gameplay systems and consumers. |
| [NexusForever.Game.Static](Source/NexusForever.Game.Static/) | Enums, flags, and static protocol/game constants. |
| [NexusForever.GameTable](Source/NexusForever.GameTable/) | Game-table loading and generated/static data access. |

Most feature work lands in `NexusForever.Game` first, with interfaces in
`Game.Abstract` only when another layer needs a stable contract. Constants and
opcode/game-table values generally belong in `Game.Static` or the network
static folders, not scattered through handlers.

Important gameplay subfolders on this branch:

| Path | Notes |
| --- | --- |
| [Source/NexusForever.Game/Spell/](Source/NexusForever.Game/Spell/) | Spell casting, validators, proc logic, target selection, effect data, and effect handlers. Pair this with [todo.md](todo.md). |
| [Source/NexusForever.Game/Entity/](Source/NexusForever.Game/Entity/) | Player/world entity state, movement, stats, synchronization, triggers, and the new `PathMissionManager`. |
| [Source/NexusForever.Game/PathContent/](Source/NexusForever.Game/PathContent/) | Path missions, path episodes, settler improvements, and path-related static types. |
| [Source/NexusForever.Game/Pvp/](Source/NexusForever.Game/Pvp/) | Duel state and duel manager code ported into this branch. |
| [Source/NexusForever.Game/Storefront/](Source/NexusForever.Game/Storefront/) | Storefront catalog/domain objects. Purchasing execution lives in the world server. |
| [Source/NexusForever.Game/Prerequisite/Check/](Source/NexusForever.Game/Prerequisite/Check/) | Prerequisite evaluators, including entitlement and stealth checks added on this branch. |

### Network And Packet Flow

| Project | What it does |
| --- | --- |
| [NexusForever.Network](Source/NexusForever.Network/) | Shared packet/message infrastructure and global opcode registration. |
| [NexusForever.Network.World](Source/NexusForever.Network.World/) | World client/server packet models. Files are commonly named `Client*`, `Server*`, or shared DTO names. |
| [NexusForever.Network.Auth](Source/NexusForever.Network.Auth/) | Auth protocol messages. |
| [NexusForever.Network.Sts](Source/NexusForever.Network.Sts/) | STS protocol messages. |
| [NexusForever.Network.Internal](Source/NexusForever.Network.Internal/) | Internal service-to-service messages. |

Common packet-change path:

1. Add or update the packet model in
   [Source/NexusForever.Network.World/Message/Model/](Source/NexusForever.Network.World/Message/Model/).
2. Register or update the opcode in
   [Source/NexusForever.Network/Message/GameMessageOpcode.cs](Source/NexusForever.Network/Message/GameMessageOpcode.cs).
3. Add the world handler in
   [Source/NexusForever.WorldServer/Network/Message/Handler/](Source/NexusForever.WorldServer/Network/Message/Handler/).
4. Keep business rules in `NexusForever.Game` or a world-server service, not in
   packet serialization code.

On this branch, PvP duel packets live under the `Pvp` handler/model areas, path
packets under `Path`, and storefront purchasing packets under `Account` plus
shared storefront DTOs.

### Persistence

| Project | What it does |
| --- | --- |
| [NexusForever.Database](Source/NexusForever.Database/) | Shared EF/database infrastructure. |
| [NexusForever.Database.Auth](Source/NexusForever.Database.Auth/) | Account/auth data. This branch adds account store transactions. |
| [NexusForever.Database.Character](Source/NexusForever.Database.Character/) | Character data. This branch adds path mission and path episode persistence. |
| [NexusForever.Database.Chat](Source/NexusForever.Database.Chat/) | Chat persistence. |
| [NexusForever.Database.Group](Source/NexusForever.Database.Group/) | Group persistence. |
| [NexusForever.Database.World](Source/NexusForever.Database.World/) | World/static persistence. |

Database projects generally use:

- `Model/` for EF models.
- `Repository/` for repository helpers when present.
- `Migrations/` for EF migrations and snapshots.
- `*Context.cs` and `*Database.cs` for context configuration and database
  registration.

Migrations are generated-looking files, but they are still source on this
branch. Review them for schema intent, but avoid style-only churn inside
migration designer files.

### Scripts And Content Behavior

| Project | What it does |
| --- | --- |
| [NexusForever.Script](Source/NexusForever.Script/) | Shared script framework: compilation, loading, template types, finders, and watchers. |
| [NexusForever.Script.Main](Source/NexusForever.Script.Main/) | General scripts, AI examples, and tutorial scripts. |
| [NexusForever.Script.Instance](Source/NexusForever.Script.Instance/) | Arena, battleground, and expedition scripts. This branch has active battleground/instance edits. |
| `NexusForever.Script.*` zone projects | Zone-specific script projects such as Alizar, Olyssia, Arcterra, Farside, and Isigrol. |

Instance scripts are usually organized by content type, then map or encounter:
`Arena/`, `Battleground/`, and `Expedition/`. Public event behavior often spans
map scripts, event scripts, public event objective definitions, creature hooks,
and phase definitions.

### Shared Utilities And Tooling

| Project | What it does |
| --- | --- |
| [NexusForever.Shared](Source/NexusForever.Shared/) | Shared helpers, configuration patterns, logging/service registration helpers. |
| [NexusForever.Telemetry](Source/NexusForever.Telemetry/) | OpenTelemetry and metrics support. |
| [NexusForever.Cryptography](Source/NexusForever.Cryptography/) | Cryptography helpers. |
| [NexusForever.IO](Source/NexusForever.IO/) | Binary/file IO helpers. |
| [NexusForever.API](Source/NexusForever.API/) | Shared API infrastructure. |
| [NexusForever.API.Character.Client](Source/NexusForever.API.Character.Client/) | Client for the character API. |
| [NexusForever.CodeQuality](Source/NexusForever.CodeQuality/) | Local Roslyn-based code quality reporter added on this branch. |

`ServiceCollectionExtensions.cs` files are the usual dependency-injection
registration points. If a manager or service exists but is not being resolved at
runtime, check the relevant `ServiceCollectionExtensions.cs` first.

## Branch-Specific Work Areas

This branch currently includes ported or in-progress work in these areas:

| Area | Main files to inspect |
| --- | --- |
| PvP duels | [Source/NexusForever.Game/Pvp/](Source/NexusForever.Game/Pvp/), `WorldServer/Network/Message/Handler/Pvp`, and `Network.World/Message/Model/Pvp`. |
| Stealth and spell effects | `SpellEffectStealthHandler`, `SpellEffectRemoveStealthHandler`, `ServerUnitStealth`, `EntityStatus`, and stealth prerequisite checks. |
| Path missions | `PathMissionManager`, [Source/NexusForever.Game/PathContent/](Source/NexusForever.Game/PathContent/), character DB path models/migrations, and world path handlers. |
| Store purchasing | `WorldServer/Storefront`, storefront packet models, account purchase handlers, and auth DB store transaction models/migrations. |
| Battleground scripts | [Source/NexusForever.Script.Instance/Battleground/](Source/NexusForever.Script.Instance/Battleground/). |
| Spell cleanup backlog | [todo.md](todo.md). Start here before changing spell handlers broadly. |
| Code quality reporting | [CODEQUALITY.md](CODEQUALITY.md), [scripts/quality.sh](scripts/quality.sh), [scripts/quality.ps1](scripts/quality.ps1), and [eng/CodeCoverage.runsettings](eng/CodeCoverage.runsettings). |

## Useful Commands

Build the full solution:

```bash
dotnet build Source/NexusForever.sln -c Release
```

Apply all EF database migrations from the repository root:

```bash
./update-db.sh
```

The database update helper applies Auth, Character, World, Chat, and Group
migrations in order. It changes into each host directory before running EF so
`WorldServer.json`, `ChatServer.json`, and `GroupServer.json` are read from the
same locations the existing design-time factories expect. The helper supplies
the required Auth, Character, and World context names internally because
`WorldServer` exposes more than one EF context. Extra EF options are forwarded
to each update, so after a successful build you can run:

```bash
./update-db.sh --no-build
```

Run the same migration flow from Windows PowerShell:

```powershell
.\update-db.ps1
```

The PowerShell helper mirrors the Windows installation guide's command-line EF
migration flow, keeps the same host-project ordering as `update-db.sh`, and
targets the same `WorldServer.json`, `ChatServer.json`, and `GroupServer.json`
files that the current design-time factories load. If those config files have
not been copied from their `*.example.json` counterparts yet, you can let the
script create them first:

```powershell
.\update-db.ps1 -CopyExampleConfigs
```

To match a release build or reuse a prior build on Windows:

```powershell
.\update-db.ps1 -Configuration Release -NoBuild
```

Generate the quality report only:

```bash
./scripts/quality.sh report
```

Run the full quality wrapper on Linux, WSL, or Git Bash:

```bash
./scripts/quality.sh
```

Run the same quality wrapper on Windows PowerShell:

```powershell
.\scripts\quality.ps1
```

Quality output is written to `artifacts/code-quality/latest/`. The important
files are `quality-report.json`, `index.html`, `raw/files.csv`, and
`raw/functions.csv`.

There are currently no dedicated C# test projects in the solution. The quality
tool reports coverage as unavailable instead of inventing a number. If tests are
added, put them in `*.Tests` projects or mark them with
`<IsTestProject>true</IsTestProject>` so the wrapper can discover them.

## Working Safely In This Codebase

This repository has long history and many overlapping ownership styles. Prefer
small, evidence-backed changes over broad cleanup sweeps.

- Preserve old behavior unless the change is intentionally fixing a known bug.
- Keep packet serialization, handler dispatch, gameplay rules, and persistence
  changes in their own layers.
- Do not refactor EF migrations, generated data, or protocol constants just for
  style.
- When adding a feature, update the packet model, opcode, handler, game/domain
  service, DI registration, and database model/migration together as needed.
- Run the full solution build before committing functional changes.
- Use [todo.md](todo.md) for spell triage so easy fixes and dangerous rewrites
  stay separated.
- Update the milestone when all features of that milestone are complete. 
