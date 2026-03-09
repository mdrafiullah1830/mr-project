# 🎯 User Profile Persistence System - Complete Guide

## আপনার চাওয়া সিস্টেম ✅

আপনার মতামত অনুযায়ী একটি সম্পূর্ণ **User Profile Data Persistence System** তৈরি করা হয়েছে।

### কী কী কাজ করে:

✅ **Sign In এর পর Data Save হয়**
- যখন কোনো user sign in করবে, তার সব তথ্য JSON file এ automatically save হবে
- Username, Email, Profile Photo, Bio, Address - সবকিছু রাখা যাবে

✅ **Profile Update করলে Data Update হয়**
- Profile page এ নাম, ফোন, ঠিকানা, ছবি change করলে automatically JSON file এ update হবে

✅ **পরবর্তী Sign In এ পূর্ববর্তী Data থাকবে**
- যেকোনো user বারবার sign in করলে তার আগের সব data same থাকবে
- Profile photo forget হবে না
- Bio আগের মতোই থাকবে

✅ **Multiple Users Support**
- 50+ user এর data একসাথে রাখা যাবে
- প্রতিটি user এর data separate থাকবে

---

## 🏗️ System Architecture

```
┌─────────────────────────────────────────────────┐
│         Frontend (HTML/JavaScript)              │
│  ✓ auth.html (Sign In/Sign Up)                 │
│  ✓ userprofile.html (Profile Management)       │
│  ✓ index.html (Home Page)                      │
└────────────┬────────────────────────────────────┘
             │ API Calls
             ↓
┌─────────────────────────────────────────────────┐
│    Profile Manager API (Port 5002)              │
│    Python Flask Server                          │
│  ✓ Create/Update Profiles                      │
│  ✓ Save to JSON File                           │
│  ✓ Load Profile Data                           │
└────────────┬────────────────────────────────────┘
             │ Read/Write
             ↓
┌─────────────────────────────────────────────────┐
│         data/user_profiles.json                 │
│    Persistent User Data Storage                 │
└─────────────────────────────────────────────────┘
```

---

## 📁 Files Created/Modified

### নতুন ফাইল:
1. **`backend/profile_api.py`** - Profile API Server (Python Flask)
2. **`assets/js/profile-manager.js`** - JavaScript Client Library
3. **`START_PROFILE_API.sh`** - Startup Script

### Modified ফাইল:
1. **`assets/html/auth.html`** - Sign in/up এর পর profile API কে call করে
2. **`assets/html/userprofile.html`** - Profile data load করে display করে
3. **`assets/js/auth.js`** - Sign in/up পর profile save করে

---

## 🚀 শুরু করার জন্য (Getting Started)

### Step 1: Profile API Server Start করুন

**Option A: সরাসরি চালু করুন**
```bash
cd "/Users/mdrafiullah/Desktop/mr project"
.venv/bin/python backend/profile_api.py
```

**Option B: Script ব্যবহার করুন**
```bash
bash START_PROFILE_API.sh
```

আপনি এই output দেখবেন:
```
🚀 Starting User Profile API on http://localhost:5002
📊 API Endpoints available:
   • GET  /api/profile/<user_id>
   • GET  /api/profile/email/<email>
   • GET  /api/profile/username/<username>
   • POST /api/profile
   • PUT  /api/profile/<user_id>
   • POST /api/profile/<user_id>/photo
   • POST /api/profile/<user_id>/wishlist
   • POST /api/profile/<user_id>/recently-viewed
   • POST /api/profile/<user_id>/orders
   • DELETE /api/profile/<user_id>
```

### Step 2: HTTP Server চালু করুন

```bash
python3 -m http.server 8000
```

### Step 3: Browser এ যান

```
http://localhost:8000/assets/html/index.html
```

---

## 🔄 কাজের প্রক্রিয়া (Workflow)

### যখন User Sign In করে:

```
1. User auth.html এ email & password দেয়
   ↓
2. JavaScript form handle করে
   ↓
3. LocalStorage এ user data save করে (offline backup)
   ↓
4. Profile API কে call করে: POST /api/profile
   ↓
5. API user data JSON file এ save করে
   ↓
6. userprofile.html এ redirect হয়
   ↓
7. Profile page আবার API থেকে data load করে
   ↓
8. User তার profile দেখে এবং edit করতে পারে
```

