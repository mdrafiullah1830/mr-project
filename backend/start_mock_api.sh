#!/usr/bin/env bash
set -euo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"
echo "Starting MR Shop mock API (Flask) on port 5010"
python3 "$HERE/mock_api.py"
