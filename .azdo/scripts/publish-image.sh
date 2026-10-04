#!/usr/bin/env bash
# This script builds/pushes only. All live resource changes remain behind validation and migrations.
set -euo pipefail

: "${API_VERSION:?A unique release version is required}"
: "${VITE_APP_VERSION:?The application SemVer is required}"

bash "$(dirname "${BASH_SOURCE[0]}")/refresh-azd-environment.sh"
registry=$(azd env get-value AZURE_CONTAINER_REGISTRY_ENDPOINT)
if [[ ! "$registry" =~ ^[a-z0-9]+\.azurecr\.io$ ]]; then
    echo "Expected the existing Azure Container Registry endpoint; refusing to publish elsewhere." >&2
    exit 1
fi

azd publish api --no-prompt
image=$(azd env get-value SERVICE_API_IMAGE_NAME)
if [[ "$image" != "$registry/"* || "$image" != *":$API_VERSION" ]]; then
    echo "Published image does not match the expected registry and release version." >&2
    exit 1
fi

# Resolve once, then hand off the immutable digest rather than a mutable tag or an azd environment file.
digest=$(az acr repository show --name "${registry%%.*}" --image "${image#"$registry/"}" --query digest --output tsv)
if [[ ! "$digest" =~ ^sha256:[a-f0-9]{64}$ ]]; then
    echo "The registry did not return a valid image digest." >&2
    exit 1
fi
image_digest="${image%:*}@$digest"
echo "Candidate image: $image_digest"
echo "##vso[task.setvariable variable=ImageDigest;isOutput=true]$image_digest"
