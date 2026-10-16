#!/bin/sh
# Managed by homelab-platform. This job runs only on trusted protected release tags.
set -eu
set +x
fail() { printf '%s\n' "$1" >&2; exit 1; }
[ "${CI_DEBUG_TRACE:-false}" != "true" ] || fail 'Disable CI_DEBUG_TRACE for registry authentication.'
[ "${CI_COMMIT_REF_PROTECTED:-false}" = "true" ] || fail 'A protected release tag is required.'
printf '%s\n' "${CI_COMMIT_TAG:-}" | grep -Eq '^v(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$' || fail 'Invalid release version.'
[ "$CI_PROJECT_PATH" = "$PLATFORM_PROJECT_PATH" ] || fail 'Project path mismatch.'
[ "$CI_REGISTRY" = "$PLATFORM_REGISTRY_HOST" ] || fail 'Registry host mismatch.'
[ "$CI_REGISTRY_IMAGE" = "$PLATFORM_REGISTRY_PREFIX" ] || fail 'Registry prefix mismatch.'
printf '%s\n' "$CI_COMMIT_SHA" | grep -Eq '^([a-f0-9]{40}|[a-f0-9]{64})$' || fail 'Invalid commit SHA.'
printf '%s\n' "$CI_PIPELINE_ID" | grep -Eq '^[1-9][0-9]*$' || fail 'Invalid pipeline ID.'
printf '%s\n' "$CI_PROJECT_ID" | grep -Eq '^[1-9][0-9]*$' || fail 'Invalid project ID.'
printf '%s\n' "$PLATFORM_SERVICE" | grep -Eq '^[a-z][a-z0-9-]*$' || fail 'Invalid service name.'
[ -f "$PLATFORM_DOCKERFILE" ] && [ -d "$PLATFORM_BUILD_CONTEXT" ] || fail 'Declared Dockerfile or build context is missing.'
umask 077
DOCKER_CONFIG=$(mktemp -d)
export DOCKER_CONFIG
trap 'rm -f -- "$DOCKER_CONFIG/config.json"; rmdir -- "$DOCKER_CONFIG"' EXIT HUP INT TERM
auth=$(printf '%s:%s' "$CI_REGISTRY_USER" "$CI_REGISTRY_PASSWORD" | base64 | tr -d '\n')
printf '{"auths":{"%s":{"auth":"%s"}}}\n' "$CI_REGISTRY" "$auth" > "$DOCKER_CONFIG/config.json"
unset auth
mkdir -p release
metadata="release/$PLATFORM_SERVICE.buildkit.json"
buildctl-daemonless.sh build --frontend dockerfile.v0 \
  --local "context=$PLATFORM_BUILD_CONTEXT" \
  --local "dockerfile=$(dirname "$PLATFORM_DOCKERFILE")" \
  --opt "filename=$(basename "$PLATFORM_DOCKERFILE")" \
  --output "type=image,name=$CI_REGISTRY_IMAGE/$PLATFORM_SERVICE:$CI_COMMIT_TAG,push=true" \
  --metadata-file "$metadata"
digest=$(sed -n 's/^[[:space:]]*"containerimage\.digest":[[:space:]]*"\(sha256:[a-f0-9]\{64\}\)".*/\1/p' "$metadata")
[ "$(printf '%s\n' "$digest" | wc -l | tr -d ' ')" = "1" ] || fail 'BuildKit returned ambiguous digest metadata.'
printf '%s\n' "$digest" | grep -Eq '^sha256:[a-f0-9]{64}$' || fail 'BuildKit did not return one immutable sha256 image digest.'
printf '{"schema":1,"service":"%s","repository":"%s/%s","version":"%s","digest":"%s","imageReference":"%s/%s@%s","commit":"%s","pipelineId":"%s","projectId":"%s"}\n' \
  "$PLATFORM_SERVICE" "$CI_REGISTRY_IMAGE" "$PLATFORM_SERVICE" "$CI_COMMIT_TAG" "$digest" \
  "$CI_REGISTRY_IMAGE" "$PLATFORM_SERVICE" "$digest" "$CI_COMMIT_SHA" "$CI_PIPELINE_ID" "$CI_PROJECT_ID" > "release/$PLATFORM_SERVICE.json"
