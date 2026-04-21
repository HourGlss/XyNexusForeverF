# NexusForever Code Quality

This repository now has a C#-native quality surface inspired by the Rusaren,
SaferCpp, and SaferPython model: quality is evidence, not taste. The routine
path is advisory and artifact-backed, so it can guide cleanup without blocking
local work on a single headline number.

## Canonical Commands

Windows PowerShell:

```powershell
.\scripts\quality.ps1
.\scripts\quality.ps1 -Task report
.\scripts\quality.ps1 -Task format
```

Linux, WSL, or Git Bash:

```bash
./scripts/quality.sh
./scripts/quality.sh report
./scripts/quality.sh format
```

The default `all` task builds the solution, runs tests with Cobertura coverage
collection when test projects exist, and generates the quality report.

## Stable Artifacts

The report writes stable paths under `artifacts/code-quality/latest/`:

- `quality-report.json`
- `index.html`
- `raw/files.json`
- `raw/functions.json`
- `raw/files.csv`
- `raw/functions.csv`
- `raw/coverage.json`
- `build.log`

`artifacts/` is already ignored by git, so report output is repeatable local and
CI evidence rather than checked-in churn.

## What Is Scored

The C# reporter has three routine lanes:

- `Complexity`: Roslyn-based function inventory with cyclomatic complexity,
  advisory cognitive complexity, maintainability context, file hotspot tables,
  and a headline score based on measured file complexity health.
- `Clean Code`: file-size scoring, measured-source/test/tooling separation,
  oversized file surfacing, and build-warning scoring when a build log is
  available.
- `Coverage`: Cobertura summary from `dotnet test --collect:"XPlat Code
  Coverage"` when test projects and coverage artifacts exist.

Only test and tooling files are excluded from complexity and clean-code
measurements. Every other `.cs` file under `Source/` is measured, including EF
migrations, designers, generated-looking files, and entry points. Build outputs
under `bin/` and `obj/` are still ignored because they are not source files.

## Current Coverage Reality

The current solution does not contain dedicated C# test projects. The coverage
lane therefore reports `degraded` and records the absence of test projects as a
known gap instead of estimating coverage. Once tests are added, keep them in
`*.Tests` projects or set `<IsTestProject>true</IsTestProject>` so the wrapper
can discover them and collect Cobertura artifacts automatically.

## Mindset

This follows the referenced templates in a few practical ways:

- Reports are non-blocking by default and include machine-readable JSON plus
  human-readable HTML.
- Measured source is scored separately from tests and tooling; generated-looking
  source is still measured on this branch.
- Coverage is treated as evidence, not proof. Missing coverage is surfaced as a
  gap, and future critical-file gates should be explicit instead of relying only
  on one global percentage.
- Complexity is localizable. The useful output is the hotspot table, not just
  the overall grade.
- Routine checks stay fast; deeper lanes such as mutation testing, fuzzing,
  dependency policy, and performance budgets can be added later without changing
  the report contract.
