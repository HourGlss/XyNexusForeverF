#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOURCE_DIR="$ROOT_DIR/Source"

if [[ -n "${DOTNET_EF:-}" ]]; then
    read -r -a EF_CMD <<< "$DOTNET_EF"
elif [[ -x "$HOME/.dotnet/tools/dotnet-ef" ]]; then
    EF_CMD=("$HOME/.dotnet/tools/dotnet-ef")
elif command -v dotnet-ef >/dev/null 2>&1; then
    EF_CMD=("dotnet-ef")
else
    EF_CMD=("dotnet" "ef")
fi

run_update() {
    local host_project="$1"
    local database_project="$2"
    shift 2

    echo "==> Updating ${database_project#NexusForever.Database.} database"
    (
        cd "$SOURCE_DIR/$host_project"
        "${EF_CMD[@]}" database update \
            --project "../$database_project" \
            --startup-project "." \
            "$@"
    )
}

run_update "NexusForever.WorldServer" "NexusForever.Database.Auth" "$@"
run_update "NexusForever.WorldServer" "NexusForever.Database.Character" "$@"
run_update "NexusForever.WorldServer" "NexusForever.Database.World" "$@"
run_update "NexusForever.Server.ChatServer" "NexusForever.Database.Chat" "$@"
run_update "NexusForever.Server.GroupServer" "NexusForever.Database.Group" "$@"

echo "==> Database updates complete"
