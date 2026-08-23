"""Regression tests for API versioning inputs."""

from pathlib import Path
import json
import unittest


REPO_ROOT = Path(__file__).resolve().parents[2]


class ServerVersionFilterTests(unittest.TestCase):
    def test_server_version_tracks_infrastructure_changes(self) -> None:
        version = json.loads(
            (REPO_ROOT / "src/server/version.json").read_text(encoding="utf-8")
        )

        self.assertIn(":/infra", version["pathFilters"])


if __name__ == "__main__":
    unittest.main()
