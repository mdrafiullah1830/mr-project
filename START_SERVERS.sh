#!/usr/bin/env bash
set -euo pipefail

# MR Shop - START ALL SERVERS (local dev)
# Starts:
# - Frontend static server (port 8000)
# - Flask Chat API (port 5001)

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LOG_DIR="${ROOT_DIR}/.logs"

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

mkdir -p "${LOG_DIR}"

is_listening() {
	local port="$1"
	lsof -nP -iTCP:"${port}" -sTCP:LISTEN >/dev/null 2>&1
}

echo "🚀 Starting MR Shop Services..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

# Start HTTP Server (Frontend)
echo "1️⃣  Starting HTTP Server (port ${HTTP_PORT})..."
if is_listening "${HTTP_PORT}"; then
	echo "   ℹ️  HTTP Server already running on port ${HTTP_PORT}"
else
	cd "${ROOT_DIR}"
	nohup "${VENV_PY}" -m http.server --bind 127.0.0.1 "${HTTP_PORT}" > "${LOG_DIR}/httpserver.log" 2>&1 &
	HTTP_PID=$!
	echo "${HTTP_PID}" > "${LOG_DIR}/httpserver.pid"
	echo "   ✅ HTTP Server started (PID: ${HTTP_PID})"
fi

# Start Flask Chat API
echo "2️⃣  Starting Chat API Server (port ${CHAT_PORT})..."
if is_listening "${CHAT_PORT}"; then
	echo "   ℹ️  Chat API already running on port ${CHAT_PORT}"
else
	cd "${ROOT_DIR}"
	nohup "${VENV_PY}" backend/chat_api.py > "${LOG_DIR}/chat_api.log" 2>&1 &
	CHAT_PID=$!
	echo "${CHAT_PID}" > "${LOG_DIR}/chat_api.pid"
	echo "   ✅ Chat API started (PID: ${CHAT_PID})"
fi

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "✅ Servers are up (or already running)."
echo ""
echo "📱 Access points:"
echo "   🏠 Store:  http://localhost:${HTTP_PORT}/assets/html/index.html"
echo "   🔐 Auth:   http://localhost:${HTTP_PORT}/assets/html/auth.html"
echo "   💬 Chat:   http://localhost:${HTTP_PORT}/assets/html/chat.html"
echo "   🧪 OAuth:  http://localhost:${HTTP_PORT}/assets/html/complete-oauth-demo.html"
echo ""
echo "📊 Health checks:"
echo "   HTTP (Frontend):  http://localhost:${HTTP_PORT}/ ✓"
echo "   Chat API health:  http://localhost:${CHAT_PORT}/api/chat/health ✓"
echo ""
echo "Logs: ${LOG_DIR}"
echo "To stop: pkill -f 'http.server|chat_api.py'"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
