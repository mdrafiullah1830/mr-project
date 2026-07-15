# MR SHOP (mrmart18) - Complete Project Report

> Generated: July 15, 2026  
> Project: MR Shop - Bangladesh's #1 Online Marketplace  
> Live: https://mrshopbd.com  
> Currency: ৳ BDT (Taka)

---

## 1. PROJECT OVERVIEW

MR Shop is a full-stack Bangladeshi e-commerce platform selling authentic local products (food, sweets, handicrafts, clothing, books & antiques). It uses a 3-tier architecture:

| Layer | Technology | Port |
|-------|-----------|------|
| **Frontend** | Static HTML + Vanilla CSS + Vanilla JS | 8080 |
| **Backend (Chat)** | Python 3 (TF-IDF Q&A) | 8000 |
| **Backend (API)** | C# ASP.NET Core + MongoDB | 5000 |
| **Database** | MongoDB Atlas (Cloud) | 27017 |

---

## 2. FRONTEND REPORT

### 2.1 Tech Stack
- **HTML5** - 30+ pages (static)
- **CSS3** - 10 stylesheets (vanilla, no framework)
- **JavaScript** - 18 modules (vanilla, no framework)
- **Icons** - Font Awesome 6 (CDN)
- **Fonts** - Inter, Playfair Display, Nunito, Fredoka
- **PWA** - Service Worker + manifest.json

### 2.2 Pages (30+ HTML Files)

| Category | Pages | Description |
|----------|-------|-------------|
| **Home** | `index.html` | Amazon-style landing page |
| **Auth** | `signin.html`, `signup.html`, `auth.html` | Login/Register with split-panel design |
| **Shopping** | `cart.html`, `checkout.html`, `payment.html` | Full checkout flow |
| **Products** | `food&natural.html`, `sweets&dairy.html`, `handricrafts.html`, `clothing.html`, `book.html`, `antique.html` | 6 category pages |
| **Product Detail** | `coin1.html`, `coin2.html`, `coin4.html`, `book.html` | Individual product pages |
| **User** | `userprofile.html`, `setting.html`, `wishlist.html`, `notification.html`, `email-notifications.html` | User account management |
| **Seller** | `seller.html`, `seller-settings.html`, `becomeseller.html` | Seller onboarding & dashboard |
| **Admin** | `admin.html` | Admin dashboard |
| **Search** | `search-results.html`, `compare.html` | Search & product comparison |
| **Social** | `work-for-people.html`, `workfor.html`, `workforchild.html`, `workforpeople.html` | Donation/social impact pages |
| **Other** | `blog.html`, `faq.html`, `chat.html`, `review.html`, `order-tracking.html` | Support & tracking |
| **SEO** | `robots.txt`, `sitemap.xml` | Search engine directives |

### 2.3 CSS Architecture (10 Stylesheets)

| File | Lines | Purpose |
|------|-------|---------|
| `amazon-style.css` | ~1382 | Main design system (Amazon-style) |
| `dark-mode.css` | - | Dark theme overrides |
| `auth.css` | - | Auth page (split-panel, gradient) |
| `book.css` | - | Book detail (glassmorphism) |
| `book-categories.css` | - | Book category sidebar |
| `clothing.css` | - | Clothing product page |
| `seller-dashboard.css` | - | Seller dashboard |
| `userprofile.css` | - | User profile |
| `work-for-child.css` | - | Children donation page |
| `work-for-people.css` | - | People donation page |

**CSS Naming Conventions:**
| Prefix | Usage |
|--------|-------|
| `.amz-*` | Amazon-style pages |
| `.bk-*` | Book pages |
| `.rc-*` | Clothing pages |
| `.wfc-*` | Work-for-children pages |
| `.wfp-*` | Work-for-people pages |

**Design System Colors:**
| Variable | Value | Usage |
|----------|-------|-------|
| `--amazon-dark` | `#131921` | Header background |
| `--amazon-orange` | `#febd69` | Search button, accents |
| `--amazon-yellow` | `#ff9900` | Primary brand color |
| `--amazon-blue` | `#146eb4` | "Prime" badges, links |
| `--amazon-green` | `#007600` | In-stock, free delivery |
| `--accent` | `#7c3aed` | Purple accent |
| `--accent-2` | `#06b6d4` | Cyan accent |

### 2.4 JavaScript Modules (18 Files)

