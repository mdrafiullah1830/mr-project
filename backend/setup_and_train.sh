#!/bin/bash

# MR Shop AI Chatbot - Complete Setup Script
# =============================================
# This script will:
# 1. Install all Python dependencies
# 2. Train the chatbot model
# 3. Start the FastAPI server

PROJECT_DIR='/Users/mdrafiullah/Desktop/mr project '
BACKEND_DIR="$PROJECT_DIR/backend"
VENV_PYTHON="$PROJECT_DIR/.venv/bin/python"

echo "======================================================================="
echo "🤖 MR Shop AI Chatbot - Complete Setup"
echo "======================================================================="
echo ""

# Step 1: Check Python
echo "1️⃣  Checking Python environment..."
"$VENV_PYTHON" --version
echo "✅ Python ready\n"

# Step 2: Install dependencies
echo "2️⃣  Installing Python packages..."
echo "   This may take a few minutes on first run..."
echo ""

PACKAGES=(
    "fastapi"
    "uvicorn"
    "pydantic"
    "sentence-transformers"
    "torch"
    "transformers"
    "numpy"
    "scikit-learn"
)

for package in "${PACKAGES[@]}"; do
    echo "   📥 Installing $package..."
    "$VENV_PYTHON" -m pip install "$package" -q 2>&1 | grep -v "already satisfied" || true
done

echo "✅ All dependencies installed\n"

# Step 3: Train model
echo "3️⃣  Training chatbot model..."
cd "$BACKEND_DIR"
"$VENV_PYTHON" train_chatbot_model.py
echo ""

# Step 4: Start server
echo "4️⃣  Starting FastAPI server..."
echo ""
echo "======================================================================="
echo "✅ Setup complete! Starting Chatbot API on http://localhost:5001"
echo "======================================================================="
echo ""
echo "📚 API Documentation: http://localhost:5001/docs"
echo "🧪 Test the API: curl -X POST http://localhost:5001/api/chat -H \"Content-Type: application/json\" -d '{\"message\": \"মধু কত দামে পাওয়া যায়?\"}''"
echo ""
echo "To stop the server, press Ctrl+C"
echo "======================================================================="
echo ""

# Start the server
cd "$PROJECT_DIR"
"$VENV_PYTHON" -m uvicorn backend.chatbot_api:app --host 0.0.0.0 --port 5001 --reload
