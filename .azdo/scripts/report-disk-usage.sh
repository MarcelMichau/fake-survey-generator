#!/usr/bin/env bash
# Report disk usage at each boundary of the Build + Test job without changing agent state.
set -u

printf '\n=== Disk usage: %s ===\n' "${1:-snapshot}"
df -h /
df -i /

for path in \
  "${PIPELINE_WORKSPACE:-}/.nuget/packages" \
  "${BUILD_SOURCESDIRECTORY:-}/src/server" \
  "${BUILD_SOURCESDIRECTORY:-}/src/client/ui/node_modules" \
  "${HOME:-}/.cache/ms-playwright" \
  "${HOME:-}/.dapr" \
  "${AGENT_TEMPDIRECTORY:-}"; do
  if [ -d "$path" ]; then
    du -sh "$path" || true
  fi
done

if command -v docker >/dev/null 2>&1; then
  # Include per-image unique/shared layer sizes; image-list SIZE alone double-counts shared layers.
  timeout 20s docker system df -v || true
fi
printf '=== End disk usage snapshot ===\n\n'
