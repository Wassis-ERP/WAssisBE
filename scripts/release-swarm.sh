#!/usr/bin/env bash
# Run manually on an authorized Swarm manager. Never called from an unauthenticated webhook.
set -euo pipefail
: "${IMAGE:?Immutable image@sha256:digest required}"
: "${BUILD_SHA:?Commit SHA required}"
: "${API_SERVICE:?Existing API service required}"
: "${NETWORK:?Existing database overlay network required}"
: "${DB_SECRET:?Versioned Docker database secret required}"
: "${READY_URL:?HTTPS readiness endpoint required}"
[[ "$IMAGE" =~ @sha256:[a-f0-9]{64}$ ]] || { echo 'An immutable image digest is required.'; exit 1; }
[[ "$BUILD_SHA" =~ ^[a-f0-9]{40}$ ]] || { echo 'A full commit SHA is required.'; exit 1; }
export BUILD_SHA
[[ "$READY_URL" == https://* ]] || { echo 'Readiness URL must use HTTPS.'; exit 1; }
old_image=$(docker service inspect --format '{{.Spec.TaskTemplate.ContainerSpec.Image}}' "$API_SERVICE")
printf 'Release SHA=%s image=%s previous=%s\n' "$BUILD_SHA" "$IMAGE" "$old_image"
run_job() {
  local argument="$1" job="wassis-schema-${BUILD_SHA:0:12}-${2}-$(date +%s)" task state
  docker service create --quiet --name "$job" --mode replicated-job --replicas 1 --restart-condition none \
    --with-registry-auth --network "$NETWORK" --secret "source=$DB_SECRET,target=wassis_db_connection" \
    --env ASPNETCORE_ENVIRONMENT=Staging --env "BUILD_SHA=$BUILD_SHA" "$IMAGE" "$argument" >/dev/null
  for _ in $(seq 1 120); do
    task=$(docker service ps --no-trunc --format '{{.ID}}' "$job" | head -n 1)
    state=$(docker inspect --format '{{.Status.State}}' "$task" 2>/dev/null || true)
    case "$state" in
      complete) docker service logs "$job"; docker service rm "$job" >/dev/null; return 0 ;;
      failed|rejected) echo "Schema job failed: $job. API rollout blocked; inspect task locally."; return 1 ;;
    esac
    sleep 5
  done
  echo "Schema job timed out: $job. API rollout blocked; inspect task locally."; return 1
}
run_job --migrate migrate
run_job --validate-schema validate
docker service update --image "$IMAGE" --with-registry-auth "$API_SERVICE" >/dev/null
for _ in $(seq 1 24); do
  if curl --fail --silent --max-time 10 "$READY_URL" | python3 -c 'import json,os,sys; x=json.load(sys.stdin); sys.exit(0 if x.get("status")=="ready" and x.get("buildSha")==os.environ["BUILD_SHA"] else 1)' 2>/dev/null; then
    echo "Ready: $BUILD_SHA"; exit 0
  fi
  sleep 5
done
echo "Readiness failed. Review migration compatibility before restoring image: $old_image"
exit 1
