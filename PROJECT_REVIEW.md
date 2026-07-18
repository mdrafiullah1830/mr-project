# MR Shop - Complete Project Review

**Project:** MR Shop (mrmart18)
**Live URLs:** https://mrshopbangladesh.tech | https://mrshopbd.com
**GitHub:** https://github.com/mdrafiullah1830/mr-project
**Review Date:** July 12, 2026
**Reviewer:** ChatGPT / AI Code Review

---

## 1. Project Overview

MR Shop is a full-stack e-commerce web application branded as "Bangladesh's #1 Online Marketplace." It enables users to browse, purchase, and review authentic Bangladeshi products across 6 categories: Food & Natural, Sweets & Dairy, Handicrafts, Clothing, Books, and Antiques.

The platform supports three user roles (customer, seller, admin), features an AI-powered chatbot, PWA capabilities, dark mode, bilingual support (English/Bengali), and multiple local payment methods (bKash, Nagad, COD, Card).

**Architecture:** Multi-page vanilla HTML/CSS/JS frontend + C# ASP.NET Core REST API backend + Python TF-IDF chatbot

---

## 2. Features

### Customer Features
- User registration & login (Email + Google OAuth)
- Product browsing with 6 categories
- Search with debounced suggestions
- Product comparison (side-by-side)
- Shopping cart with quantity management
- Wishlist management
- Multi-step checkout flow
- Payment: bKash, Nagad, Credit Card, Cash on Delivery
- Coupon system (MRSHOP100, MRSHOP50, FREESHIP)
- Order tracking with visual timeline
- User profile management
- Product reviews & ratings
- Dark mode toggle
- English/Bengali language toggle
- PWA (installable, offline support)
- AI chatbot for FAQs

### Seller Features
- Seller registration/onboarding
- Seller dashboard with stats
- Product management
- Order status updates
- Shop settings (banking, notifications, 2FA)

### Admin Features
- Admin dashboard
- Product management
- Order management
- User management (blocked from search engines)

---

## 3. Tech Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| **Frontend** | HTML5, CSS3, Vanilla JavaScript | ES2020+ |
| **Backend API** | C# / ASP.NET Core | .NET 10.0 |
| **Database** | MongoDB Atlas | Cloud-hosted |
| **Chatbot** | Python 3 | stdlib only |
| **Authentication** | JWT + Google OAuth 2.0 | - |
| **PWA** | Service Worker + Manifest | - |
| **Hosting** | Vercel (Frontend) | Auto-deploy |
| **Containerization** | Docker | Multi-stage build |
| **Analytics** | Microsoft Clarity | Session recording |
| **CSS Framework** | Custom (Amazon-inspired) | No Bootstrap/Tailwind |
| **JS Framework** | None (Vanilla JS) | No React/Vue/Angular |

---

## 4. Folder Architecture

