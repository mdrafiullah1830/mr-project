# Google OAuth Integration Guide - MR Shop

## Status: ✅ READY FOR SETUP

আপনার project এ Google Sign-In/Sign-Up এর জন্য সব কিছু ready আছে। এখন শুধু Google Client ID configure করতে হবে।

## Quick Start (Demo Mode)

এই মুহূর্তে **Demo Mode** এ আছে যেখানে:
- ✅ "Google Sign-In" button click করলে demo user login হবে
- ✅ সব functionality কাজ করবে
- ✅ Real Google API ছাড়াই test করতে পারবেন

## Production Setup (Real Google OAuth)

যখন real Google OAuth চাইবেন, এই steps follow করুন:

### 1️⃣ Google Cloud Console এ Project তৈরি করুন

```
URL: https://console.cloud.google.com
```

**Steps:**
- New Project create করুন (e.g., "MR Shop E-commerce")
- Project dashboard এ যান

### 2️⃣ OAuth 2.0 Credential তৈরি করুন

**Path:** APIs & Services → Credentials

**Steps:**
1. "Create Credentials" button click করুন
2. Select: "OAuth 2.0 Client ID"
3. Application Type: **Web Application**
4. Authorized JavaScript origins add করুন:
   ```
   http://localhost:8000
   http://localhost:3000
   http://localhost:5000
   http://127.0.0.1:8000
   ```
5. Authorized redirect URIs add করুন:
   ```
   http://localhost:8000/assets/html/auth.html
   http://localhost:8000/callback
   http://localhost:8000/assets/html/auth.html#google-callback
   ```
6. **Create** button press করুন

### 3️⃣ আপনার Client ID Copy করুন

Google Console থেকে **Client ID** copy করুন। Format হবে:
```
123456789-abcdef.apps.googleusercontent.com
```

### 4️⃣ Code এ Client ID Replace করুন

File: `/assets/js/auth.js`

Line 5 এ এটা find করুন:
```javascript
const CLIENT_ID = 'YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com';
```

Replace করুন আপনার actual Client ID দিয়ে:
```javascript
const CLIENT_ID = '123456789-abcdef.apps.googleusercontent.com';
```

### 5️⃣ Test করুন

```bash
# Terminal এ যদি HTTP server না চলছে:
cd "/Users/mdrafiullah/Desktop/mr project /assets/html"
python3 -m http.server 8000
```

তারপর browser এ:
```
http://localhost:8000/auth.html#register
```

"Continue with Google" button click করুন!

## কী কী হবে Google OAuth এ?

### Sign In / Sign Up এর সময়:
1. ✅ User "Continue with Google" click করবে
2. ✅ Google login page এ redirect হবে
3. ✅ User তার Google account দিয়ে authenticate করবে
4. ✅ Permission grant করবে
5. ✅ আপনার website এ return হবে
6. ✅ User profile automatically populate হবে (name, email, avatar)
7. ✅ Userprofile page এ redirect হবে

## Current Code Features

```javascript
✅ OAuth 2.0 Authorization Code Flow
✅ CSRF Protection (State parameter)
✅ Secure token handling
✅ User info retrieval
✅ LocalStorage auto-save
✅ Demo mode fallback
✅ Error handling
✅ Loading notifications
```

## File Changes Made

### Modified Files:
1. `/assets/html/auth.html` - Added Google SDK script
2. `/assets/js/auth.js` - Implemented full OAuth flow

### New Code Features:
- `initiateGoogleOAuth()` - Start OAuth flow
- `handleGoogleCallback()` - Handle redirect callback
- `fetchGoogleUserInfo()` - Get user details from Google
- `simulateGoogleLogin()` - Demo mode for testing
- `generateRandomState()` - CSRF protection

## Troubleshooting

### ❌ "Invalid redirect_uri"
**Solution:** Ensure you added exact URLs in Google Cloud Console:
- Include http (not https for localhost)
- Exact port numbers (8000, 3000, etc.)
- Exact paths (/assets/html/auth.html)

### ❌ "Client ID is invalid"
**Solution:** Make sure you:
- Copied correct Client ID from Google Console
- Put it in the right place in auth.js (line 5)
- Saved the file

### ❌ "Redirect mismatch"
**Solution:** The redirect URI in your code must match EXACTLY what's in Google Cloud Console

## Testing Checklist

- [ ] Created Google Cloud Project
- [ ] Created OAuth 2.0 credentials
- [ ] Copied Client ID
- [ ] Updated auth.js with Client ID
- [ ] Updated Google Cloud Console with redirect URIs
- [ ] Tested "Continue with Google" button
- [ ] Successfully logged in
- [ ] User data saved to localStorage
- [ ] Redirected to userprofile.html

## Next Steps (Optional)

আপনি আরও improve করতে পারেন:

1. **Facebook OAuth** - Similar process with Facebook SDK
2. **Apple Sign In** - Apple Developer account এ setup
3. **GitHub OAuth** - Developer applications এ
4. **Backend Integration** - Node.js/Express server
5. **Database** - MongoDB/PostgreSQL তে user data save করা

## Demo Testing

এখনই test করুন demo mode এ:

```
URL: http://localhost:8000/auth.html
1. "Register" tab এ যান
2. "Continue with Google" click করুন
3. Demo user automatic login হবে
4. userprofile.html এ redirect হবে
```

---

**Questions?** আমাকে জানাবেন কোথায় stuck আছেন!

**Owner:** mdrafiullah
**Date:** 27 January 2026
