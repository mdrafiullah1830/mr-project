# 🎯 HOW TO ACCESS THE ADMIN DASHBOARD

## Step 1: Make Sure Backend is Running ✅

```bash
cd ~/Desktop/"mr project "/backend-csharp
dotnet run
# Should show: Running on http://localhost:5010
```

## Step 2: Open Your Website

Go to: `http://localhost:8000`

You should see the normal homepage.

---

## Step 3: Login as Admin

1. Click **"Sign in"** button (top right of navbar)
2. Go to the login page
3. **Username:** `mrshop`
4. **Password:** `mrshop`
5. Click **"Sign In"**

---

## Step 4: See the Admin Link

After login, you'll be redirected to userprofile.html.

Look at the navbar (top right) - you should now see:
- **👤 Profile** (current page)
- **⚙️ Admin** (NEW - this is the admin link!)
- **Log out**

---

## Step 5: Click the Admin Link

Click the **⚙️ Admin** link in the navbar.

You'll be taken to the admin dashboard at `/admin.html`

---

## What You'll See in the Admin Dashboard

### Left Sidebar (Navigation)
- 📊 Dashboard
- 🏷️ Categories
- 📦 Products
- 📋 Orders
- 👥 Users
- ⚙️ Settings

### Main Content Area
- Dashboard shows 4 statistics cards (categories, products, orders, users count)
- Each section has forms and tables to manage data

---

## Admin Features Available

### 📊 Dashboard
- View total categories, products, orders, users

### 🏷️ Categories
- ➕ Add new categories
- 📸 Upload category images
- 🗑️ Delete categories
- 📋 View all categories

### 📦 Products
- ➕ Add new products
- 📸 Upload product images
- 💵 Set prices, discounts, stock
- 🗑️ Delete products
- 📋 View all products

### 📋 Orders
- View all orders
- Filter by status (pending, completed, cancelled)

### 👥 Users
- View all registered users
- Search users by name or email

### ⚙️ Settings
- Configure site name
- Set site description
- Set contact email
- 💾 Backup all data
- 🗑️ Clear admin data

---

## Troubleshooting

### Issue: Don't see the ⚙️ Admin link after login

**Solution:**
1. Make sure you're logged in (check if "Profile" link is showing)
2. If not logged in, go to Sign in
3. After successful login, refresh the page (F5)
4. The admin link should now appear

### Issue: Admin link doesn't work

**Solution:**
1. Make sure backend is running: `dotnet run`
2. Check that `/admin.html` file exists
3. Check browser console (F12) for error messages
4. Refresh the page

### Issue: Admin dashboard is blank

**Solution:**
1. Make sure backend is running on port 5010
2. Check browser console (F12 → Console) for JavaScript errors
3. Make sure admin.js file is loaded (check Network tab in DevTools)
4. Try refreshing the page

### Issue: Can't login with mrshop/mrshop

**Solution:**
1. Double-check the username and password spelling
2. Make sure backend is running
3. Check if you're using the correct email instead of username
4. Try creating a new test account and login with that

---

## Quick Commands

### Start backend:
```bash
cd ~/Desktop/"mr project "/backend-csharp && dotnet run
```

### Test admin login (curl):
```bash
curl -s http://localhost:5010/api/auth/signin -X POST \
  -H "Content-Type: application/json" \
  -d '{"username_or_email":"mrshop","password":"mrshop"}' | jq .
```

### Check if backend is running:
```bash
lsof -i :5010
```

---

## File Locations

| Item | Path |
|------|------|
| Admin HTML | `/admin.html` |
| Admin CSS | `/assets/css/admin.css` |
| Admin JS | `/assets/js/admin.js` |
| Homepage | `/index.html` |
| Backend | `/backend-csharp` |

---

## Still Not Working?

Please check:

1. **Backend running?** Type `lsof -i :5010` - should see dotnet process
2. **Logged in?** Check navbar - do you see "Profile" link?
3. **Right credentials?** Username: `mrshop`, Password: `mrshop`
4. **Files exist?** Check `/admin.html`, `/assets/css/admin.css`, `/assets/js/admin.js`
5. **Browser console errors?** Press F12 and check Console tab

If you're still stuck, let me know what you see! 🙂