### যখন User Profile Edit করে:

```
1. User profile.html এ নাম/ফোন/বায়ো change করে
   ↓
2. "Save Changes" button click করে
   ↓
3. JavaScript API কে call করে: PUT /api/profile/<user_id>
   ↓
4. API JSON file এ update করে
   ↓
5. Success message দেখায়
```

### যখন User পরবার Sign In করে:

```
1. User sign in করে
   ↓
2. JavaScript user data JSON থেকে load করে
   ↓
3. Exact same profile দেখতে পায়
   ↓
4. কোনো data lose হয় না! ✅
```

---

## 📊 API Endpoints

### 1. Get Profile by ID
```bash
GET /api/profile/<user_id>

# Example:
curl http://localhost:5002/api/profile/user_123

# Response:
{
  "success": true,
  "profile": {
    "user_id": "user_123",
    "username": "rafi",
    "email_address": "rafi@example.com",
    "full_name": "Md. Rafi Ullah",
    "phone_number": "+880 1700-000000",
    "address": "Dhaka, Bangladesh",
    "profile_photo_url": "https://i.pravatar.cc/200?img=33",
    "bio": "Welcome to my profile",
    "created_at": "2025-01-28T10:30:00Z",
    "updated_at": "2025-01-28T12:45:00Z"
  }
}
```

### 2. Get Profile by Email
```bash
GET /api/profile/email/<email>

curl http://localhost:5002/api/profile/email/rafi@example.com
```

### 3. Get Profile by Username
```bash
GET /api/profile/username/<username>

curl http://localhost:5002/api/profile/username/rafi1830
```

### 4. Create/Update Profile
```bash
POST /api/profile

curl -X POST http://localhost:5002/api/profile \
  -H "Content-Type: application/json" \
  -d '{
    "user_id": "user_123",
    "username": "rafi",
    "email_address": "rafi@example.com",
    "full_name": "Md. Rafi Ullah",
    "phone_number": "+880 1700-000000",
    "address": "Dhaka, Bangladesh",
    "profile_photo_url": "https://i.pravatar.cc/200?img=33",
    "bio": "Welcome to my profile",
    "gender": "male",
    "date_of_birth": "1995-05-15"
  }'
```

### 5. Update Profile Fields
```bash
PUT /api/profile/<user_id>

curl -X PUT http://localhost:5002/api/profile/user_123 \
  -H "Content-Type: application/json" \
  -d '{
    "full_name": "Updated Name",
    "phone_number": "+880 1800-000000",
    "bio": "New bio text"
  }'
```

### 6. Update Profile Photo
```bash
POST /api/profile/<user_id>/photo

curl -X POST http://localhost:5002/api/profile/user_123/photo \
  -H "Content-Type: application/json" \
  -d '{
    "profile_photo_url": "https://i.pravatar.cc/200?img=45"
  }'
```

### 7. Add to Wishlist
```bash
POST /api/profile/<user_id>/wishlist

curl -X POST http://localhost:5002/api/profile/user_123/wishlist \
  -H "Content-Type: application/json" \
  -d '{
    "item": {
      "id": "product_456",
      "name": "Product Name",
      "price": 500,
      "image": "product.jpg"
    }
  }'
```

### 8. Add to Recently Viewed
```bash
POST /api/profile/<user_id>/recently-viewed

curl -X POST http://localhost:5002/api/profile/user_123/recently-viewed \
  -H "Content-Type: application/json" \
  -d '{
    "item": {
      "id": "product_123",
      "name": "Product Name",
      "price": 300
    }
  }'
```

### 9. Add Order
```bash
POST /api/profile/<user_id>/orders

curl -X POST http://localhost:5002/api/profile/user_123/orders \
  -H "Content-Type: application/json" \
  -d '{
    "order": {
      "order_id": "order_789",
      "items": ["product_1", "product_2"],
      "total": 1500,
      "status": "pending",
      "date": "2025-01-28"
    }
  }'
```

### 10. Delete Profile
```bash
DELETE /api/profile/<user_id>

curl -X DELETE http://localhost:5002/api/profile/user_123
```

---

## 💾 JSON File Structure

