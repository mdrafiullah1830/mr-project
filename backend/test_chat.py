#!/usr/bin/env python3
"""
Test script for MR Shop AI Chat System
Run this to verify the chat API is working correctly
"""

import requests
import json
from datetime import datetime

API_URL = "http://localhost:5001/api/chat"
HEALTH_URL = "http://localhost:5001/api/health"

def print_banner():
    print("=" * 60)
    print("🧪 MR Shop AI Chat System - Test Suite")
    print("=" * 60)
    print()

def test_health_check():
    """Test if server is running"""
    print("1️⃣  Testing server health...")
    try:
        response = requests.get(HEALTH_URL, timeout=5)
        if response.status_code == 200:
            data = response.json()
            print("   ✅ Server is healthy!")
            print(f"   📊 Products loaded: {data.get('products_loaded', 0)}")
            print(f"   📁 Categories loaded: {data.get('categories_loaded', 0)}")
            return True
        else:
            print(f"   ❌ Server returned status code: {response.status_code}")
            return False
    except requests.exceptions.ConnectionError:
        print("   ❌ Cannot connect to server!")
        print("   💡 Make sure to run: python backend/chat_api.py")
        return False
    except Exception as e:
        print(f"   ❌ Error: {e}")
        return False

def test_chat_query(question):
    """Test a chat query"""
    print(f"\n💬 User: {question}")
    try:
        response = requests.post(
            API_URL,
            json={"message": question},
            headers={"Content-Type": "application/json"},
            timeout=10
        )
        
        if response.status_code == 200:
            data = response.json()
            bot_response = data.get('response', 'No response')
            print(f"🤖 Bot: {bot_response[:200]}...")
            print(f"   ✅ Type: {data.get('type', 'unknown')}")
            return True
        else:
            print(f"   ❌ Request failed with status: {response.status_code}")
            return False
            
    except Exception as e:
        print(f"   ❌ Error: {e}")
        return False

def run_tests():
    """Run all tests"""
    print_banner()
    
    # Test 1: Health check
    if not test_health_check():
        print("\n❌ Server is not running. Please start it first:")
        print("   cd backend")
        print("   python chat_api.py")
        return
    
    print("\n" + "=" * 60)
    print("2️⃣  Testing Chat Queries")
    print("=" * 60)
    
    # Test queries
    test_queries = [
        "Hello!",
        "What products do you sell?",
        "Tell me about your prices",
        "How does shipping work?",
        "What's your charity work?"
    ]
    
    passed = 0
    for query in test_queries:
        if test_chat_query(query):
            passed += 1
    
    # Summary
    print("\n" + "=" * 60)
    print("📊 Test Summary")
    print("=" * 60)
    print(f"✅ Passed: {passed + 1}/{len(test_queries) + 1}")
    print(f"❌ Failed: {len(test_queries) + 1 - passed - 1}/{len(test_queries) + 1}")
    
    if passed == len(test_queries):
        print("\n🎉 All tests passed! Your chat system is working perfectly!")
    else:
        print("\n⚠️  Some tests failed. Check the errors above.")
    
    print("\n💡 Try the chat interface:")
    print("   Open chat.html in your browser")
    print("=" * 60)

if __name__ == "__main__":
    run_tests()
