# Spell Diagnostics

Spell diagnostics are opt-in telemetry for reproducing class/spell bugs with a
real client. They add structured logs, traces, and metrics around the central
spell pipeline:

- cast lifecycle and final cast result
- cast validator failures
- prerequisite checks
- target selection counts
- effect handler dispatch, missing handlers, skipped effects, and non-ok effect
  results
- chat markers that bracket a manual test session

## Enable

In `WorldServer.json`, enable the diagnostics block:

```json
"Diagnostics": {
    "Spell": {
        "Enable": true,
        "TraceAll": false,
        "ChatMarkers": true,
        "IncludeEffectResults": true,
        "IncludePrerequisiteSuccess": false,
        "MaxChatMessageLength": 256,
        "Spell4Ids": [],
        "Spell4BaseIds": [],
        "PlayerNames": []
    }
}
```

With `TraceAll` set to `false`, spell traces are captured when a character has
an active chat-marked test session, or when the spell/player matches one of the
configured filters. Set `TraceAll` to `true` only for short local sessions.

## Manual Client Workflow

Use normal chat, not a command:

```text
stalker - pounce start
```

Cast the spell, try the tier/AMP/prerequisite scenario, and put observations in
chat while the session is active:

```text
tier 4 still shows the wrong cooldown
proc fired once, but the resource refund did not happen
```

End the session:

```text
stalker - pounce test over
```

The server emits structured log entries named like `Spell diagnostic test
started`, `Spell diagnostic test note`, and `Spell diagnostic test ended`. Spell
casts during the active session are tagged with `nf.spell_test.id` and
`nf.spell_test.label`, so the corresponding traces, metrics, and logs can be
filtered together in Aspire.

## Saving Reports

Aspire stores telemetry in memory while the dashboard is running, but the
dashboard can export selected resources and telemetry types as a zip file. Use
the dashboard settings menu, open **Manage logs and telemetry**, select the
world-server structured logs, traces, and metrics for the test window, then
export them.

For long-term unattended retention, route OTLP through a collector or another
backend in addition to Aspire.
