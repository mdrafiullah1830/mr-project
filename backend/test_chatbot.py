#!/usr/bin/env python3
"""
MR Shop AI Chatbot - Test Script
=================================
Quick test script to verify chatbot is working correctly
"""

import requests
import json
from pathlib import Path
import sys
import time

API_URL = "http://localhost:5001"

# Test queries in different languages
TEST_QUERIES = [
    # Bangla queries
    {
        "message": "আপনাদের মধু কত দামে পাওয়া যায়?",
        "language": "Bangla",
        "expected_category": "pricing"
    },
    {
        "message": "ডেলিভারি কত দিনে হয়?",
        "language": "Bangla",
        "expected_category": "delivery"
    },
    {
        "message": "পণ্য ফেরত দেওয়া যায় কিনা?",
        "language": "Bangla",
        "expected_category": "returns"
    },
    
    # English queries
    {
        "message": "What honey products do you have?",
        "language": "English",
        "expected_category": "pricing"
    },
    {
        "message": "How long does delivery take?",
        "language": "English",
        "expected_category": "delivery"
    },
    {
        "message": "What is your return policy?",
        "language": "English",
        "expected_category": "returns"
    },
]

def print_header(text):
    """Print formatted header"""
    print("\n" + "=" * 70)
    print(f"  {text}")
    print("=" * 70)

def print_result(query, response, success):
    """Print test result"""
    status = "✅ PASS" if success else "❌ FAIL"
    
    print(f"\n{status}")
    print(f"Query: {query['message']}")
    print(f"Language: {query['language']}")
    
    if success:
        print(f"\nResponse:")
        print(f"  Text: {response['response'][:100]}...")
        print(f"  Confidence: {response['confidence']:.2%}")
        print(f"  Category: {response['category']}")
        print(f"  Expected: {query['expected_category']}")
        
        if response['category'] == query['expected_category']:
            print("  ✅ Category matches!")
        else:
            print(f"  ⚠️  Category mismatch (got {response['category']}, expected {query['expected_category']})")
    else:
        print(f"Error: {response.get('detail', 'Unknown error')}")

def test_health():
    """Test health check endpoint"""
    print_header("Testing Health Check")
    
    try:
        response = requests.get(f"{API_URL}/api/chat/health", timeout=5)
        
        if response.status_code == 200:
            data = response.json()
            print(f"✅ Health Check Passed")
            print(f"   Status: {data['status']}")
            print(f"   Model Loaded: {data['model_loaded']}")
            print(f"   Data Points: {data['data_points']}")
            return True
        else:
            print(f"❌ Health Check Failed (Status: {response.status_code})")
            return False
    except requests.exceptions.ConnectionError:
        print(f"❌ Could not connect to API at {API_URL}")
        print("   Make sure the server is running:")
        print("   python3 -m uvicorn backend.chatbot_api:app --host 0.0.0.0 --port 5001")
        return False
    except Exception as e:
        print(f"❌ Error: {str(e)}")
        return False

def test_chat():
    """Test chat endpoint with multiple queries"""
    print_header("Testing Chat Endpoint")
    
    passed = 0
    failed = 0
    
    for i, query in enumerate(TEST_QUERIES, 1):
        try:
            response = requests.post(
                f"{API_URL}/api/chat",
                json={"message": query["message"]},
                timeout=10
            )
            
            if response.status_code == 200:
                data = response.json()
                print_result(query, data, True)
                
                # Check confidence
                if data['confidence'] >= 0.7:
                    passed += 1
                else:
                    print(f"  ⚠️  Low confidence score: {data['confidence']:.2%}")
                    failed += 1
            else:
                print_result(query, response.json(), False)
                failed += 1
                
        except requests.exceptions.Timeout:
            print_result(query, {"detail": "Request timeout"}, False)
            failed += 1
        except Exception as e:
            print_result(query, {"detail": str(e)}, False)
            failed += 1
    
    return passed, failed

def test_api_info():
    """Test API info endpoint"""
    print_header("Testing API Info")
    
    try:
        response = requests.get(f"{API_URL}/", timeout=5)
        
        if response.status_code == 200:
            data = response.json()
            print(f"✅ API Info Retrieved")
            print(f"   Service: {data['service']}")
            print(f"   Version: {data['version']}")
            print(f"   Features: {', '.join(data['features'][:2])}...")
            return True
        else:
            print(f"❌ Failed to get API info (Status: {response.status_code})")
            return False
    except Exception as e:
        print(f"❌ Error: {str(e)}")
        return False

def main():
    """Main test runner"""
    print("\n")
    print("╔" + "=" * 68 + "╗")
    print("║" + " " * 15 + "🤖 MR SHOP AI CHATBOT - TEST SUITE" + " " * 20 + "║")
    print("╚" + "=" * 68 + "╝")
    
    print(f"\nAPI Endpoint: {API_URL}")
    print(f"Time: {time.strftime('%Y-%m-%d %H:%M:%S')}")
    
    # Run tests
    health_ok = test_health()
    
    if not health_ok:
        print("\n❌ Cannot proceed - API not responding")
        return False
    
    api_info_ok = test_api_info()
    chat_passed, chat_failed = test_chat()
    
    # Summary
    print_header("Test Summary")
    
    total_tests = 1 + 1 + len(TEST_QUERIES)
    total_passed = (1 if health_ok else 0) + (1 if api_info_ok else 0) + chat_passed
    total_failed = (0 if health_ok else 1) + (0 if api_info_ok else 1) + chat_failed
    
    print(f"\nTotal Tests: {total_tests}")
    print(f"✅ Passed: {total_passed}")
    print(f"❌ Failed: {total_failed}")
    print(f"Success Rate: {(total_passed/total_tests)*100:.1f}%")
    
    if total_failed == 0:
        print("\n🎉 All tests passed! Chatbot is working perfectly.")
        return True
    else:
        print(f"\n⚠️  {total_failed} test(s) failed. Check the output above.")
        return False

if __name__ == "__main__":
    success = main()
    sys.exit(0 if success else 1)
