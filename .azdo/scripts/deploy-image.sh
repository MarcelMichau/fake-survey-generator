#!/usr/bin/env bash
set -euo pipefail

: "${API_VERSION:?The published release version is required}"
: "${IMAGE_DIGEST:?The published image digest is required; rebuilding in deployment is forbidden}"
if [[ ! "$IMAGE_DIGEST" =~ ^[a-z0-9]+\.azurecr\.io/[a-z0-9_./-]+@sha256:[a-f0-9]{64}$ ]]; then
    echo "Expected a fully qualified ACR image digest, not a mutable image tag." >&2
    exit 1
fi

bash "$(dirname "${BASH_SOURCE[0]}")/refresh-azd-environment.sh"
registry=$(azd env get-value AZURE_CONTAINER_REGISTRY_ENDPOINT)
if [[ "$IMAGE_DIGEST" != "$registry/"* ]]; then
    echo "Candidate image is not from this environment's registry." >&2
    exit 1
fi

# The fully qualified remote image makes azd skip both packaging and registry publication.
# infra/api.bicepparam still supplies API_VERSION for the unique Container Apps revision suffix.
azd deploy api --from-package "$IMAGE_DIGEST" --no-prompt
