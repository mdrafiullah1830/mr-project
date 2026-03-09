# 🚀 Quick Start Guide - User Profile Persistence

## ⚡ 3 Steps to Run

### Step 1: Start Profile API (Terminal 1)
```bash
cd "/Users/mdrafiullah/Desktop/mr project"
.venv/bin/python backend/profile_api.py
```

Expected output:
```
🚀 Starting User Profile API on http://localhost:5002
📊 API Endpoints available:
 * Running on http://127.0.0.1:5002
```

### Step 2: Start HTTP Server (Terminal 2)
```bash
cd "/Users/mdrafiullah/Desktop/mr project"
python3 -m http.server 8000
```

### Step 3: Open in Browser
```
http://localhost:8000/assets/html/index.html
```

---

## 🎯 What to Test

### Test 1: Sign Up
1. Click "Sign up" button
2. Enter: username="rafi", email="rafi@example.com", password="123456"
3. Click "Sign up" button
4. ✅ Should redirect to userprofile.html
5. ✅ Data should be saved in `data/user_profiles.json`

### Test 2: Check Profile Data
```bash
# Terminal 3
cat data/user_profiles.json | grep "rafi"
```
✅ You should see your profile data

### Test 3: Edit Profile
1. On userprofile.html, click "Edit" button
2. Change full name to "Rafi Ullah Updated"
3. Change phone to "+880 1700-000000"
4. Click "Save Changes"
5. ✅ Should see success message
6. ✅ Data saved in JSON

### Test 4: Sign Out & Sign Back In
1. Click "Logout" button
2. Sign in again with same email
3. ✅ All previous profile data should be there!
4. ✅ Profile photo, bio, everything preserved!

### Test 5: Multiple Users
1. Sign out
2. Sign up with different email: "ali@example.com"
3. Update profile info
4. Sign out
5. Sign up with another email: "fatima@example.com"
6. ✅ All 3 users data should be in JSON file separately

---

## 📊 API Endpoints Quick Reference

### Get User Profile
```bash
curl http://localhost:5002/api/profile/user_123
```

### Create/Update Profile
```bash
curl -X POST http://localhost:5002/api/profile \
  -H "Content-Type: application/json" \
  -d '{"user_id":"u1","username":"rafi","email_address":"rafi@test.com"}'
```

### Update Specific Fields
```bash
curl -X PUT http://localhost:5002/api/profile/user_123 \
  -H "Content-Type: application/json" \
  -d '{"full_name":"New Name","bio":"New Bio"}'
```

### Check if API is Running
```bash
curl http://localhost:5002/health
```

---

## 🔍 Debugging

### Check API is Running
```bash
lsof -i :5002
```

### Check Port 8000 is Running
```bash
lsof -i :8000
```

### View User Profiles JSON
```bash
cat data/user_profiles.json
```

### Pretty Print JSON (Mac/Linux)
```bash
cat data/user_profiles.json | python3 -m json.tool
```

### Check API Errors
Look at Terminal 1 (where Profile API is running) for error messages

### Check Browser Errors
1. Open http://localhost:8000/assets/html/auth.html
2. Press F12 (Developer Tools)
3. Click "Console" tab
4. Look for red error messages

---

## 📁 Important Files

| File | Purpose | Port |
|------|---------|------|
| `backend/profile_api.py` | Profile API Server | 5002 |
| `assets/js/profile-manager.js` | JavaScript Client Library | - |
| `assets/html/auth.html` | Sign In/Sign Up Page | 8000 |
| `assets/html/userprofile.html` | User Profile Page | 8000 |
| `data/user_profiles.json` | User Data Storage | - |

---

## ✅ Checklist

- [ ] Profile API running on port 5002
- [ ] HTTP Server running on port 8000
- [ ] Can sign up with email/username/password
- [ ] Profile data saved to JSON
- [ ] Can edit profile information
- [ ] Changes saved to JSON
- [ ] Can sign out and sign back in
- [ ] All data preserved after re-login
- [ ] Multiple users can coexist
- [ ] Each user has separate data

---

## 🎉 You're All Set!

System is ready for:
✅ User sign in/sign up  
✅ Profile data persistence  
✅ Data preservation across sessions  
✅ Multiple user support  
✅ Profile editing and updating  

Enjoy! 🚀
