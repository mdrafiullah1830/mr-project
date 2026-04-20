#!/usr/bin/env bash
set -euo pipefail

# Install MR Shop auto-start services on macOS (launchd)
# - Frontend static server (http.server) on port 8000
# - Chat API (Flask) on port 5001
#
# After install, the services start at login automatically.

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
LAUNCH_AGENTS_DIR="${HOME}/Library/LaunchAgents"
LOG_DIR="${HOME}/Library/Logs/mrshop"

HTTP_PORT="${MRSHOP_HTTP_PORT:-8000}"
CHAT_PORT="${MRSHOP_CHAT_PORT:-5001}"

VENV_PY="${ROOT_DIR}/.venv/bin/python"
if [[ ! -x "${VENV_PY}" ]]; then
  echo "❌ Could not find executable venv python at: ${VENV_PY}"
  echo "   Create it first (example):"
  echo "     python3 -m venv .venv"
  echo "     .venv/bin/python -m pip install flask flask-cors"
  exit 1
fi

# Some macOS setups (notably iCloud-synced Desktop) can leave quarantine xattrs
# on venv files, which may break Python imports when run under launchd.
if command -v xattr >/dev/null 2>&1; then
  xattr -dr com.apple.quarantine "${ROOT_DIR}/.venv" 2>/dev/null || true
fi

mkdir -p "${LAUNCH_AGENTS_DIR}" "${LOG_DIR}"

HTTP_PLIST="${LAUNCH_AGENTS_DIR}/com.mrshop.httpserver.plist"
CHAT_PLIST="${LAUNCH_AGENTS_DIR}/com.mrshop.chatapi.plist"

DOMAIN="gui/$(id -u)"

# Best-effort unload if already installed
launchctl bootout "${DOMAIN}" "com.mrshop.httpserver" 2>/dev/null || true
launchctl bootout "${DOMAIN}" "com.mrshop.chatapi" 2>/dev/null || true
launchctl bootout "${DOMAIN}" "${HTTP_PLIST}" 2>/dev/null || true
launchctl bootout "${DOMAIN}" "${CHAT_PLIST}" 2>/dev/null || true

# Write LaunchAgents
cat > "${HTTP_PLIST}" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>Label</key>
  <string>com.mrshop.httpserver</string>

  <key>WorkingDirectory</key>
  <string>${ROOT_DIR}</string>

  <key>ProgramArguments</key>
  <array>
    <string>${VENV_PY}</string>
    <string>-m</string>
    <string>http.server</string>
    <string>--bind</string>
    <string>127.0.0.1</string>
    <string>${HTTP_PORT}</string>
  </array>

  <key>RunAtLoad</key>
  <true/>

  <key>KeepAlive</key>
  <true/>

  <key>StandardOutPath</key>
  <string>${LOG_DIR}/httpserver.out.log</string>

  <key>StandardErrorPath</key>
  <string>${LOG_DIR}/httpserver.err.log</string>
</dict>
</plist>
EOF

cat > "${CHAT_PLIST}" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>Label</key>
  <string>com.mrshop.chatapi</string>

  <key>WorkingDirectory</key>
  <string>${ROOT_DIR}</string>

  <key>ProgramArguments</key>
  <array>
    <string>${VENV_PY}</string>
    <string>${ROOT_DIR}/backend/chat_api.py</string>
  </array>

  <key>EnvironmentVariables</key>
  <dict>
    <key>MRSHOP_CHAT_BIND</key>
    <string>127.0.0.1</string>
    <key>MRSHOP_CHAT_PORT</key>
    <string>${CHAT_PORT}</string>
    <key>MRSHOP_CHAT_DEBUG</key>
    <string>0</string>
    <key>PYTHONUNBUFFERED</key>
    <string>1</string>
  </dict>

  <key>RunAtLoad</key>
  <true/>

  <key>KeepAlive</key>
  <true/>

  <key>StandardOutPath</key>
  <string>${LOG_DIR}/chatapi.out.log</string>

  <key>StandardErrorPath</key>
  <string>${LOG_DIR}/chatapi.err.log</string>
</dict>
</plist>
EOF

# Load + start
launchctl bootstrap "${DOMAIN}" "${HTTP_PLIST}"
launchctl bootstrap "${DOMAIN}" "${CHAT_PLIST}"

launchctl kickstart -k "${DOMAIN}/com.mrshop.httpserver" || true
launchctl kickstart -k "${DOMAIN}/com.mrshop.chatapi" || true

echo "✅ Installed launchd auto-start services."
echo "   Frontend: http://localhost:${HTTP_PORT}/assets/html/index.html"
echo "   Chat API: http://localhost:${CHAT_PORT}/api/chat/health"
echo ""
echo "Logs: ${LOG_DIR}"
echo "Check status: launchctl print ${DOMAIN}/com.mrshop.chatapi"
