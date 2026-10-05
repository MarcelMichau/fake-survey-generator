#!/usr/bin/env bash
# Fresh hosted jobs must retrieve their own deployment outputs; never artifact .azure credentials.
set -euo pipefail

: "${AZURE_ENV_NAME:?AZURE_ENV_NAME is required}"
: "${AZURE_SUBSCRIPTION_ID:?AZURE_SUBSCRIPTION_ID is required}"
: "${AZURE_LOCATION:?AZURE_LOCATION is required}"
: "${TYPE_SAFE_API_KEY:?TYPE_SAFE_API_KEY is required to evaluate the Bicep parameters}"

azd config set auth.useAzCliAuth true
azd config set alpha.deployment.stacks on
azd env new "$AZURE_ENV_NAME" --subscription "$AZURE_SUBSCRIPTION_ID" --location "$AZURE_LOCATION" --no-prompt
if ! azd env refresh "$AZURE_ENV_NAME" --no-prompt; then
    echo "Could not refresh the existing environment. Bootstrap dev infrastructure before running parallel image publishing." >&2
    exit 1
fi