```
mrmart18/
├── index.html                    # Homepage (47KB)
├── auth.html                     # Combined login/register
├── signin.html                   # Standalone login
├── signup.html                   # Standalone register
├── cart.html                     # Shopping cart
├── checkout.html                 # Multi-step checkout
├── payment.html                  # Payment methods
├── admin.html                    # Admin dashboard
├── seller.html                   # Seller dashboard
├── seller-settings.html          # Seller settings
├── becomeseller.html             # Seller onboarding
├── userprofile.html              # User profile
├── setting.html                  # User settings
├── wishlist.html                 # Saved products
├── search-results.html           # Search results
├── order-tracking.html           # Order tracking
├── compare.html                  # Product comparison
├── review.html                   # Product review
├── blog.html                     # Blog/articles
├── chat.html                     # AI chatbot
├── notification.html             # Notifications
├── email-notifications.html      # Email settings
├── food&natural.html             # Category: Food
├── sweets&dairy.html             # Category: Sweets
├── sweets&food.html              # Category: Sweets/Food
├── handricrafts.html             # Category: Handicrafts
├── clothing.html                 # Category: Clothing
├── book.html                     # Book product detail
├── book-categories.html          # Book categories
├── antique.html                  # Antique category
├── coin1.html, coin2.html, coin4.html  # Antique details
├── work-for-people.html          # Social impact
├── workfor.html, workforpeople.html, workforchild.html
│
├── assets/
│   ├── css/                      # 10 stylesheets (124KB)
│   │   ├── amazon-style.css      # Main design system (26KB)
│   │   ├── dark-mode.css         # Dark theme overrides
│   │   ├── auth.css              # Auth page styles
│   │   ├── book.css              # Book detail page
│   │   ├── book-categories.css   # Book categories
│   │   ├── clothing.css          # Clothing page
│   │   ├── seller-dashboard.css  # Seller dashboard
│   │   ├── userprofile.css       # User profile
│   │   ├── work-for-child.css    # Children donation
│   │   └── work-for-people.css   # People donation
│   │
│   ├── js/                       # 18 JS modules (120KB)
│   │   ├── api.js                # API client (JWT, CRUD)
│   │   ├── auth-shared.js        # Auth module (login/register)
│   │   ├── auth.js               # Auth page forms
│   │   ├── cart.js               # Cart management
│   │   ├── products.js           # Product data & rendering
│   │   ├── wishlist.js           # Wishlist management
│   │   ├── search.js             # Search with suggestions
│   │   ├── loader.js             # Font/icon loader
│   │   ├── darkmode.js           # Dark mode toggle
│   │   ├── i18n.js               # EN/BN translations
│   │   ├── book.js               # Book detail logic
│   │   ├── book-categories.js    # Book categories
│   │   ├── clothing.js           # Clothing page logic
│   │   ├── seller-dashboard.js   # Seller dashboard
│   │   ├── seller-settings.js    # Seller settings
│   │   ├── userprofile.js        # User profile
│   │   ├── work-for-people.js    # Donation page
│   │   └── work-for-child.js     # Children donation
│   │
│   └── images/                   # 49 image assets (~9MB)
│       ├── mrlogo.png            # Main logo (125KB)
│       ├── index.jpeg            # Hero image (168KB)
│       └── ... (product images, payment icons, social icons)
│
├── backend/                      # Python chatbot
│   ├── chat_server.py            # HTTP server (port 8000)
│   ├── chatwithus.py             # TF-IDF engine
│   └── local_site_agent.py       # Empty placeholder
│
├── MRShop.API/                   # C# ASP.NET Core backend
│   ├── Program.cs                # Entry point + DI config
│   ├── MRShop.API.csproj         # Project file
│   ├── Dockerfile                # Multi-stage Docker build
│   ├── appsettings.json          # Configuration
│   ├── appsettings.Development.json
│   ├── appsettings.Production.json
│   ├── .env.example              # Environment template
│   ├── Controllers/
│   │   ├── AuthController.cs     # JWT + Google OAuth
│   │   ├── CartController.cs     # Cart CRUD
│   │   ├── OrdersController.cs   # Order management
│   │   ├── ProductsController.cs # Product catalog
│   │   └── WishlistController.cs # Wishlist management
│   ├── DTOs/
│   │   ├── AuthDTOs.cs           # Auth request/response
│   │   ├── CartDTOs.cs           # Cart DTOs
│   │   ├── OrderDTOs.cs          # Order DTOs
│   │   ├── ProductDTOs.cs        # Product DTOs
│   │   └── WishlistDTOs.cs       # Wishlist DTOs
│   ├── Models/
│   │   ├── User.cs               # User entity
│   │   ├── Product.cs            # Product entity
│   │   ├── CartItem.cs           # Cart item entity
│   │   ├── Order.cs              # Order entity
│   │   └── WishlistItem.cs       # Wishlist item entity
│   └── Services/
│       ├── JwtService.cs         # JWT generation
│       └── MongoDbService.cs     # MongoDB connection
│
├── manifest.json                 # PWA manifest
├── service-worker.js             # Offline caching
├── robots.txt                    # SEO directives
├── sitemap.xml                   # SEO sitemap
├── README.md                     # Project documentation
└── .gitignore                    # Git ignore rules
```

---

## 5. Design System

### Color Palette
| Variable | Light Mode | Dark Mode |
|----------|-----------|-----------|
| Header BG | `#131921` | `#0d1117` |
| Primary | `#ff9900` | `#ff9900` |
| Body BG | `#eaeded` | `#161b22` |
| Card BG | `#ffffff` | `#21262d` |
| Text | `#0f1111` | `#e6edf3` |

### Typography
- **Primary Font:** Inter (Google Fonts)
- **Icon Font:** Font Awesome 6 (Free CDN)
- **Base Size:** 14px
- **Line Height:** 1.5

### CSS Architecture
- BEM-like naming with `amz-` prefix
- CSS Custom Properties for theming
- No preprocessor (raw CSS)
- 10 stylesheet files (124KB total)
- Inline styles on many HTML pages

### Responsive Breakpoints
| Breakpoint | Target |
|------------|--------|
| 1024px | Tablet landscape |
| 768px | Tablet portrait |
| 480px | Mobile |

