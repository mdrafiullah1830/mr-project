#!/usr/bin/env python3
"""
MR Shop Chat API Backend
Simple Flask-based chat API for AI assistant responses
Runs on http://localhost:5001
"""

from flask import Flask, request, jsonify
from flask_cors import CORS
import json
import os
from datetime import datetime

app = Flask(__name__)
CORS(app)  # Enable CORS for all routes

# Sample responses based on product categories and common queries
CHAT_RESPONSES = {
    'honey': [
        'We have pure raw honey, organic honey with nuts, and raw Manuka honey available. Which one interests you?',
        'Our honey products are 100% pure and sourced from trusted local beekeepers. Would you like details on pricing?',
        'Try our Raw Manuka Honey - it\'s premium quality and priced at 1299৳.'
    ],
    'product': [
        'We have a wide range of products including groceries, electronics, clothing, and more. What are you looking for?',
        'Browse our categories to find exactly what you need. Each product has detailed descriptions and reviews.',
        'Our products are carefully selected for quality and value. Check out our search feature to find something specific!'
    ],
    'price': [
        'Prices vary by product. Use our search feature to check prices for specific items.',
        'We offer competitive pricing on all our products. You can filter by price range in our store.',
        'Each product displays its price clearly. We also offer discounts on bulk orders!'
    ],
    'order': [
        'To place an order, add items to your cart and proceed to checkout. You can track your order once placed.',
        'Orders are processed within 24 hours. You\'ll receive tracking information via email.',
        'We offer fast delivery to most areas. Delivery time depends on your location.'
    ],
    'delivery': [
        'We deliver to most areas in the city. Delivery typically takes 2-3 business days.',
        'Standard delivery is free on orders over 500৳. Express delivery is available for urgent orders.',
        'You can track your delivery in real-time through your order dashboard.'
    ],
    'payment': [
        'We accept cash on delivery, credit/debit cards, and mobile banking payments.',
        'All payments are processed securely using industry-standard encryption.',
        'You can save your payment methods for faster checkout next time!'
    ],
    'return': [
        'We have a 7-day return policy for unused items. Contact our support team to initiate a return.',
        'Returns are easy! Just contact us with your order number and reason for return.',
        'Refunds are processed within 3-5 business days after we receive the returned item.'
    ],
    'account': [
        'You can create an account for faster checkout and order tracking. Sign in with email or Google.',
        'Your account stores your addresses, payment methods, and order history.',
        'You\'ll earn rewards points on every purchase if you have an account!'
    ],
    'search': [
        'Use our powerful search feature to find products by name, category, or description.',
        'Search suggestions help you find related products quickly.',
        'Filters help you narrow down results by price, rating, and availability.'
    ],
    'default': [
        'I\'m here to help! You can ask me about products, orders, delivery, payments, and more. What would you like to know?',
        'Feel free to ask about our products, services, or policies. How can I assist you?',
        'I can help you with shopping questions. What\'s on your mind?',
        'Welcome to MR Shop AI Assistant! Ask me anything about our store.',
        'How can I make your shopping experience better today?'
    ]
}

def get_chat_response(user_message):
    """
    Generate a response based on user message
    Matches keywords to provide relevant responses
    """
    message_lower = user_message.lower()
    
    # Check for keyword matches
    for keyword, responses in CHAT_RESPONSES.items():
        if keyword in message_lower:
            return responses[0]  # Return first response for that keyword
    
    # If no keyword match, return a default response
    import random
    return random.choice(CHAT_RESPONSES['default'])

@app.route('/api/chat', methods=['POST'])
def chat():
    """
    Chat endpoint - receives user message and returns AI response
    Expected JSON: { "message": "user message here" }
    Returns: { "response": "AI response" }
    """
    try:
        data = request.get_json()
        
        if not data or 'message' not in data:
            return jsonify({
                'error': 'Invalid request. Please provide a message.',
                'response': 'I didn\'t understand that. Please rephrase your question.'
            }), 400
        
        user_message = data.get('message', '').strip()
        
        if not user_message:
            return jsonify({
                'response': 'Please type a message to get started!'
            }), 200
        
        # Generate response
        ai_response = get_chat_response(user_message)
        
        return jsonify({
            'response': ai_response,
            'timestamp': datetime.now().isoformat(),
            'user_message': user_message
        }), 200
        
    except Exception as e:
        print(f'Error: {str(e)}')
        return jsonify({
            'error': str(e),
            'response': 'Sorry, I encountered an error. Please try again.'
        }), 500

@app.route('/api/chat/health', methods=['GET'])
def health():
    """Health check endpoint"""
    return jsonify({
        'status': 'ok',
        'service': 'MR Shop Chat API',
        'version': '1.0.0'
    }), 200

@app.route('/', methods=['GET'])
def index():
    """Root endpoint info"""
    return jsonify({
        'service': 'MR Shop Chat API',
        'version': '1.0.0',
        'endpoints': {
            'chat': 'POST /api/chat - Send message and get response',
            'health': 'GET /api/chat/health - Health check'
        },
        'example': {
            'request': {'message': 'What honey products do you have?'},
            'response': {'response': 'We have pure raw honey, organic honey with nuts, and raw Manuka honey available. Which one interests you?'}
        }
    }), 200

if __name__ == '__main__':
    bind_host = os.getenv('MRSHOP_CHAT_BIND', '127.0.0.1')
    bind_port = int(os.getenv('MRSHOP_CHAT_PORT', '5001'))
    debug_env = os.getenv('MRSHOP_CHAT_DEBUG', '1').strip().lower()
    debug_mode = debug_env in ('1', 'true', 'yes', 'on')

    print('='*60)
    print('🚀 MR Shop Chat API Server')
    print('='*60)
    print(f'Starting Flask server on http://{bind_host}:{bind_port}')
    print('Endpoints:')
    print('  - POST /api/chat')
    print('  - GET  /api/chat/health')
    print('  - GET  /')
    print('='*60)
    print('Press Ctrl+C to stop the server\n')
    
    app.run(
        host=bind_host,
        port=bind_port,
        debug=debug_mode,
        use_reloader=False
    )
