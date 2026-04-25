#!/usr/bin/env bash

set -euo pipefail

host="${1:-192.168.0.144}"

printf 'Checking Aspire remote receiver on %s\n' "$host"
printf '\nListeners:\n'
ss -ltnp | rg '(:17021|:17022|:4318)' || true

printf '\nAspire apphosts:\n'
/home/xyf/.aspire/bin/aspire ps --format Json

printf '\nDashboard health:\n'
curl -sS --max-time 5 "http://${host}:17021/health"
printf '\n'

printf '\nOTLP metrics POST status:\n'
curl -sS -o /dev/null -w '%{http_code}\n' \
  --max-time 5 \
  -X POST "http://${host}:4318/v1/metrics" \
  -H 'Content-Type: application/x-protobuf' \
  --data-binary ''
