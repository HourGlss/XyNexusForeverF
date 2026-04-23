# NexusTogether Gameplay TODO

Current milestone: `XYF-1.3`

## Goal

Make WildStar class kits usable in game.

Progress is measured by player capability, not internal plumbing. A skill or AMP
is healthier only when a player can actually:

- slot or select it,
- commit/save it,
- use it without obvious server failure,
- see the primary gameplay result happen,
- reopen the builder or relog without losing expected state.

## Working Rules

- `xywikif` sqlite data and targeted Ghidra work are source truth when the
  current C# disagrees.
- `class_skills.md` and `class_amps.md` are the live gameplay backlogs.
- `class_skills_issue_map.md` is the sync map between `class_skills.md` and
  GitHub issue anchors.
- `[x]` means human-confirmed in game or confirmed by a closed GitHub issue.
- `2` means "probably works" from code, tests, and evidence, but not yet human
  confirmed.
- `1` means "partially working or suspicious"; a bug likely remains.
- `0` means "no believable working path" or "data is too incomplete to claim it
  works".
- Every reusable bug family should get a regression test.
- Every reverse-engineered answer that helps future fixes should be recorded in
  `/home/xyf/NCSOFT/tooling/xywikif/wiki/wiki.sqlite`.
- Only comment on GitHub issues after implementation lands. Humans close issues.

## What Recent Spell Fixes Actually Taught Us

The last round of work was useful because it improved real player behavior:

- Engineer bots and several Engineer spell paths were fixed as gameplay bugs,
  not as abstract engine chores.
- Targeting, telegraph, facing, and positional execution bugs blocked more
  skills than raw effect-handler counts suggested.
- Shared effect handlers fixed many score-2 skills at once.
- Rapid-tap, threshold, and cooldown problems were cross-skill bugs, not
  one-off spell bugs.
- AMP save/sync was a real gameplay blocker and needed its own fix and tests.
- Regression tests help most when they protect a reusable bug family instead of
  one isolated skill.

That is the model to keep following.

## Current Baseline (2026-04-23)

### Class Skills

Source: `class_skills.md`

| State | Count |
| --- | ---: |
| `[x]` human-confirmed | 7 |
| `2` probably works | 129 |
| `1` partial/suspicious | 53 |
| `0` no believable path | 0 |
| Total | 189 |

Score-1 skill counts by class:

- `Esper`: 13
- `Stalker`: 13
- `Engineer`: 10
- `Medic`: 7
- `Warrior`: 5
- `Spellslinger`: 5

Important reading of this baseline:

- We no longer have score-0 class skills in the tracker.
- The real skill backlog is now the score-1 bucket, not generic "spell system"
  theory work.
- The next gains should come from burning down shared score-1 bug families and
  converting score-2 skills into `[x]` through human verification.

### AMPs

Source: `class_amps.md`

| State | Count |
| --- | ---: |
| `[x]` human-confirmed | 0 |
| `2` probably works | 741 |
| `1` partial/suspicious | 28 |
| `0` no believable path / data gap | 129 |
| Total | 898 |

Important reading of this baseline:

- Class-specific AMPs are mostly in `2`, with a smaller real bug bucket in `1`.
- All current score-0 AMPs are in the shared tree.
- Many shared AMP names are blank in the current wiki extraction, so
  `class_amps.md` sometimes has fallback names from linked `Spell4` titles or
  AMP ids.

That means the AMP backlog is two different problems:

1. real gameplay bugs around AMP behavior and persistence,
2. data-quality cleanup for shared AMP naming and classification.

## XYF-1.x Gameplay Milestones

### XYF-1.2 Capability Baseline And Shared Bug Families

Status: completed 2026-04-23.

Purpose:

Turn spell and AMP work into a gameplay-first backlog instead of a notional
engine roadmap.

Completion evidence:

- `class_skills.md` exists and is scored.
- `class_amps.md` exists and is scored.
- score-0 class skills have already been burned down to `0`.
- shared spell bug families have already landed for:
  - shared score-2 effect handlers,
  - ranked-one dispel and scale effects,
  - ranked-zero rapid-tap threshold handling,
  - AMP save/sync persistence.
- regression anchors now exist in:
  - `SpellEvidenceTests`
  - `AmpHandlerTests`

This milestone is what made the current tracker-based plan possible.

### XYF-1.3 Class Kit Reliability

This is the main gameplay backlog.

