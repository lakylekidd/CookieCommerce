"""Validate release context and collect immutable service receipts; never deploy or read credentials."""
import json
import os
from pathlib import Path
import re
import sys


def require(condition, message):
    if not condition:
        raise ValueError(message)


def context(config):
    require(os.environ.get("CI_COMMIT_REF_PROTECTED") == "true", "A protected release tag is required.")
    version = os.environ.get("CI_COMMIT_TAG", "")
    require(re.fullmatch(r"v(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)", version), "Invalid stable release version.")
    require(os.environ.get("CI_PROJECT_PATH") == config["projectPath"], "Project scope mismatch.")
    require(os.environ.get("CI_REGISTRY_IMAGE") == config["registryPrefix"], "Registry prefix mismatch.")
    require(os.environ.get("CI_REGISTRY") == config["registryHost"], "Registry hostname mismatch.")
    require(re.fullmatch(r"(?:[a-f0-9]{40}|[a-f0-9]{64})", os.environ.get("CI_COMMIT_SHA", "")), "Invalid commit SHA.")
    for name in ("CI_PIPELINE_ID", "CI_PROJECT_ID"):
        require(re.fullmatch(r"[1-9][0-9]*", os.environ.get(name, "")), "Invalid release provenance.")
    if config.get("projectId"):
        require(os.environ["CI_PROJECT_ID"] == config["projectId"], "Project ID mismatch.")
    return version


def collect(config):
    version = context(config)
    artifacts = []
    for service in config["services"]:
        path = Path("release", service + ".json")
        require(path.is_file() and not path.is_symlink() and path.stat().st_size < 65536, "Missing or oversized service receipt.")
        receipt = json.loads(path.read_text(encoding="utf-8"))
        repository = config["registryPrefix"] + "/" + service
        require(receipt.get("schema") == 1 and receipt.get("service") == service, "Invalid service receipt identity.")
        require(receipt.get("repository") == repository and receipt.get("version") == version, "Artifact scope/version mismatch.")
        digest = receipt.get("digest", "")
        require(re.fullmatch(r"sha256:[a-f0-9]{64}", digest), "An immutable image digest is required.")
        require(receipt.get("imageReference") == repository + "@" + digest, "Mutable or mismatched image reference.")
        for field, variable in (("commit", "CI_COMMIT_SHA"), ("pipelineId", "CI_PIPELINE_ID"), ("projectId", "CI_PROJECT_ID")):
            require(receipt.get(field) == os.environ[variable], "Artifact provenance mismatch.")
        artifacts.append(receipt)
    release = {"schema": 1, "projectPath": config["projectPath"], "version": version,
               "commit": os.environ["CI_COMMIT_SHA"], "pipelineId": os.environ["CI_PIPELINE_ID"],
               "projectId": os.environ["CI_PROJECT_ID"], "artifacts": artifacts}
    Path("release/release.json").write_text(json.dumps(release, indent=2) + "\n", encoding="utf-8")


def main():
    config = json.loads(Path(".platform/ci/release-config.json").read_text(encoding="utf-8"))
    require(len(sys.argv) == 2 and sys.argv[1] in ("validate", "collect"), "Use validate or collect.")
    if sys.argv[1] == "validate":
        context(config)
    else:
        collect(config)


if __name__ == "__main__":
    try:
        main()
    except (ValueError, KeyError, OSError, TypeError):
        print("Release validation failed: check the protected tag, project scope and complete immutable receipts.", file=sys.stderr)
        sys.exit(1)