**File Location:** `data/user_profiles.json`

```json
[
  {
    "user_id": "user_123",
    "username": "rafi1830",
    "email_address": "rafi@example.com",
    "full_name": "Md. Rafi Ullah",
    "phone_number": "+880 1712-345678",
    "address": "House 12, Road 5, Dhanmondi, Dhaka",
    "date_of_birth": "1995-05-15",
    "gender": "male",
    "profile_photo_url": "https://i.pravatar.cc/200?img=33",
    "bio": "Welcome to my profile",
    "orders": [
      {
        "order_id": "order_001",
        "items": ["saree", "honey"],
        "total": 1500,
        "status": "delivered"
      }
    ],
    "wishlist": [
      {
        "id": "product_1",
        "name": "Product Name",
        "price": 500
      }
    ],
    "recently_viewed": [
      "product_1",
      "product_2",
      "product_3"
    ],
    "reviews": [],
    "created_at": "2025-01-28T10:30:00Z",
    "updated_at": "2025-01-28T12:45:00Z"
  }
]
```

---

## 🎨 JavaScript Usage in HTML Files

### auth.html এ Sign In এর পর:
```javascript
// profile-manager.js automatically এই function call করে:
await saveProfile({
  user_id: userData.id,
  username: username,
  email_address: userData.email,
  full_name: username,
  profile_photo_url: 'https://i.pravatar.cc/200?img=33',
  // ... etc
});
```

### userprofile.html এ Data Load করতে:
```javascript
// Profile data load করুন
const profile = await getProfile(userId);

// Display করুন
document.getElementById('fullName').value = profile.full_name;
document.getElementById('phoneNumber').value = profile.phone_number;
document.getElementById('profilePhoto').src = profile.profile_photo_url;
```

### Profile Update করতে:
```javascript
// Specific fields update করুন
await updateProfileFields(userId, {
  full_name: 'New Name',
  phone_number: '+880 1800-000000',
  bio: 'Updated bio'
});
```

---

## ✅ Features

### Implemented:
✅ User profile creation on sign in  
✅ Persistent JSON storage  
✅ Profile photo management  
✅ Bio/Address management  
✅ Multiple user support  
✅ API endpoints for all operations  
✅ Auto-load profile on sign in  
✅ Update profile fields  
✅ Wishlist management  
✅ Recently viewed tracking  
✅ Order tracking  

### Coming Soon:
🔜 Database migration (SQLite/PostgreSQL)  
🔜 Cloud backup  
🔜 Profile sharing  
🔜 Advanced analytics  

---

## 🐛 Troubleshooting

### Problem: API not starting
**Solution:**
```bash
# Check if Flask is installed
.venv/bin/python -c "import flask; print(flask.__version__)"

# If not, install:
pip3 install flask flask-cors
```

### Problem: Port 5002 already in use
**Solution:**
```bash
# Find process using port 5002
lsof -i :5002

# Kill the process
kill -9 <PID>

# Then restart
.venv/bin/python backend/profile_api.py
```

### Problem: Profile not saving
**Solution:**
1. Check if API is running: `curl http://localhost:5002/health`
2. Check browser console for errors (F12)
3. Check API logs in terminal

### Problem: "Cannot POST /api/profile"
**Solution:**
- Make sure profile-manager.js is loaded in HTML
- Check if API server is running on port 5002
- Check CORS is enabled (it is by default)

---

## 📝 Next Steps

1. **Test the system:**
   - Sign in with different usernames
   - Update profile information
   - Sign out and sign back in
   - Verify all data is preserved

2. **Customize profile fields:**
   - Edit `profile_api.py` to add more fields
   - Edit `userprofile.html` to show new fields

3. **Add database:**
   - Replace JSON with SQLite/PostgreSQL
   - Update API queries

4. **Deploy to production:**
   - Use production WSGI server (gunicorn/waitress)
   - Use environment variables for configuration
   - Add authentication/authorization

---

## 📞 Support

যদি কোনো সমস্যা হয়:
1. API logs check করুন
2. Browser console check করুন (F12 → Console)
3. Network tab check করুন (F12 → Network)
4. `data/user_profiles.json` file check করুন

**Done!** ✅ আপনার সিস্টেম সম্পূর্ণ প্রস্তুত!
