# Quick Testing Guide - Profile & Data Display

## System Status

✅ **Backend:** Running on `http://localhost:5010`
✅ **Login System:** Any username/password accepted
✅ **Admin Detection:** Username "mrshop" = Admin role
✅ **Profile Data:** Persists in localStorage + backend

---

## Test Scenario 1: Normal Login & Profile View

### Steps
1. Open `userprofile.html` (or navigate from login)
2. Login with any credentials:
   - Username: `testuser`
   - Password: `anything`
3. Check profile page loads with:
   - ✓ User name displays in sidebar
   - ✓ Email displays in sidebar
   - ✓ All form fields populate
   - ✓ Backend status shows "✅ Connected to Backend"

### Expected Results
- Profile data loads from backend API
- Form fields auto-filled with user data
- Photo displays (if available in backend)
- Green status indicator shows connected

---

## Test Scenario 2: Admin Login

### Steps
1. Login with username: `mrshop`
2. Password: any value
3. Check profile page for admin indicator

### Expected Results
- ✓ "👑 Welcome Admin!" message shown
- ✓ adminInfo stored in localStorage
- ✓ Role set to 'admin' in user object

---

## Test Scenario 3: Edit Profile (Online)

### Steps
1. Click "✏️ Edit" button on profile page
2. Modify any field (e.g., phone number, address)
3. Click "💾 Save Changes"
4. Wait for notification

### Expected Results
- ✓ Edit mode activates (fields become editable)
- ✓ "Profile saved to Backend" notification appears
- ✓ Data persists in both backend and localStorage
- ✓ Backend status remains "✅ Connected"

---

## Test Scenario 4: Profile Photo Display

### Steps
1. Check profile with existing photo (user_id: "2" has photo)
2. Navigate to that user's profile
3. Observe photo display

### Expected Results
- ✓ Profile photo loads from `/uploads/profiles/` path
- ✓ Photo displays in profile-photo-wrapper
- ✓ Photo upload button available ("📷")

---

## Test Scenario 5: Offline Editing

### Prerequisites
- Have profile data loaded (from previous online session)

### Steps
1. **Stop Backend:** Open terminal and press `Ctrl+C` on running `dotnet run`
2. **Refresh browser** on userprofile.html
3. **Check status:** Should show "⚠️ Working Offline"
4. **Click Edit** and modify profile
5. **Save Changes** 
6. **Restart Backend:** Run `dotnet run` in backend-csharp folder
7. **Refresh browser** - data should sync

### Expected Results
- ✓ Profile data still displays from localStorage
- ✓ Status shows "⚠️ Working Offline"
- ✓ Edit form works and changes save locally
- ✓ "Profile saved locally. (Offline Mode)" notification
- ✓ Form exits edit mode
- ✓ When backend restarts, data syncs

---

## Test Scenario 6: Data Persistence

### Steps
1. Login and edit profile
2. **Close browser completely**
3. **Clear only cache** (not localStorage)
4. **Open browser and navigate to userprofile.html**
5. Profile should still show saved data

### Expected Results
- ✓ localStorage data persists across browser sessions
- ✓ Profile fields remain populated
- ✓ User info available even on first page load

---

## Test Scenario 7: Backend Recovery

### Steps
1. Stop backend (Ctrl+C)
2. Edit profile - note "Offline Mode"
3. Start backend again: `cd backend-csharp && dotnet run`
4. Click "💾 Save Changes" again (or refresh page)

### Expected Results
- ✓ When backend comes back online, status changes to "✅ Connected"
- ✓ Offline edits can be synced to backend
- ✓ No data loss during offline period

---

## Test Users with Existing Data

Use these user IDs to test with pre-existing data:

```
User ID: test-user-123
Name: John Updated Doe
Email: john@example.com
Phone: +1234567890

User ID: 2
Name: rafi1830
Email: ullahmdrafi295@gmail.com
Phone: +880 1606079896
Photo: ✓ Has profile photo
```

---

## Debugging Tips

### Check localStorage
Open browser DevTools → Application → Local Storage → http://localhost:...

Look for:
- `mr_shop_user` - User login data
- `mr_shop_user_profile` - Profile data
- `adminInfo` - Admin status (if admin)

### Check Backend Logs
```bash
tail -f /tmp/backend.log
```

### Check Console Errors
DevTools → Console tab for any JavaScript errors

### API Test
```bash
# Test profile API
curl http://localhost:5010/api/profile/test-user-123

# Test profile exists
curl http://localhost:5010/api/profile/test-user-123/exists
```

---

## Status Indicators

| Status | Meaning | What to Do |
|--------|---------|-----------|
| 🟢 Connected to Backend | API working, full sync | Normal operation |
| 🟡 Connecting... | Fetching data | Wait for completion |
| 🟠 Working Offline | Using localStorage | Changes save locally |
| 🔴 Error | API failed | Check backend logs |

---

## Quick Command Reference

### Start Backend
```bash
cd "/Users/mdrafiullah/Desktop/mr project /backend-csharp"
dotnet run
```

### Stop Backend
Press `Ctrl+C` in the terminal where backend is running

### Access Frontend
```
http://localhost:5010 (Swagger UI for API)
file:///Users/mdrafiullah/Desktop/mr\ project/userprofile.html (Local file)
```

### View Profile Data File
```bash
cat "/Users/mdrafiullah/Desktop/mr project /data/user_profiles.json"
```

---

## Feature Summary

| Feature | Status | Notes |
|---------|--------|-------|
| Login with any credentials | ✅ | Accepts all username/password |
| Admin detection (mrshop) | ✅ | Case-insensitive |
| Profile data loading | ✅ | From backend or localStorage |
| Profile photo display | ✅ | From `/uploads/profiles/` |
| Profile editing | ✅ | Works online and offline |
| Data persistence | ✅ | Both localStorage and backend |
| Offline support | ✅ | Full functionality without backend |
| Backend sync | ✅ | Auto-syncs when online |

---

## Next Steps

1. Test all scenarios above
2. Report any issues with specific test cases
3. Let user data accumulate for more realistic testing
4. Consider adding image upload functionality (already coded)
