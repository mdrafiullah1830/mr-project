#!/bin/bash

# MR Shop Chat Server Start Script
# Run this script to start the AI chat backend

cd "$(dirname "$0")"

echo "🚀 Starting MR Shop AI Chat Server..."
echo ""

# Check if virtual environment exists
if [ ! -d "venv" ]; then
    echo "⚠️  Virtual environment not found. Creating one..."
    python3 -m venv venv
    source venv/bin/activate
    echo "📦 Installing dependencies..."
    pip install -r requirements.txt
else
    source venv/bin/activate
fi

# Start the server
echo ""
echo "✅ Starting server on http://localhost:5001"
echo "📡 Chat API endpoint: http://localhost:5001/api/chat"
echo ""
echo "Press CTRL+C to stop the server"
echo "----------------------------------------"
echo ""

python chat_api.py