---

## 6. User Flow

```
1. Homepage → Browse categories/products
2. Search → Search results with filters
3. Product Click → Product detail (or category page)
4. Add to Cart → Cart page
5. Checkout → Multi-step (address → payment → confirm)
6. Payment → bKash/Nagad/Card/COD
7. Order Confirmation → Order tracking
8. Profile → Order history, wishlist, settings
```

---

## 7. Seller Flow

```
1. becomeseller.html → Application form
2. Admin approval → localStorage stored
3. Login → seller.html (dashboard)
4. Dashboard → Stats, products, orders
5. seller-settings.html → Shop info, banking, notifications
```

**Note:** Seller data is stored in localStorage, not in MongoDB. This is a significant limitation.

---

## 8. Admin Flow

```
1. Login with admin@mrmart18.com / mrmart18.bd
2. Redirected to admin.html
3. Dashboard with product/order management
4. Order status updates
5. User management
```

**Note:** Admin credentials are hardcoded in client-side JavaScript (SECURITY RISK).

---

## 9. AI Features

### TF-IDF Chatbot (chat.html)
- **Algorithm:** Term Frequency-Inverse Document Frequency
- **Dependencies:** Zero (Python stdlib only)
- **Port:** 8000
- **Data Source:** Crawls all .html, .md, .txt files in project
- **Features:**
  - Cosine similarity search
  - Rate limiting (30 req/min)
  - Input sanitization
  - File upload for bug reports
  - CORS whitelist

**Limitation:** No semantic understanding, no embeddings, no external API calls.

---

## 10. Payment System

| Method | Implementation |
|--------|---------------|
| **bKash** | UI only (no real integration) |
| **Nagad** | UI only (no real integration) |
| **Credit Card** | UI only (no real integration) |
| **Cash on Delivery** | Functional (order created) |

**Note:** Payment methods are frontend-only. No real payment gateway integration exists. Orders are created in MongoDB but no actual payment processing occurs.

### Coupon System
| Code | Discount |
|------|----------|
| `MRSHOP100` | ৳100 off |
| `MRSHOP50` | 5% off |
| `FREESHIP` | Free shipping |

---

## 11. Authentication Flow

### Primary Flow (C# API)
```
1. User submits email/password
2. Frontend sends POST /api/customerauth/login
3. Backend verifies password (SHA-256 + static salt)
4. Backend returns JWT token (24hr expiry)
5. Frontend stores token in localStorage
6. Subsequent requests include Authorization: Bearer <token>
7. On 401 response → auto-logout
```

### Google OAuth Flow
```
1. User clicks "Continue with Google"
2. Google Identity SDK loads
3. User selects Google account
4. JWT credential returned to frontend
5. Frontend decodes JWT (works without backend)
6. User data saved to localStorage
7. Redirect to userprofile.html
```

### Fallback Flow (localStorage)
```
1. API offline → fallback to localStorage
2. Check admin credentials (hardcoded)
3. Check localStorage('mr_shop_users') array
4. Create user in localStorage if not exists
```

---

## 12. Security Features

### Implemented
| Feature | Location |
|---------|----------|
| JWT authentication | AuthController.cs |
| CORS policy | Program.cs |
| Rate limiting | chat_server.py (30 req/min) |
| Input sanitization | chat_server.py |
| Filename sanitization | chat_server.py |
| robots.txt blocking admin | robots.txt |
| HTTPS enforcement | Program.cs |

### CRITICAL Vulnerabilities
| Issue | Severity | Location |
|-------|----------|----------|
| Hardcoded admin credentials in client JS | CRITICAL | auth-shared.js:67-68 |
| Production MongoDB password in git | CRITICAL | appsettings.json |
| JWT secret in git | CRITICAL | appsettings.json |
| SHA-256 password hashing (no bcrypt) | HIGH | AuthController.cs |
| SSL validation disabled for MongoDB | HIGH | MongoDbService.cs |
| Swagger exposed in production | HIGH | Program.cs:131 |
| No rate limiting on auth endpoints | HIGH | AuthController.cs |
| Cart prices from client (no server validation) | HIGH | CartController.cs |
| No MongoDB indexes | MEDIUM | MongoDbService.cs |
| No input validation on regex search | MEDIUM | ProductsController.cs |

---

## 13. SEO Features

