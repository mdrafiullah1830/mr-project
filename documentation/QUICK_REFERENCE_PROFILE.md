# 🎯 Quick Reference - User Profile Backend Integration

## ⚡ Quick Start

### Start the Backend:
```bash
cd "/Users/mdrafiullah/Desktop/mr project /backend-csharp"
dotnet run
```

### Test the Integration:
1. Open `index.html` in browser
2. Sign up or log in
3. Click profile icon
4. See "✅ Connected to Backend" status
5. Edit profile and save
6. Logout and login - data persists!

---

## 🔌 API Endpoints

| Method | Endpoint | Purpose |
|--------|----------|---------|
| `GET` | `/api/profile/{userId}/exists` | Check if profile exists |
| `POST` | `/api/profile` | Create new profile |
| `GET` | `/api/profile/{userId}` | Get profile data |
| `PUT` | `/api/profile/{userId}` | Update profile |
| `POST` | `/api/profile/{userId}/photo` | Upload photo |

---

## 📁 Key Files

### Frontend:
- `userprofile.html` - Profile page UI
- `assets/js/userprofile.js` - Integration logic

### Backend:
- `Controllers/ProfileController.cs` - API endpoints
- `Services/ProfileService.cs` - Business logic
- `Models/Profile.cs` - Data models

### Data:
- `data/user_profiles.json` - Profile database
- `wwwroot/uploads/profiles/` - Profile photos

---

## 🎨 Status Indicators

| Indicator | Meaning |
|-----------|---------|
| 🔄 Connecting... | Fetching from backend |
| ✨ Creating profile... | First-time setup |
| ✅ Connected to Backend | Fully synced |
| ⚠️ Offline Mode | Using cache |
| ❌ Backend Offline | API down |

---

## 🧪 Quick Tests

### Test Profile Creation:
```bash
curl -X POST http://localhost:5010/api/profile \
  -H "Content-Type: application/json" \
  -d '{
    "user_id": "testuser123",
    "full_name": "Test User",
    "email_address": "test@example.com"
  }'
```

### Test Profile Retrieval:
```bash
curl http://localhost:5010/api/profile/testuser123
```

### View All Profiles:
```bash
cat "/Users/mdrafiullah/Desktop/mr project /data/user_profiles.json" | jq .
```

---

## 🔧 Troubleshooting

### Backend Not Starting?
```bash
# Kill existing process
pkill -f "MRShop"

# Start fresh
cd "/Users/mdrafiullah/Desktop/mr project /backend-csharp"
dotnet run
```

### Profile Not Loading?
1. Check browser console (F12)
2. Verify API is running: `curl http://localhost:5010/api/profile/test/exists`
3. Clear localStorage and try again

### Photos Not Uploading?
1. Check directory exists: `ls backend-csharp/wwwroot/uploads/profiles/`
2. Check file size (max 5MB)
3. Check file type (jpg, png, gif, webp only)

---

## 📊 Data Flow

```
User Opens Page
    ↓
Check Authentication
    ↓
Load Profile (Backend)
    ↓
Auto-fill Form
    ↓
User Edits
    ↓
Save to Backend
    ↓
Update UI
    ↓
Success!
```

---

## ✨ Features at a Glance

| Feature | Status |
|---------|--------|
| Auto Profile Creation | ✅ |
| Real-Time Sync | ✅ |
| Photo Upload | ✅ |
| Offline Support | ✅ |
| Error Handling | ✅ |
| Status Indicators | ✅ |
| Notifications | ✅ |
| Data Persistence | ✅ |

---

## 🚀 Production Checklist

- [ ] Update API URL in JavaScript (from localhost to production)
- [ ] Configure CORS for production domain
- [ ] Set up SSL/HTTPS for backend
- [ ] Configure file upload limits
- [ ] Set up backup for user_profiles.json
- [ ] Configure image optimization
- [ ] Add rate limiting
- [ ] Set up monitoring

---

## 📞 Quick Commands

```bash
# Start backend
cd backend-csharp && dotnet run

# Build backend
cd backend-csharp && dotnet build

# View profiles
cat data/user_profiles.json | jq .

# View uploaded photos
ls -la backend-csharp/wwwroot/uploads/profiles/

# Test API
curl http://localhost:5010/api/profile/test/exists

# Kill backend
pkill -f "MRShop"
```

---

## 🎉 Success Criteria

✅ User can sign up and profile is auto-created
✅ User can edit and save profile
✅ Profile data persists after logout/login
✅ User can upload profile photo
✅ Status indicator shows connection state
✅ Offline mode works with localStorage
✅ Error notifications appear when needed
✅ All data saved to JSON file

---

**Status:** ✅ **FULLY LINKED AND OPERATIONAL**

**Last Updated:** December 8, 2025
