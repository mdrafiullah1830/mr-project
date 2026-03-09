#!/bin/bash

# MR Shop - START ALL SERVERS
# Run this script to start all services needed for MR Shop

PROJECT_DIR='/Users/mdrafiullah/Desktop/mr project '

echo "🚀 Starting MR Shop Services..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

# Start HTTP Server (Frontend)
echo "1️⃣  Starting HTTP Server (port 8000)..."
cd "$PROJECT_DIR"
nohup python3 -m http.server 8000 > /tmp/server.log 2>&1 &
HTTP_PID=$!
echo "   ✅ HTTP Server started (PID: $HTTP_PID)"

# Start Flask Chat API
echo "2️⃣  Starting Chat API Server (port 5001)..."
nohup "/Users/mdrafiullah/Desktop/mr project /.venv/bin/python" backend/chat_api.py > /tmp/chat_api.log 2>&1 &
CHAT_PID=$!
echo "   ✅ Chat API started (PID: $CHAT_PID)"

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "✅ All servers started successfully!"
echo ""
echo "📱 Access points:"
echo "   🏠 Store:  http://localhost:8000/assets/html/index.html"
echo "   🔐 Auth:   http://localhost:8000/assets/html/auth.html"
echo "   💬 Chat:   http://localhost:8000/assets/html/chat.html"
echo "   🧪 OAuth:  http://localhost:8000/assets/html/complete-oauth-demo.html"
echo ""
echo "📊 Server status:"
echo "   HTTP (Frontend):  http://localhost:8000/ ✓"
echo "   Chat API:         http://localhost:5001/ ✓"
echo ""
echo "To stop all servers, run: pkill -f 'http.server|chat_api.py'"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
