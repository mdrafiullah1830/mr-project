#!/usr/bin/env python3
"""Simple mock API for local frontend testing.

Run: python3 mock_api.py

Provides endpoints used by the frontend for easy local testing when the real
backend is not running. Port: 5010
"""
from __future__ import annotations
import json
import os
from flask import Flask, jsonify, request
from flask_cors import CORS

ROOT = os.path.dirname(os.path.abspath(__file__))
DATA_DIR = os.path.join(ROOT, '..', 'data')
PRODUCTS_PATH = os.path.join(DATA_DIR, 'products.json')

app = Flask(__name__)
CORS(app)


def load_products():
    try:
        with open(PRODUCTS_PATH, 'r', encoding='utf-8') as f:
            return json.load(f)
    except Exception:
        return []


@app.route('/api/products', methods=['GET'])
def api_products():
    prods = load_products()
    return jsonify({'success': True, 'data': prods})


@app.route('/api/orders/user/<user_id>', methods=['GET'])
@app.route('/api/orders/<user_id>', methods=['GET'])
def api_orders(user_id):
    # Return a small list of orders for testing
    orders = [
        { 'id': '#MR5001', 'status': 'delivered', 'date': '2025-11-28', 'total': '1250', 'items': 3 },
        { 'id': '#MR5000', 'status': 'pending', 'date': '2025-11-27', 'total': '850', 'items': 2 }
    ]
    return jsonify({'success': True, 'data': orders})


@app.route('/api/orders', methods=['GET'])
def api_orders_query():
    user_id = request.args.get('userId')
    return api_orders(user_id or 'anonymous')


@app.route('/api/wishlist/<user_id>', methods=['GET'])
def api_wishlist(user_id):
    wishlist = [
        { 'id': 1, 'name': 'Antique Coin Collection', 'price': '450', 'image': 'https://picsum.photos/seed/wishlist1/300' },
        { 'id': 2, 'name': 'Handcrafted Saree', 'price': '2500', 'image': 'https://picsum.photos/seed/wishlist3/300' }
    ]
    return jsonify({'success': True, 'data': wishlist})


@app.route('/api/reviews/user/<user_id>', methods=['GET'])
def api_reviews(user_id):
    reviews = [
        { 'id': 1, 'product_id': 201, 'rating': 5, 'comment': 'Excellent!' },
        { 'id': 2, 'product_id': 202, 'rating': 4, 'comment': 'Comfortable' }
    ]
    return jsonify({'success': True, 'data': reviews})


@app.route('/api/profile/<user_id>/exists', methods=['GET'])
def api_profile_exists(user_id):
    return jsonify({'success': True, 'data': {'exists': True}})


@app.route('/api/profile/<user_id>', methods=['GET'])
def api_profile_get(user_id):
    profile = {
        'user_id': user_id,
        'full_name': 'Md. Rafi Ullah',
        'email_address': 'rafi@mrshop.com',
        'phone_number': '+8801712345678',
        'address': 'House 12, Road 5, Dhanmondi, Dhaka',
        'date_of_birth': '1995-05-15',
        'gender': 'male',
        'profile_photo_path': '/uploads/mock-profile.jpg'
    }
    return jsonify({'success': True, 'data': profile})


@app.route('/api/profile', methods=['POST'])
def api_profile_create():
    body = request.get_json() or {}
    return jsonify({'success': True, 'data': body, 'message': 'Profile created (mock)'}), 201


@app.route('/api/profile/<user_id>/photo', methods=['POST'])
def api_profile_photo(user_id):
    if 'file' not in request.files:
        return jsonify({'success': False, 'message': 'No file uploaded'}), 400
    file = request.files['file']
    filename = file.filename or f'{user_id}-photo.jpg'
    # For mock, we won't save file; return a fake path
    photo_path = f'/uploads/{filename}'
    return jsonify({'success': True, 'data': {'photo_path': photo_path}, 'message': 'Uploaded (mock)'}), 200


if __name__ == '__main__':
    print('Starting mock API on http://127.0.0.1:5010')
    app.run(host='0.0.0.0', port=5010, debug=True)
