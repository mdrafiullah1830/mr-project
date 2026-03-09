#!/usr/bin/env python3
"""
User Profile API for MR Shop
Handles user profile data persistence
"""

from flask import Flask, request, jsonify
from flask_cors import CORS
import json
import os
from datetime import datetime
from pathlib import Path

app = Flask(__name__)
CORS(app)

# Data file path
DATA_DIR = Path(__file__).parent / 'data'
PROFILES_FILE = DATA_DIR / 'user_profiles.json'

def load_profiles():
    """Load all user profiles from JSON file"""
    if not PROFILES_FILE.exists():
        return []
    try:
        with open(PROFILES_FILE, 'r', encoding='utf-8') as f:
            return json.load(f)
    except Exception as e:
        print(f"Error loading profiles: {e}")
        return []

def save_profiles(profiles):
    """Save all user profiles to JSON file"""
    try:
        PROFILES_FILE.parent.mkdir(parents=True, exist_ok=True)
        with open(PROFILES_FILE, 'w', encoding='utf-8') as f:
            json.dump(profiles, f, indent=2, ensure_ascii=False)
        return True
    except Exception as e:
        print(f"Error saving profiles: {e}")
        return False

def find_profile(user_id=None, username=None, email=None):
    """Find a user profile by ID, username, or email"""
    profiles = load_profiles()
    for profile in profiles:
        if user_id and profile.get('user_id') == str(user_id):
            return profile
        if username and profile.get('username') == username:
            return profile
        if email and profile.get('email_address') == email:
            return profile
    return None

# ===================== API ENDPOINTS =====================

@app.route('/api/profile/<user_id>', methods=['GET'])
def get_profile(user_id):
    """Get user profile by ID"""
    profile = find_profile(user_id=user_id)
    if profile:
        return jsonify({
            'success': True,
            'profile': profile
        })
    return jsonify({
        'success': False,
        'message': 'Profile not found'
    }), 404

@app.route('/api/profile/email/<email>', methods=['GET'])
def get_profile_by_email(email):
    """Get user profile by email"""
    profile = find_profile(email=email)
    if profile:
        return jsonify({
            'success': True,
            'profile': profile
        })
    return jsonify({
        'success': False,
        'message': 'Profile not found'
    }), 404

@app.route('/api/profile/username/<username>', methods=['GET'])
def get_profile_by_username(username):
    """Get user profile by username"""
    profile = find_profile(username=username)
    if profile:
        return jsonify({
            'success': True,
            'profile': profile
        })
    return jsonify({
        'success': False,
        'message': 'Profile not found'
    }), 404

@app.route('/api/profile', methods=['POST'])
def create_or_update_profile():
    """Create or update user profile"""
    data = request.get_json()
    
    if not data or not data.get('user_id'):
        return jsonify({
            'success': False,
            'message': 'user_id is required'
        }), 400
    
    user_id = str(data.get('user_id'))
    profiles = load_profiles()
    
    # Find existing profile
    existing_index = None
    for idx, profile in enumerate(profiles):
        if profile.get('user_id') == user_id:
            existing_index = idx
            break
    
    # Prepare profile data
    profile_data = {
        'user_id': user_id,
        'username': data.get('username'),
        'full_name': data.get('full_name', ''),
        'phone_number': data.get('phone_number', ''),
        'email_address': data.get('email_address', ''),
        'address': data.get('address', ''),
        'date_of_birth': data.get('date_of_birth'),
        'gender': data.get('gender'),
        'profile_photo_path': data.get('profile_photo_path'),
        'profile_photo_url': data.get('profile_photo_url'),
        'bio': data.get('bio'),
        'orders': data.get('orders', []),
        'wishlist': data.get('wishlist', []),
        'recently_viewed': data.get('recently_viewed', []),
        'reviews': data.get('reviews', []),
        'updated_at': datetime.utcnow().isoformat() + 'Z'
    }
    
    # Update or create
    if existing_index is not None:
        # Keep created_at
        profile_data['created_at'] = profiles[existing_index].get('created_at', profile_data['updated_at'])
        profiles[existing_index] = profile_data
        action = 'updated'
    else:
        profile_data['created_at'] = datetime.utcnow().isoformat() + 'Z'
        profiles.append(profile_data)
        action = 'created'
    
    # Save to file
    if save_profiles(profiles):
        return jsonify({
            'success': True,
            'message': f'Profile {action} successfully',
            'profile': profile_data
        })
    else:
        return jsonify({
            'success': False,
            'message': 'Error saving profile'
        }), 500

@app.route('/api/profile/<user_id>', methods=['PUT'])
def update_profile(user_id):
    """Update specific user profile fields"""
    data = request.get_json()
    profile = find_profile(user_id=user_id)
    
    if not profile:
        return jsonify({
            'success': False,
            'message': 'Profile not found'
        }), 404
    
    # Update fields
    if 'full_name' in data:
        profile['full_name'] = data['full_name']
    if 'phone_number' in data:
        profile['phone_number'] = data['phone_number']
    if 'address' in data:
        profile['address'] = data['address']
    if 'date_of_birth' in data:
        profile['date_of_birth'] = data['date_of_birth']
    if 'gender' in data:
        profile['gender'] = data['gender']
    if 'profile_photo_path' in data:
        profile['profile_photo_path'] = data['profile_photo_path']
    if 'profile_photo_url' in data:
        profile['profile_photo_url'] = data['profile_photo_url']
    if 'bio' in data:
        profile['bio'] = data['bio']
    
    profile['updated_at'] = datetime.utcnow().isoformat() + 'Z'
    
    profiles = load_profiles()
    for idx, p in enumerate(profiles):
        if p.get('user_id') == str(user_id):
            profiles[idx] = profile
            break
    
    if save_profiles(profiles):
        return jsonify({
            'success': True,
            'message': 'Profile updated successfully',
            'profile': profile
        })
    else:
        return jsonify({
            'success': False,
            'message': 'Error saving profile'
        }), 500

