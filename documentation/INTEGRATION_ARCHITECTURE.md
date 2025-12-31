# 🔗 User Profile Backend Integration Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         USER PROFILE SYSTEM                              │
│                     ✅ Fully Integrated & Linked                         │
└─────────────────────────────────────────────────────────────────────────┘

┌───────────────────────┐
│   USER OPENS PAGE     │
│   userprofile.html    │
└───────────┬───────────┘
            │
            ▼
┌───────────────────────┐
│ Check Authentication  │
│ (localStorage check)  │
└───────────┬───────────┘
            │
            ▼
┌───────────────────────────────────────────────────────────┐
│              🔄 LOAD PROFILE DATA FLOW                     │
├───────────────────────────────────────────────────────────┤
│                                                            │
│  1. Get userId from localStorage                           │
│  2. Update status: "🔄 Connecting..."                      │
│  3. Check if profile exists in backend                     │
│     ↓                                                      │
│     GET /api/profile/{userId}/exists                       │
│     ↓                                                      │
│  4. If NOT exists → Auto-create profile                    │
│     ↓                                                      │
│     POST /api/profile                                      │
│     {                                                      │
│       user_id, full_name, email_address                   │
│     }                                                      │
│     ↓                                                      │
│     Status: "✨ Creating profile..."                       │
│     Notification: "Welcome! Your profile has been created" │
│                                                            │
│  5. Fetch complete profile                                 │
│     ↓                                                      │
│     GET /api/profile/{userId}                              │
│     ↓                                                      │
│  6. Auto-fill all form fields                              │
│  7. Update sidebar (name, email)                           │
│  8. Save to localStorage (backup)                          │
│  9. Status: "✅ Connected to Backend"                      │
│                                                            │
└───────────────────────────────────────────────────────────┘
            │
            ▼
┌─────────────────────────────────────────────────────────────┐
│                   USER INTERACTION                           │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────────┐      ┌──────────────────┐            │
│  │   EDIT PROFILE   │      │  UPLOAD PHOTO    │            │
│  └────────┬─────────┘      └────────┬─────────┘            │
│           │                         │                       │
│           ▼                         ▼                       │
│  ┌────────────────────┐    ┌──────────────────┐            │
│  │ 1. Click "Edit"    │    │ 1. Click camera  │            │
│  │ 2. Modify fields   │    │ 2. Select image  │            │
│  │ 3. Click "Save"    │    │ 3. Preview shown │            │
│  └────────┬───────────┘    └────────┬─────────┘            │
│           │                         │                       │
│           ▼                         ▼                       │
│  ┌────────────────────────┐ ┌─────────────────────────┐    │
│  │ PUT /api/profile/...   │ │ POST /api/profile/.../  │    │
│  │ {                      │ │ photo                   │    │
│  │   full_name,           │ │ (FormData with file)    │    │
│  │   phone_number,        │ │                         │    │
│  │   address,             │ │                         │    │
│  │   date_of_birth,       │ │                         │    │
│  │   gender               │ │                         │    │
│  │ }                      │ │                         │    │
│  └────────┬───────────────┘ └────────┬────────────────┘    │
│           │                          │                      │
│           ▼                          ▼                      │
│  ┌────────────────────────────────────────────────┐        │
│  │          BACKEND PROCESSING                     │        │
│  └────────────────────────────────────────────────┘        │
│                                                              │
└──────────────────────────────────────────────────────────────┘
                        │
                        ▼
┌────────────────────────────────────────────────────────────────┐
│                    C# BACKEND API                               │
│                   localhost:5010                                │
├────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────────────────────────────────────┐          │
│  │           ProfileController.cs                    │          │
│  ├──────────────────────────────────────────────────┤          │
│  │                                                   │          │
│  │  • GET    /api/profile/{userId}/exists           │          │
│  │  • POST   /api/profile                           │          │
│  │  • GET    /api/profile/{userId}                  │          │
│  │  • PUT    /api/profile/{userId}                  │          │
│  │  • POST   /api/profile/{userId}/photo            │          │
│  │                                                   │          │
│  └───────────────────┬──────────────────────────────┘          │
│                      │                                          │
│                      ▼                                          │
│  ┌──────────────────────────────────────────────────┐          │
│  │           ProfileService.cs                       │          │
│  ├──────────────────────────────────────────────────┤          │
│  │                                                   │          │
│  │  • Thread-safe file operations                   │          │
│  │  • SemaphoreSlim for locking                     │          │
│  │  • JSON serialization/deserialization            │          │
│  │  • Partial update support                        │          │
│  │  • Image path management                         │          │
│  │                                                   │          │
│  └───────────────────┬──────────────────────────────┘          │
│                      │                                          │
│                      ▼                                          │
│  ┌──────────────────────────────────────────────────┐          │
│  │              DATA STORAGE                         │          │
│  ├──────────────────────────────────────────────────┤          │
│  │                                                   │          │
│  │  📄 user_profiles.json                           │          │
│  │     ├─ user_id                                   │          │
│  │     ├─ full_name                                 │          │
│  │     ├─ email_address                             │          │
│  │     ├─ phone_number                              │          │
│  │     ├─ address                                   │          │
│  │     ├─ date_of_birth                             │          │
│  │     ├─ gender                                    │          │
│  │     ├─ profile_photo_path                        │          │
│  │     ├─ bio                                       │          │
│  │     ├─ created_at                                │          │
│  │     └─ updated_at                                │          │
│  │                                                   │          │
│  │  📁 wwwroot/uploads/profiles/                    │          │
│  │     └─ {userId}_{timestamp}.{ext}               │          │
│  │                                                   │          │
│  └───────────────────────────────────────────────────┘          │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                        │
                        ▼