| Module | Lines | Purpose |
|--------|-------|---------|
| `api.js` | 74 | Centralized API client (`MR_API`) - GET/POST/PUT/DELETE with JWT |
| `auth.js` | - | Auth page toggle (login/register) |
| `auth-shared.js` | 379 | `MR_Auth` module - C# API auth + Google OAuth + localStorage fallback |
| `cart.js` | - | `MR_Cart` module - cart CRUD + server sync |
| `products.js` | 210 | `MR_PRODUCTS` array (8 fallback products) + API fetch |
| `wishlist.js` | - | `MR_Wishlist` module - wishlist CRUD |
| `search.js` | - | `MR_Search` module - debounced search + autocomplete |
| `loader.js` | - | Non-blocking font/icon loader |
| `darkmode.js` | - | Dark mode toggle (persisted to localStorage) |
| `i18n.js` | - | English/Bengali toggle (60+ strings, `data-i18n`) |
| `book-categories.js` | - | Book category sidebar |
| `book.js` | - | Book product page logic |
| `clothing.js` | - | Clothing product page (size/color selection) |
| `seller-dashboard.js` | - | Seller dashboard (orders, sales, sparkline charts) |
| `seller-settings.js` | - | Seller settings (shop info, banking, 2FA) |
| `userprofile.js` | - | User profile (photo upload, settings) |
| `work-for-child.js` | - | Children donation page (animated counters) |
| `work-for-people.js` | - | People donation page |

### 2.5 Frontend Features