@app.route('/api/profile/<user_id>/photo', methods=['POST'])
def update_profile_photo(user_id):
    """Update user profile photo"""
    data = request.get_json()
    profile = find_profile(user_id=user_id)
    
    if not profile:
        return jsonify({
            'success': False,
            'message': 'Profile not found'
        }), 404
    
    profile['profile_photo_url'] = data.get('profile_photo_url')
    if 'profile_photo_path' in data:
        profile['profile_photo_path'] = data['profile_photo_path']
    
    profile['updated_at'] = datetime.utcnow().isoformat() + 'Z'
    
    profiles = load_profiles()
    for idx, p in enumerate(profiles):
        if p.get('user_id') == str(user_id):
            profiles[idx] = profile
            break
    
    if save_profiles(profiles):
        return jsonify({
            'success': True,
            'message': 'Profile photo updated successfully',
            'profile': profile
        })
    else:
        return jsonify({
            'success': False,
            'message': 'Error saving profile'
        }), 500

@app.route('/api/profile/<user_id>/wishlist', methods=['POST'])
def add_to_wishlist(user_id):
    """Add item to user's wishlist"""
    data = request.get_json()
    profile = find_profile(user_id=user_id)
    
    if not profile:
        return jsonify({
            'success': False,
            'message': 'Profile not found'
        }), 404
    
    item = data.get('item')
    if item:
        if 'wishlist' not in profile:
            profile['wishlist'] = []
        if item not in profile['wishlist']:
            profile['wishlist'].append(item)
        
        profile['updated_at'] = datetime.utcnow().isoformat() + 'Z'
        
        profiles = load_profiles()
        for idx, p in enumerate(profiles):
            if p.get('user_id') == str(user_id):
                profiles[idx] = profile
                break
        
        save_profiles(profiles)
    
    return jsonify({
        'success': True,
        'message': 'Item added to wishlist',
        'wishlist': profile.get('wishlist', [])
    })

@app.route('/api/profile/<user_id>/recently-viewed', methods=['POST'])
def add_to_recently_viewed(user_id):
    """Add item to recently viewed"""
    data = request.get_json()
    profile = find_profile(user_id=user_id)
    
    if not profile:
        return jsonify({
            'success': False,
            'message': 'Profile not found'
        }), 404
    
    item = data.get('item')
    if item:
        if 'recently_viewed' not in profile:
            profile['recently_viewed'] = []
        
        # Remove if already exists (to move to front)
        if item in profile['recently_viewed']:
            profile['recently_viewed'].remove(item)
        
        # Add to front
        profile['recently_viewed'].insert(0, item)
        
        # Keep only last 20 items
        profile['recently_viewed'] = profile['recently_viewed'][:20]
        
        profile['updated_at'] = datetime.utcnow().isoformat() + 'Z'
        
        profiles = load_profiles()
        for idx, p in enumerate(profiles):
            if p.get('user_id') == str(user_id):
                profiles[idx] = profile
                break
        
        save_profiles(profiles)
    
    return jsonify({
        'success': True,
        'message': 'Item added to recently viewed',
        'recently_viewed': profile.get('recently_viewed', [])
    })

@app.route('/api/profile/<user_id>/orders', methods=['POST'])
def add_order(user_id):
    """Add order to user profile"""
    data = request.get_json()
    profile = find_profile(user_id=user_id)
    
    if not profile:
        return jsonify({
            'success': False,
            'message': 'Profile not found'
        }), 404
    
    order = data.get('order')
    if order:
        if 'orders' not in profile:
            profile['orders'] = []
        profile['orders'].append(order)
        
        profile['updated_at'] = datetime.utcnow().isoformat() + 'Z'
        
        profiles = load_profiles()
        for idx, p in enumerate(profiles):
            if p.get('user_id') == str(user_id):
                profiles[idx] = profile
                break
        
        save_profiles(profiles)
    
    return jsonify({
        'success': True,
        'message': 'Order added successfully',
        'orders': profile.get('orders', [])
    })

@app.route('/api/profile/<user_id>', methods=['DELETE'])
def delete_profile(user_id):
    """Delete user profile"""
    profiles = load_profiles()
    profiles = [p for p in profiles if p.get('user_id') != str(user_id)]
    
    if save_profiles(profiles):
        return jsonify({
            'success': True,
            'message': 'Profile deleted successfully'
        })
    else:
        return jsonify({
            'success': False,
            'message': 'Error deleting profile'
        }), 500

@app.route('/health', methods=['GET'])
def health():
    """Health check"""
    return jsonify({'status': 'Profile API is running on port 5002'})

if __name__ == '__main__':
    print("🚀 Starting User Profile API on http://localhost:5002")
    print("📊 API Endpoints available:")
    print("   • GET  /api/profile/<user_id>")
    print("   • GET  /api/profile/email/<email>")
    print("   • GET  /api/profile/username/<username>")
    print("   • POST /api/profile")
    print("   • PUT  /api/profile/<user_id>")
    print("   • POST /api/profile/<user_id>/photo")
    print("   • POST /api/profile/<user_id>/wishlist")
    print("   • POST /api/profile/<user_id>/recently-viewed")
    print("   • POST /api/profile/<user_id>/orders")
    print("   • DELETE /api/profile/<user_id>")
    app.run(host='0.0.0.0', port=5002, debug=False, use_reloader=False)
