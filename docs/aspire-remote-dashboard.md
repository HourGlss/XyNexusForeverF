# Aspire Remote Dashboard

This repo supports a two-box development setup:

- `192.168.0.241`: the real NexusForever server, started manually.
- `192.168.0.144`: the Aspire dashboard host, used only to receive and display telemetry.

## Why this mode exists

The normal `NexusForever.Aspire.AppHost` profile orchestrates a full local stack: MySQL, RabbitMQ, migrations, and all server projects.

That is the wrong shape for a remote/manual server workflow. The `NexusForever.Aspire.RemoteDashboard` launch profile runs the Aspire dashboard without starting the local NexusForever stack, so a manually started server can export telemetry into it.

## Run on 192.168.0.144

Run the AppHost with the `NexusForever.Aspire.RemoteDashboard` launch profile.

This profile:

- serves the dashboard at `http://192.168.0.144:17021`
- receives OTLP/HTTP telemetry at `http://192.168.0.144:4318`
- exposes the local Aspire resource service at `http://localhost:17022` for the VS Code Aspire sidebar
- disables dashboard auth for LAN/dev convenience
- skips local orchestration by setting `NEXUSFOREVER_ASPIRE_REMOTE_DASHBOARD_ONLY=true`

Smoke test on `192.168.0.144`:

```bash
aspire run --apphost Source/NexusForever.Aspire.AppHost/NexusForever.Aspire.AppHost.csproj --no-build
```

With the dashboard running, these checks should succeed:

```bash
ss -ltnp | rg '(:17021|:4318)'
curl -sS -o /dev/null -w '%{http_code}\n' -X POST http://192.168.0.144:4318/v1/metrics -H 'Content-Type: application/x-protobuf' --data-binary ''
aspire ps --format Json
```

Expected result:

- `17021` is listening for the dashboard
- `4318` is listening for OTLP/HTTP
- the OTLP POST returns `200`
- `aspire ps` lists `NexusForever.Aspire.AppHost`

## Run from VS Code

This repo now includes VS Code workspace files to make the Aspire extension useful for the remote receiver setup:

- `.vscode/launch.json` adds an `Aspire: Remote Dashboard Receiver` debug target
- `.vscode/settings.json` points the extension at `/home/xyf/.aspire/bin/aspire`, keeps the dashboard in the VS Code browser, and leaves the tab open after debug stops
- `.vscode/tasks.json` adds run, stop, and smoke-test tasks for the remote receiver
- `.aspire/settings.json` pins the AppHost project so Aspire CLI and extension commands resolve the correct AppHost from the repo root

Important detail:

- the Aspire VS Code extension debug schema does not expose AppHost launch profile selection
- because of that, `NexusForever.Aspire.RemoteDashboard` is intentionally the first launch profile in `launchSettings.json`
- the full local orchestration profile is still available as `NexusForever.Aspire.FullStack`

Use `F5` in VS Code with the `Aspire: Remote Dashboard Receiver` configuration, or run the `Aspire: Run Remote Dashboard Receiver` task, to launch the receiver on `192.168.0.144`. After it starts, click Refresh in the Aspire sidebar if the running AppHost list has not updated yet.

## Run on 192.168.0.241

The live server must have telemetry enabled in its real runtime config files, not just the example JSON in this repo.

Use a `Telemetry` section like:

```json
"Telemetry": {
  "ServiceName": "world-server",
  "Logging": {
    "Enable": true
  },
  "Metrics": {
    "Enable": true
  },
  "Tracing": {
    "Enable": true
  },
  "Endpoint": {
    "Url": "http://192.168.0.144:4318",
    "Protocol": "HttpProtobuf",
    "Headers": ""
  }
}
```

Use a unique `ServiceName` per process, for example `auth-server`, `sts-server`, `world-server`, `character-api`, `chat-server`, and `group-server`. The server reads config from the runtime working directory, usually `Source/<Project>/bin/Debug/net10.0/<Server>.json`; changing only `*.example.json` does not affect a running deployment.

## What you get

- Structured logs
- Traces
- Metrics

Because the real server on `192.168.0.241` is not launched by the AppHost, Aspire resource orchestration features are limited. This setup is primarily a remote telemetry viewer, not a full remote process controller.

## Security note

`NexusForever.Aspire.RemoteDashboard` uses unsecured HTTP and anonymous access so telemetry from the manual server can flow without extra setup. Keep it on a trusted LAN/dev network only.