Work this queue by class skill tracker first, not by abstract subsystem.

What counts as a real fix:

- the skill can be slotted and cast,
- the primary result happens in a believable way,
- cooldown/threshold/telegraph/position behavior is at least plausible,
- any save/reopen/relog behavior the client expects still works,
- a reusable test is added when the bug pattern is broader than one skill.

Priority order right now:

1. `Esper` score-1 skills
2. `Stalker` score-1 skills
3. `Engineer` score-1 skills
4. everything else

Use shared fixes before bespoke fixes whenever possible.

Exit criteria for this lane:

- `class_skills.md` score-1 count drops from `53` to `20` or lower,
- no class has more than `5` score-1 skills left,
- `[x]` count rises from `7` to `40` or higher through human verification.

### XYF-1.4 Score-2 Verification And Promotion

`2` means "probably works", not "done".

The goal is to use testable bug families to convert score-2 items into either:

- `[x]` because humans verified them, or
- `1` because a reusable flaw was found.

Shared bug families to keep applying across all classes:

- targeting, telegraph, facing, and positional execution,
- threshold, rapid-tap, cooldown, and charge behavior,
- proxy and child-spell behavior,
- summon, bot, trap, and cleanup behavior,
- dispel, cleanse, shield, absorb, and scale behavior,
- action-set, stance, mode, and builder state behavior.

When one of these families is fixed for one class, re-score every affected skill
or AMP the same day.

Exit criteria:

- at least `25` current score-2 skills become `[x]`,
- any score-2 skill that fails reusable tests is downgraded the same day,
- the score-2 bucket becomes meaningfully trustworthy for human testing.

### XYF-1.5 AMP Reliability And Shared AMP Cleanup

AMP work is only useful if it improves actual player behavior.

AMP capability checklist:

- AMP selection can be committed,
- reopening the AMP window shows the same picks,
- relog preserves the same picks,
- AMP-granted spells or stat effects appear when enabled,
- AMP-granted spells or stat effects disappear when respeced away,
- AMP-gated prerequisites react correctly to current AMP state.

Current hotspots:

- score-1 AMPs: `28`
- score-0 shared AMPs: `129`
- shared AMP titles and classification are still noisy in the current wiki build

Exit criteria for this lane:

- AMP save/reopen/relog is human-checked on all six classes,
- score-1 AMPs drop from `28` to `10` or lower,
- shared score-0 AMPs are explicitly split into "real gameplay blocker" versus
  "data naming/classification gap".

### XYF-1.6 Gameplay Regression Gate And Evidence Hygiene

This is a quality gate for spell and AMP work.

For each real bug fix:

- update `class_skills.md` or `class_amps.md`,
- add or extend a focused regression test,
- add a sqlite fact if source-truth research answered a real question,
- comment on the matching GitHub issue after implementation,
- leave issue closure to humans.

Current regression anchors:

- `SpellEvidenceTests`
- `AmpHandlerTests`

Those should keep growing around reusable gameplay bug families.

Exit criteria:

- every new gameplay bug family fix adds or extends a focused regression test,
- tracker updates, sqlite facts, and issue comments happen as part of the same
  workflow,
- spell and AMP fixes stop depending on memory of prior reverse-engineering work.

## Scope Guard

These still matter, but they are not the main backlog until class kits and AMPs
are in better shape:

- combat formula parity,
- combat-log field-perfectness,
- full proc/aura lifetime truth,
- PvP queue and battleground scripting,
- movement spline parity,
- housing/storefront breadth work.

If a task does not clearly improve a player-facing skill or AMP outcome, it is
probably not `XYF-1.3` work.

## XYF-1.3 Exit Criteria

This milestone is complete when all of the following are true:

- `class_skills.md` has no score-0 items and `20` or fewer score-1 items,
- every class has `5` or fewer score-1 skills,
- at least `40` skills are `[x]`,
- AMP save/reopen/relog is confirmed on all six classes,
- `class_amps.md` score-1 count is `10` or lower,
- the current spell/AMP regression suite passes cleanly.

## After XYF-1.3

### XYF-1.7 Combat Truth

Once kits are broadly usable, focus on:

- combat formulas,
- combat logs,
- proc truth,
- aura lifetime truth,
- shield and absorb correctness.

### XYF-2.x PvP

PvP becomes the next real milestone only after class kit behavior is stable
enough that duel, battleground, and match bugs are not just spell bugs in
disguise.
