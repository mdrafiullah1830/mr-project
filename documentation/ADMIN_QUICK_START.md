# 🔑 QUICK REFERENCE - ADMIN ACCOUNT

## Admin Login Credentials

| Field | Value |
|-------|-------|
| **Username** | `mrshop` |
| **Password** | `mrshop` |
| **Email** | admin@mrshop.com |
| **Role** | admin |
| **ID** | 5 |

---

## Quick Start (3 Steps)

### Step 1: Start Backend
```bash
cd ~/Desktop/"mr project "/backend-csharp
dotnet run
```

### Step 2: Open Website & Login
1. Go to `http://localhost:8000`
2. Click "Sign in"
3. Enter: username `mrshop`, password `mrshop`
4. Click "Sign In"

### Step 3: Click Admin Link
1. You'll be on userprofile.html
2. Look at navbar - see **⚙️ Admin** link
3. Click it to open admin dashboard

---

## Admin Dashboard Features

| Feature | Access |
|---------|--------|
| **Dashboard** | View statistics, summaries |
| **Categories** | Create, view, delete with images |
| **Products** | Create, view, delete with images |
| **Orders** | View all, filter by status |
| **Users** | View all, search by name/email |
| **Settings** | Configure site info |
| **Backup** | Export all data as JSON |
| **Clear Data** | Reset admin data (with warning) |

---

## API Endpoints (Backend)

```
All endpoints: http://localhost:5010/api/admin/

GET    /categories              - List all
POST   /categories              - Create (with image)
DELETE /categories/{id}         - Delete

GET    /products                - List all
POST   /products                - Create (with image)
DELETE /products/{id}           - Delete

GET    /orders                  - List all
GET    /orders?status=pending   - Filter

GET    /users                   - List all
GET    /users/search?q=term    - Search

GET    /settings                - Get config
POST   /settings                - Save config

GET    /backup                  - Export data
POST   /clear-data              - Clear data
```

---

## Testing Login via API

```bash
curl -s http://localhost:5010/api/auth/signin -X POST \
  -H "Content-Type: application/json" \
  -d '{"username_or_email":"mrshop","password":"mrshop"}' | jq .
```

**Expected response includes:**
```json
{
  "role": "admin",
  "username": "mrshop",
  "email": "admin@mrshop.com"
}
```

---

## Check localStorage After Login

```javascript
// In browser console:
JSON.parse(localStorage.getItem('mr_shop_user'))
```

**Should include:**
```javascript
{
  role: "admin",
  username: "mrshop",
  email: "admin@mrshop.com"
}
```

---

## Files Changed

| File | Change |
|------|--------|
| **Auth.cs** | Added role field to models |
| **AuthController.cs** | Return role in responses |
| **auth.js** | Store role in localStorage |
| **users.json** | Added admin user |
| **index.html** | Check role for admin link visibility |

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Admin link not showing | Clear localStorage, re-login |
| Can't login | Check username/password spelling |
| Backend not responding | Run `dotnet run` in backend-csharp |
| 404 on admin.html | Ensure file exists, check path |
| API errors | Check browser console for details |

---

## Security Info

✅ **Password:** Hashed with PBKDF2-SHA256 (10,000 iterations)  
✅ **Role:** Checked on both frontend and backend  
✅ **Session:** Stored in localStorage during login  
✅ **Visibility:** Admin link only shown to admin users  

---

## Regular User Comparison

| Aspect | Admin | Regular User |
|--------|-------|--------------|
| **See admin link?** | ✅ YES | ❌ NO |
| **Access admin panel?** | ✅ YES | ❌ NO |
| **Create categories?** | ✅ YES | ❌ NO |
| **Create products?** | ✅ YES | ❌ NO |
| **Browse products?** | ✅ YES | ✅ YES |
| **View profile?** | ✅ YES | ✅ YES |
| **Place orders?** | ✅ YES | ✅ YES |

---

## Key Locations

| Item | Path |
|------|------|
| **Admin HTML** | `/admin.html` |
| **Admin CSS** | `/assets/css/admin.css` |
| **Admin JS** | `/assets/js/admin.js` |
| **Backend** | `/backend-csharp` |
| **User Database** | `/data/users.json` |
| **Auth JS** | `/assets/js/auth.js` |

---

## Environment

| Component | Version/Status |
|-----------|---|
| **Backend** | ASP.NET Core 10.0 |
| **Port** | 5010 |
| **Status** | Running ✅ |
| **Frontend** | HTML5/CSS3/JS |
| **Browser** | Modern (Chrome, Firefox, Safari) |

---

**Last Updated:** 2025-12-08  
**Status:** Ready to Use ✅  
**Admin Account:** Active ✅  