| Feature | Status | File |
|---------|--------|------|
| Meta title | ✅ | All HTML files |
| Meta description | ✅ | Most HTML files |
| Canonical URL | ❌ | Missing |
| Open Graph tags | ❌ | Missing |
| Twitter Cards | ❌ | Missing |
| Structured Data (JSON-LD) | ❌ | Missing |
| robots.txt | ✅ | robots.txt |
| sitemap.xml | ✅ | sitemap.xml (14 URLs) |
| Semantic HTML | Partial | Some pages |
| Image alt attributes | Partial | Many empty |
| H1-H6 hierarchy | Partial | Inconsistent |

---

## 14. Performance Optimizations

| Feature | Status |
|---------|--------|
| Service Worker caching | ✅ Network-first strategy |
| Font loading (non-blocking) | ✅ loader.js |
| Lazy loading images | ❌ Not implemented |
| Image compression | ❌ Images are large (925KB coin images) |
| CSS minification | ❌ No build tool |
| JS minification | ❌ No build tool |
| Bundle splitting | ❌ 18 separate JS files |
| Gzip/Brotli | ❌ Depends on hosting |
| CDN | ❌ Not configured |
| Preconnect hints | ❌ Not implemented |

---

## 15. Accessibility Features

| Feature | Status |
|---------|--------|
| ARIA labels on interactive elements | ❌ Missing |
| Skip-to-content link | ❌ Missing |
| Keyboard navigation | Partial |
| Screen reader support | ❌ Minimal |
| Focus indicators | Default browser |
| Color contrast | ✅ Good |
| Responsive design | ✅ 3 breakpoints |
| Form labels | ✅ Present |
| Alt text on images | ❌ Often empty strings |

---

## 16. Third-party APIs

| API | Purpose | Status |
|-----|---------|--------|
| Google Identity Services | OAuth login | ✅ Implemented |
| Microsoft Clarity | Session recording/heatmaps | ✅ Active |
| MongoDB Atlas | Database | ✅ Connected |
| Google Fonts (Inter) | Typography | ✅ Loading |
| Font Awesome 6 | Icons | ✅ Loading |

---

## 17. Known Limitations

### Architecture
1. **No build system** - 18 separate JS files loaded as `<script>` tags
2. **No CSS preprocessor** - Raw CSS with inline duplicates
3. **No framework** - Vanilla JS means no component reuse
4. **No TypeScript** - No type safety

### Data
5. **Product data in 3 places** - index.html, products.js, MongoDB
6. **Seller data in localStorage** - Not persisted to database
7. **Order history hardcoded** in userprofile.js
8. **No real payment integration**

### Security
9. **Admin credentials in client code**
10. **Production secrets in git history**
11. **Weak password hashing**
12. **No input validation on most endpoints**

### Performance
13. **No image optimization**
14. **No code minification/bundling**
15. **No lazy loading**
16. **No CDN configured**

### Testing
17. **Zero test files**
18. **No unit tests**
19. **No integration tests**
20. **No E2E tests**

---

## 18. Future Roadmap

### Priority 1 (Security)
- [ ] Rotate all exposed secrets
- [ ] Implement BCrypt password hashing
- [ ] Move admin credentials to database
- [ ] Add rate limiting on auth endpoints
- [ ] Server-side price validation

### Priority 2 (Architecture)
- [ ] Add Vite/Webpack build system
- [ ] Implement React/Vue framework
- [ ] Add TypeScript
- [ ] CSS preprocessing (SASS/Tailwind)
- [ ] Component-based architecture

### Priority 3 (Features)
- [ ] Real payment gateway (bKash/Nagad API)
- [ ] Email notification system
- [ ] SMS notifications
- [ ] Product image upload
- [ ] Seller product management API
- [ ] Admin panel API integration

### Priority 4 (Quality)
- [ ] Unit tests (Jest)
- [ ] Integration tests
- [ ] E2E tests (Cypress/Playwright)
- [ ] CI/CD pipeline
- [ ] Error tracking (Sentry)
- [ ] Performance monitoring

### Priority 5 (SEO/Marketing)
- [ ] Open Graph meta tags
- [ ] Twitter Card meta tags
- [ ] Structured data (JSON-LD)
- [ ] Google Analytics integration
- [ ] Sitemap auto-generation

---

## Appendix: File Statistics

| Category | Count | Size |
|----------|-------|------|
| HTML files | 37 | 670 KB |
| CSS files | 10 | 124 KB |
| JS files | 19 | 120 KB |
| C# files | 18 | 45 KB |
| Python files | 2 | 17 KB |
| Config files | 10 | 25 KB |
| Image files | 49 | 9.1 MB |
| **Total** | **149** | **~10 MB** |
| **Code only** | **86** | **~1 MB** |