┌────────────────────────────────────────────────────────────────┐
│                    RESPONSE FLOW                                │
├────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ✅ Success Response:                                           │
│     {                                                           │
│       "success": true,                                          │
│       "message": "Profile updated successfully",                │
│       "data": { ... profile data ... }                          │
│     }                                                           │
│                                                                 │
│  ❌ Error Response:                                             │
│     {                                                           │
│       "success": false,                                         │
│       "message": "Error details",                               │
│       "data": null                                              │
│     }                                                           │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                        │
                        ▼
┌────────────────────────────────────────────────────────────────┐
│                  FRONTEND UPDATES                               │
├────────────────────────────────────────────────────────────────┤
│                                                                 │
│  • Update form fields                                           │
│  • Update sidebar (name, email, photo)                          │
│  • Save to localStorage (backup)                                │
│  • Show success notification                                    │
│  • Update status indicator                                      │
│  • Exit edit mode (if editing)                                  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘


═══════════════════════════════════════════════════════════════════
                      ERROR HANDLING
═══════════════════════════════════════════════════════════════════

┌─────────────────────────────────────────────────────────────────┐
│  IF BACKEND IS OFFLINE:                                          │
│                                                                  │
│  1. Status: "❌ Backend Offline"                                 │
│  2. Load data from localStorage                                  │
│  3. User can still view/edit profile                             │
│  4. Changes saved to localStorage only                           │
│  5. Will sync when backend comes back online                     │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│  IF API CALL FAILS:                                               │
│                                                                   │
│  1. Catch error in try-catch block                                │
│  2. Log error to console                                          │
│  3. Show user-friendly error notification                         │
│  4. Fall back to localStorage                                     │
│  5. Update status indicator accordingly                           │
│                                                                   │
└───────────────────────────────────────────────────────────────────┘


═══════════════════════════════════════════════════════════════════
                      STATUS INDICATORS
═══════════════════════════════════════════════════════════════════

🔄 Connecting...          ➜  Fetching data from backend
✨ Creating profile...    ➜  First-time profile creation
✅ Connected to Backend   ➜  Successfully synced
⚠️ Offline Mode          ➜  Using cached localStorage data
❌ Backend Offline        ➜  API not responding


═══════════════════════════════════════════════════════════════════
                      DATA PERSISTENCE
═══════════════════════════════════════════════════════════════════

PRIMARY:    user_profiles.json (Backend)
BACKUP:     localStorage (Frontend)
IMAGES:     wwwroot/uploads/profiles/ (Backend)

SYNC STRATEGY:
1. Always save to backend first
2. Then update localStorage
3. If backend fails, save to localStorage only
4. Load from backend first, fallback to localStorage


═══════════════════════════════════════════════════════════════════
                          FEATURES
═══════════════════════════════════════════════════════════════════

✅ Auto Profile Creation       ✅ Real-Time Sync
✅ Profile Photo Upload         ✅ Partial Updates
✅ Thread-Safe Operations       ✅ Offline Support
✅ Error Handling              ✅ Status Indicators
✅ User Notifications          ✅ Data Persistence
✅ localStorage Backup         ✅ Image Validation


═══════════════════════════════════════════════════════════════════
                    INTEGRATION STATUS
═══════════════════════════════════════════════════════════════════

🟢 BACKEND:         Running on localhost:5010
🟢 FRONTEND:        Fully integrated with backend
🟢 DATA STORAGE:    JSON file operational
🟢 IMAGE UPLOAD:    Directory created & functional
🟢 API ENDPOINTS:   All 5 endpoints tested & working
🟢 ERROR HANDLING:  Comprehensive fallback system
🟢 USER EXPERIENCE: Status indicators & notifications

STATUS: ✅ PRODUCTION READY
```
