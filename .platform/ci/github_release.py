"""Build and collect immutable GitHub Actions image receipts."""

import json
import os
from pathlib import Path
import re
import sys


SERVICES = (
    "authentication-management-api",
    "customer-api",
    "infra-keycloak",
    "product-api",
    "web",
)
VERSION = re.compile(r"v(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\Z")
DIGEST = re.compile(r"sha256:[a-f0-9]{64}\Z")
COMMIT = re.compile(r"[a-f0-9]{40}\Z")
NUMBER = re.compile(r"[1-9][0-9]*\Z")


def require(condition, message):
    if not condition:
        raise ValueError(message)


def context():
    require(os.environ.get("GITHUB_REPOSITORY") == "lakylekidd/CookieCommerce", "Repository mismatch")
    version = os.environ.get("GITHUB_REF_NAME", "")
    require(VERSION.fullmatch(version) is not None, "Invalid release version")
    require(os.environ.get("GITHUB_REF") == "refs/tags/" + version, "Release ref mismatch")
    require(os.environ.get("GITHUB_REF_PROTECTED") == "true", "Release tag is not protected")
    require(COMMIT.fullmatch(os.environ.get("GITHUB_SHA", "")) is not None, "Invalid commit")
    for field in ("GITHUB_REPOSITORY_ID", "GITHUB_RUN_ID"):
        require(NUMBER.fullmatch(os.environ.get(field, "")) is not None, "Invalid release provenance")
    return version


def repository(service):
    require(service in SERVICES, "Unknown service")
    return "ghcr.io/lakylekidd/cookiecommerce-" + service


def emit(service, digest):
    version = context()
    require(DIGEST.fullmatch(digest) is not None, "Invalid image digest")
    image = repository(service)
    receipt = {
        "schema": 1,
        "service": service,
        "repository": image,
        "version": version,
        "digest": digest,
        "imageReference": image + "@" + digest,
        "commit": os.environ["GITHUB_SHA"],
        "pipelineId": os.environ["GITHUB_RUN_ID"],
        "projectId": os.environ["GITHUB_REPOSITORY_ID"],
    }
    target = Path("release")
    target.mkdir(exist_ok=True)
    (target / (service + ".json")).write_text(json.dumps(receipt, separators=(",", ":")) + "\n", encoding="utf-8")


def collect():
    version = context()
    artifacts = []
    for service in SERVICES:
        path = Path("release") / (service + ".json")
        require(path.is_file() and not path.is_symlink() and path.stat().st_size < 65536, "Missing service receipt")
        item = json.loads(path.read_text(encoding="utf-8"))
        digest = item.get("digest", "")
        image = repository(service)
        require(DIGEST.fullmatch(digest) is not None, "Invalid service digest")
        expected = {
            "schema": 1,
            "service": service,
            "repository": image,
            "version": version,
            "digest": digest,
            "imageReference": image + "@" + digest,
            "commit": os.environ["GITHUB_SHA"],
            "pipelineId": os.environ["GITHUB_RUN_ID"],
            "projectId": os.environ["GITHUB_REPOSITORY_ID"],
        }
        require(item == expected, "Service receipt provenance mismatch")
        artifacts.append(item)
    release = {
        "schema": 1,
        "projectPath": os.environ["GITHUB_REPOSITORY"],
        "version": version,
        "commit": os.environ["GITHUB_SHA"],
        "pipelineId": os.environ["GITHUB_RUN_ID"],
        "projectId": os.environ["GITHUB_REPOSITORY_ID"],
        "artifacts": artifacts,
    }
    target = Path("receipt-bundle/release")
    target.mkdir(parents=True, exist_ok=True)
    (target / "release.json").write_text(json.dumps(release, separators=(",", ":")) + "\n", encoding="utf-8")


def main():
    require(len(sys.argv) >= 2, "Expected command")
    if sys.argv[1] == "check" and len(sys.argv) == 2:
        context()
    elif sys.argv[1] == "emit" and len(sys.argv) == 4:
        emit(sys.argv[2], sys.argv[3])
    elif sys.argv[1] == "collect" and len(sys.argv) == 2:
        collect()
    else:
        raise ValueError("Invalid command")


if __name__ == "__main__":
    try:
        main()
    except (ValueError, KeyError, OSError, TypeError, json.JSONDecodeError) as error:
        print("Release receipt error: " + str(error), file=sys.stderr)
        sys.exit(1)