- **E-Commerce:** Product catalog (6 categories), cart, coupon system (`MRSHOP100`, `MRSHOP50`, `FREESHIP`), checkout (bKash, Nagad, Credit Card, COD), wishlist, order tracking, product comparison, reviews with star ratings
- **Search:** Debounced search with category-grouped autocomplete
- **Auth:** JWT-based login/register (C# backend), Google OAuth, social login stubs (Facebook, Apple), session-based role detection (admin/seller)
- **Seller:** Onboarding, dashboard (orders, sales, revenue, sparkline charts), settings (banking, 2FA, delivery)
- **Admin:** Protected dashboard
- **PWA:** Service worker with network-first caching, offline order sync via IndexedDB, installable on mobile/desktop
- **i18n:** English/Bengali toggle with `data-i18n` attributes
- **Dark Mode:** Global dark theme toggle, persisted to localStorage
- **Analytics:** Microsoft Clarity (heatmaps, session recording)

### 2.6 localStorage Keys

| Key | Purpose |
|-----|---------|
| `mr_shop_cart` | Cart items array |
| `mr_shop_user` | Logged-in user object |
| `mr_shop_wishlist` | Wishlist items |
| `mrshop_darkmode` | Dark mode preference |
| `mrshop_lang` | Language preference (en/bn) |
| `mr_seller_settings_v1` | Seller settings |
| `mr_shop_user_profile` | User profile data |
| `mr_shop_profile_photo` | Base64 profile photo |
| `mr_reviews_<id>` | Per-product reviews |
| `mr_shop_notifications` | Notification messages |
| `mr_shop_users` | Registered users (fallback) |
| `mr_shop_token` | JWT auth token |
| `mr_shop_products` | Admin-added products |

---

## 3. BACKEND REPORT

### 3.1 Python Chat Server (Port 8000)

**Files:**
- `backend/chat_server.py` (253 lines) - HTTP server
- `backend/chatwithus.py` (216 lines) - TF-IDF Q&A engine

**Purpose:** Lightweight Q&A assistant that searches site content using TF-IDF + cosine similarity.

**API Endpoints:**

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/query` | Ask a question, returns `{results: [{path, score, snippet}]}` |
| POST | `/api/reindex` | Rebuild the content index |
| POST | `/api/report` | Upload bug report file (multipart/form-data) |
| GET | `/*` | Static file server (serves frontend) |

**Security Features:**
- Rate limiting: 30 requests/minute per IP
- Input sanitization (control chars removed, max 500 chars)
- Max query size: 1KB
- Max upload size: 10MB
- CORS whitelist (localhost:8000, localhost:3000)
- Filename sanitization on uploads

**How it works:**
1. Scans project for `.html`, `.htm`, `.md`, `.txt` files
2. Strips HTML tags/scripts/styles, extracts plain text
3. Breaks into paragraph chunks
4. Builds TF-IDF index (pure Python, no external deps)
5. On query: builds query TF-IDF vector, computes cosine similarity with chunks, returns top-K results

### 3.2 C# ASP.NET Core API (Port 5000)

**Project:** `MRShop.API/` (.NET 8)

**Files:**

| File | Lines | Purpose |
|------|-------|---------|
| `Program.cs` | 164 | App config, DI, middleware, Swagger |
| `Controllers/AuthController.cs` | 283 | Register, Login, Google OAuth, Profile, Password |
| `Controllers/ProductsController.cs` | 183 | CRUD products (admin/seller) |
| `Controllers/CartController.cs` | 126 | Cart CRUD (user) |
| `Controllers/WishlistController.cs` | 132 | Wishlist CRUD (user) |
| `Controllers/OrdersController.cs` | 155 | Orders CRUD + status update |
| `Services/MongoDbService.cs` | 41 | MongoDB connection + collection access |
| `Services/JwtService.cs` | - | JWT token generation |
| `Models/User.cs` | 38 | User model (ObjectId, name, email, passwordHash, phone, address, role, profilePhoto) |
| `Models/Product.cs` | 56 | Product model (ObjectId, name, description, price, category, stock, rating, etc.) |
| `Models/CartItem.cs` | - | Cart item model |
| `Models/WishlistItem.cs` | - | Wishlist item model |
| `Models/Order.cs` | - | Order model |
| `DTOs/AuthDTOs.cs` | - | Auth request/response DTOs |
| `DTOs/ProductDTOs.cs` | - | Product request/response DTOs |
| `DTOs/CartDTOs.cs` | - | Cart request/response DTOs |
| `DTOs/WishlistDTOs.cs` | - | Wishlist request/response DTOs |
| `DTOs/OrderDTOs.cs` | - | Order request/response DTOs |

**API Endpoints:**

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/customerauth/register` | No | Register new user |
| POST | `/api/customerauth/login` | No | Login with email/password |
| POST | `/api/customerauth/google` | No | Google OAuth login |
| GET | `/api/customerauth/me` | JWT | Get current user |
| PUT | `/api/customerauth/profile` | JWT | Update profile |
| PUT | `/api/customerauth/password` | JWT | Change password |
| GET | `/api/products` | No | List products (filter, search, sort, pagination) |
| GET | `/api/products/{id}` | No | Get single product |
| POST | `/api/products` | JWT (admin/seller) | Create product |
| PUT | `/api/products/{id}` | JWT (admin/seller) | Update product |
| DELETE | `/api/products/{id}` | JWT (admin/seller) | Delete product |
| GET | `/api/cart` | JWT | Get user's cart |
| POST | `/api/cart` | JWT | Add to cart |
| DELETE | `/api/cart/{id}` | JWT | Remove from cart |
| DELETE | `/api/cart` | JWT | Clear cart |
| GET | `/api/wishlist` | JWT | Get user's wishlist |
| POST | `/api/wishlist` | JWT | Add to wishlist |
| DELETE | `/api/wishlist/{id}` | JWT | Remove from wishlist |
| DELETE | `/api/wishlist/product/{productId}` | JWT | Remove by product ID |
| DELETE | `/api/wishlist` | JWT | Clear wishlist |
| GET | `/api/orders` | JWT | Get user's orders |
| GET | `/api/orders/{id}` | JWT | Get single order |
| POST | `/api/orders` | JWT | Create order from cart |
| PUT | `/api/orders/{id}/status` | JWT (admin/seller) | Update order status |
| GET | `/health` | No | Health check |
| GET | `/swagger` | No | Swagger UI |

**Middleware Pipeline:**
1. Static files (serves frontend from parent directory)
2. Swagger (available in all environments)
3. HTTPS redirection
4. CORS (AllowFrontend policy)
5. Authentication (JWT Bearer)
6. Authorization
7. Controllers

**CORS Allowed Origins:**
- `https://mrshopbangladesh.tech`
- `https://www.mrshopbangladesh.tech`
- `http://localhost:3000`
- `http://localhost:5000`
- `http://localhost:8080`
- `http://localhost:8000`

**Auth:**
- JWT Bearer authentication (24-hour expiry)
- Password hashing: SHA256 + salt (`MRShop_Salt_2024`)
- Google OAuth via `Google.Apis.Auth`
- Role-based: `customer`, `seller`, `admin`

---

## 4. DATABASE REPORT

### 4.1 Database: MongoDB Atlas (Cloud)

**Connection:**
- **Provider:** MongoDB Atlas (managed cloud)
- **Cluster:** `cluster0.f00ltdl.mongodb.net`
- **Database:** `mrshop`
- **Driver:** MongoDB.Driver (.NET)

**Collections (5):**

| Collection | Model | Purpose |
|------------|-------|---------|
| `users` | `User` | User accounts (customers, sellers, admins) |
| `products` | `Product` | Product catalog |
| `cartItems` | `CartItem` | Shopping cart items |
| `wishlistItems` | `WishlistItem` | User wishlists |
| `orders` | `Order` | Customer orders |

### 4.2 Schema Details

#### Users Collection
```
{
  _id: ObjectId,
  name: String,
  email: String (unique),
  passwordHash: String (SHA256 + salt),
  phone: String?,
  address: String?,
  role: String ("customer" | "seller" | "admin"),
  profilePhoto: String? (base64 or URL),
  createdAt: DateTime,
  updatedAt: DateTime
}
```

#### Products Collection
```
{
  _id: ObjectId,
  name: String,
  description: String,
  price: Decimal,
  originalPrice: Decimal?,
  category: String ("food" | "sweets" | "handicrafts" | "clothing" | "books" | "antique"),
  subcategory: String?,
  image: String (URL/path),
  images: [String],
  stock: Int,
  rating: Double,
  reviewCount: Int,
  sellerId: String?,
  isActive: Boolean,
  createdAt: DateTime,
  updatedAt: DateTime
}
```

#### CartItems Collection
```
{
  _id: ObjectId,
  userId: String (ref: users),
  productId: String,
  productName: String,
  price: Decimal,
  image: String,
  quantity: Int,
  createdAt: DateTime
}
```

#### WishlistItems Collection
```
{
  _id: ObjectId,
  userId: String (ref: users),
  productId: String,
  productName: String,
  price: Decimal,
  image: String,
  createdAt: DateTime
}
```

#### Orders Collection
```
{
  _id: ObjectId,
  userId: String (ref: users),
  items: [{
    productId: String,
    productName: String,
    price: Decimal,
    quantity: Int,
    image: String
  }],
  totalAmount: Decimal,
  shippingAddress: String,
  paymentMethod: String,
  status: String ("pending" | "confirmed" | "shipped" | "delivered" | "cancelled"),
  createdAt: DateTime,
  updatedAt: DateTime
}
```

### 4.3 Data Flow

```
Frontend (localStorage) ←→ C# API ←→ MongoDB Atlas
                     ↕
              Python Chat Server (scans HTML files, builds TF-IDF index)
```

**Offline Fallback Pattern:**
- Frontend tries C# API first
- If API unavailable → falls back to localStorage
- Products have hardcoded fallback array (8 products)
- Auth has hardcoded admin credentials (`admin@mrmart18.com` / `mrmart18.bd`)
- Cart/Wishlist sync from server on login

---

## 5. DEPLOYMENT

| Component | Platform | URL |
|-----------|----------|-----|
| Frontend | Vercel | https://mrshopbangladesh.tech |
| Backend (C#) | Expected at localhost:5000 | - |
| Backend (Python) | Expected at localhost:8000 | - |
| Database | MongoDB Atlas Cloud | - |
| Source | GitHub | https://github.com/mdrafiullah1830/mr-project |

---

## 6. SECURITY NOTES

1. **appsettings.json has hardcoded MongoDB credentials and JWT secret** - should be moved to environment variables or Azure Key Vault
2. **SHA256 password hashing is weak** - should use bcrypt or Argon2
3. **Admin credentials hardcoded in frontend JS** (`admin@mrmart18.com` / `mrmart18.bd`) - anyone can see these
4. **CORS is quite permissive** - allows all localhost ports
5. **No rate limiting on C# API** - only the Python chat server has rate limiting
6. **Google OAuth client ID exposed in frontend** - this is normal for OAuth but should be monitored

---

## 7. FILE COUNT SUMMARY

| Type | Count |
|------|-------|
| HTML pages | 30+ |
| CSS files | 10 |
| JS modules | 18 |
| C# controllers | 5 |
| C# models | 5 |
| C# DTOs | 5 |
| C# services | 2 |
| Python files | 3 |
| Images | 49+ |
| **Total project files** | ~130+ |

---

*Report generated for MR Shop (mrmart18) project.*
