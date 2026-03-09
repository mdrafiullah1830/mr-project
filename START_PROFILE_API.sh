#!/bin/bash

# Start Profile API Server
# This script starts the Python Flask server for user profile persistence

echo "🚀 Starting MR Shop Profile API Server..."
echo "=================================================="

# Check if Python is installed
if ! command -v python3 &> /dev/null; then
  echo "❌ Python 3 is not installed. Please install Python 3 first."
  exit 1
fi

# Check if Flask is installed
python3 -c "import flask" 2>/dev/null
if [ $? -ne 0 ]; then
  echo "⚙️ Installing Flask..."
  pip3 install flask flask-cors
fi

# Get the script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Start the Profile API
echo "📍 Starting Profile API on http://localhost:5002"
cd "$SCRIPT_DIR/backend"
python3 profile_api.py &
PROFILE_API_PID=$!

echo ""
echo "✅ Profile API started (PID: $PROFILE_API_PID)"
echo "=================================================="
echo "📊 API Endpoints:"
echo "   • GET  /api/profile/<user_id> - Get profile"
echo "   • POST /api/profile - Create/Update profile"
echo "   • PUT  /api/profile/<user_id> - Update fields"
echo "   • POST /api/profile/<user_id>/photo - Update photo"
echo "   • POST /api/profile/<user_id>/wishlist - Add to wishlist"
echo "   • POST /api/profile/<user_id>/recently-viewed - Add to viewed"
echo "   • POST /api/profile/<user_id>/orders - Add order"
echo "=================================================="
echo ""
echo "Press Ctrl+C to stop the server"
echo ""

# Wait for the process
wait $PROFILE_API_PID
