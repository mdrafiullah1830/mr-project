#!/usr/bin/env bash
set -euo pipefail

# Uninstall MR Shop auto-start services on macOS (launchd)

LAUNCH_AGENTS_DIR="${HOME}/Library/LaunchAgents"
HTTP_PLIST="${LAUNCH_AGENTS_DIR}/com.mrshop.httpserver.plist"
CHAT_PLIST="${LAUNCH_AGENTS_DIR}/com.mrshop.chatapi.plist"
DOMAIN="gui/$(id -u)"

launchctl bootout "${DOMAIN}" "com.mrshop.httpserver" 2>/dev/null || true
launchctl bootout "${DOMAIN}" "com.mrshop.chatapi" 2>/dev/null || true
launchctl bootout "${DOMAIN}" "${HTTP_PLIST}" 2>/dev/null || true
launchctl bootout "${DOMAIN}" "${CHAT_PLIST}" 2>/dev/null || true

rm -f "${HTTP_PLIST}" "${CHAT_PLIST}"

echo "✅ Uninstalled launchd auto-start services."
