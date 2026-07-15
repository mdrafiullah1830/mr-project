# MR Shop - Bangladesh's #1 Online Marketplace

> A complete Bangladeshi e-commerce platform selling authentic local products — food, sweets, handicrafts, clothing, books & antiques. Built with vanilla HTML/CSS/JS with no frameworks.

**Live:** https://mrshopbd.com  
**Currency:** ৳ BDT (Taka)  
**Languages:** English + Bengali (বাংলা)

---

## Features

### E-Commerce
- Product catalog with 6 categories (Food, Sweets, Handicrafts, Clothing, Books, Antiques)
- Product detail pages (books: format selection, clothing: size/color, coins: antique info)
- Shopping cart with coupon system (`MRSHOP100`, `MRSHOP50`, `FREESHIP`)
- Checkout flow (bKash, Nagad, Credit Card, Cash on Delivery)
- Wishlist with server sync
- Debounced search with category-grouped autocomplete
- Order tracking with visual timeline
- Product comparison page
- Product reviews with star ratings

### Authentication
- JWT-based login/register (C# backend at `localhost:5000/api`)
- Social login stubs (Google, Facebook, Apple)
- Session-based role detection (admin → admin.html, seller → seller.html)
- Profile photo upload (base64 to localStorage)

### Seller Features
- Seller onboarding page
- Seller dashboard (orders, sales, revenue, sparkline charts, product management)
- Seller settings (shop info, banking, notifications, 2FA, delivery preferences)

### Admin Features
- Admin dashboard (protected from search engines via robots.txt)

### Chat & AI
- "Chat with Us" page with Python TF-IDF backend
- HTTP server on port 8000 (`chat_server.py`)
- Queries site content using cosine similarity search
- Bug report file upload endpoint

### Social Impact
- 10% of profits donated to help underprivileged children
- "Work for People" donation page with impact stats
- "Support Children" donation page with animated counters & story slider

### PWA & Offline
- Service worker with network-first caching
- Offline order sync via IndexedDB background sync
- PWA manifest (installable on mobile/desktop)

### Internationalization
- English/Bengali toggle (60+ translated strings)
- `data-i18n` attribute-based translation system

### Dark Mode
- Global dark theme toggle
- Per-page dark mode support
- Persisted to localStorage

### Analytics
- Microsoft Clarity (heatmaps, session recording)
- Project key: `xkbf74mr8r`

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Static HTML + Vanilla CSS + Vanilla JavaScript |
| Icons | Font Awesome 6 (CDN) |
| Fonts | Inter, Playfair Display, Nunito, Fredoka |
| Images | Unsplash/Picsum (remote) + 49 local assets |
| Backend (chat) | Python 3 (TF-IDF Q&A) |
| Backend (API) | C# (not in repo, expected at localhost:5000) |
| PWA | Service Worker + manifest.json |
| Analytics | Microsoft Clarity |

---

## Project Structure

```
mrmart18/
├── index.html                 # Homepage (Amazon-style landing)
├── signin.html                # Login page
├── signup.html                # Registration page
├── auth.html                  # Auth page (split-panel animated form)
├── cart.html                  # Shopping cart
├── checkout.html              # Checkout page
├── payment.html               # Payment methods (bKash, Nagad, COD, card)
├── admin.html                 # Admin dashboard
├── seller.html                # Seller onboarding
├── seller-settings.html       # Seller settings
├── chat.html                  # Chat with us (AI assistant)
├── userprofile.html           # User profile
├── wishlist.html              # Wishlist
├── search-results.html        # Search results
├── order-tracking.html        # Order tracking
├── compare.html               # Product comparison
├── blog.html                  # Blog / articles
├── becomeseller.html          # Become a seller
├── review.html                # Product review
├── notification.html          # Notifications
├── email-notifications.html   # Email notification settings
├── setting.html               # User settings
├── work-for-people.html       # Social impact: work for people
├── workfor.html               # Work for people variant
├── workforchild.html          # Support children donation
├── workforpeople.html         # Work for people variant
├── food&natural.html          # Food & Natural category
├── sweets&dairy.html          # Sweets & Dairy category
├── sweets&food.html           # Sweets & Food category
├── handricrafts.html          # Handicrafts category
├── clothing.html              # Clothing category
├── book.html                  # Book product detail
├── book-categories.html       # Book category browser
├── antique.html               # Antique category
├── coin1.html                 # Antique coin detail
├── coin2.html                 # Antique coin detail
├── coin4.html                 # Antique coin detail
├── manifest.json              # PWA manifest
├── service-worker.js          # Offline caching + background sync
├── robots.txt                 # SEO directives
├── sitemap.xml                # 14 URLs for SEO
├── .gitignore                 # Git ignore rules
│
├── assets/
│   ├── css/                   # 10 stylesheets
│   │   ├── amazon-style.css   # Main design system (1382 lines)
│   │   ├── dark-mode.css      # Dark theme overrides
│   │   ├── auth.css           # Auth page (split-panel, gradient)
│   │   ├── book.css           # Book detail (glassmorphism)
│   │   ├── book-categories.css # Book category sidebar
│   │   ├── clothing.css       # Clothing product page
│   │   ├── seller-dashboard.css # Seller dashboard
│   │   ├── userprofile.css    # User profile
│   │   ├── work-for-child.css # Children donation page
│   │   └── work-for-people.css # People donation page
│   │
│   ├── js/                    # 18 JavaScript modules
│   │   ├── api.js             # Centralized API client (MR_API)
│   │   ├── auth.js            # Auth page toggle (login/register)
│   │   ├── auth-shared.js     # MR_Auth module: C# API auth
│   │   ├── cart.js            # MR_Cart module: cart management
│   │   ├── products.js        # MR_PRODUCTS array (15+ products)
│   │   ├── wishlist.js        # MR_Wishlist module
│   │   ├── search.js          # MR_Search module: debounced search
│   │   ├── loader.js          # Non-blocking font/icon loader
│   │   ├── darkmode.js        # Dark mode toggle
│   │   ├── i18n.js            # English/Bengali translations
│   │   ├── book-categories.js # Book category sidebar
│   │   ├── book.js            # Book product page
│   │   ├── clothing.js        # Clothing product page
│   │   ├── seller-dashboard.js # Seller dashboard
│   │   ├── seller-settings.js # Seller settings
│   │   ├── userprofile.js     # User profile
│   │   ├── work-for-child.js  # Children donation
│   │   └── work-for-people.js # People donation
│   │
│   └── images/                # 49 local image assets
│       ├── mrlogo.png         # Main logo
│       ├── *.jpg              # Product photos, icons, backgrounds
│       └── *.jpeg             # Coin images
│
└── backend/
    ├── chat_server.py         # HTTP server (port 8000)
    ├── chatwithus.py          # TF-IDF index + query engine
    └── local_site_agent.py    # Empty placeholder
```

---

## Design System

### Color Palette

| Variable | Value | Usage |
|----------|-------|-------|
| `--amazon-dark` | `#131921` | Header background |
| `--amazon-orange` | `#febd69` | Search button, accents |
| `--amazon-yellow` | `#ff9900` | Primary brand color |
| `--amazon-blue` | `#146eb4` | "Prime" badges, links |
| `--amazon-green` | `#007600` | In-stock, free delivery |
| `--accent` | `#7c3aed` | Purple accent (auth/book/clothing) |
| `--accent-2` | `#06b6d4` | Cyan accent |

### Typography

- **Primary:** Inter (sans-serif) — main UI
- **Serif headings:** Playfair Display — book/antique pages
- **Friendly:** Nunito + Fredoka — children donation page

### CSS Naming Conventions

| Prefix | Usage |
|--------|-------|
| `.amz-*` | Amazon-style pages |
| `.bk-*` | Book pages |
| `.rc-*` | Clothing pages |
| `.wfc-*` | Work-for-children pages |
| `.wfp-*` | Work-for-people pages |

---

## Data & State Management

### localStorage Keys

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

### API Communication

- **C# Backend:** `localhost:5000/api` (expected, not in repo)
- **Python Chat:** `localhost:8000` (TF-IDF Q&A)
- All API calls have localStorage fallbacks for offline/demo mode

---

## Getting Started

### Frontend (Static)

```bash
# Open in browser
open index.html

# Or use a local server
python3 -m http.server 8080
# Then visit http://localhost:8080
```

### Chat Backend

```bash
cd backend
python3 chat_server.py
# Server runs on http://localhost:8000
# API endpoint: POST /api/query
```

### Full Stack (with C# API)

```bash
# 1. Start C# backend on localhost:5000
# 2. Start Python chat server on localhost:8000
# 3. Open frontend in browser
```

---

## API Endpoints (Python Chat Server)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/query` | Ask a question, get TF-IDF matched answer |
| POST | `/api/reindex` | Rebuild the content index |
| POST | `/api/report` | Upload bug report file |

### Rate Limiting
- 30 requests per minute per IP
- Max file size: 10MB
- Input sanitization on all endpoints

---

## SEO

- `robots.txt` — blocks admin/seller pages from crawlers
- `sitemap.xml` — 14 URLs for mrshopbd.com
- Meta descriptions and keywords on all pages
- Semantic HTML structure

---

## Coupon Codes

| Code | Discount |
|------|----------|
| `MRSHOP100` | ৳100 off |
| `MRSHOP50` | 5% off |
| `FREESHIP` | Free shipping |

Free shipping on orders over ৳5,000.

---

## Deployment

### Vercel (Current)

- Project: `mr-project`
- Domains: `mrshopbangladesh.tech`, `www.mrshopbangladesh.tech`
- Auto-deploys from GitHub `main` branch

### GitHub

- Repo: https://github.com/mdrafiullah1830/mr-project

---

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test locally
5. Submit a pull request

---

## License

© MR IT Company. All rights reserved.

---

## Contact

- **Website:** https://mrshopbd.com
- **GitHub:** https://github.com/mdrafiullah1830/mr-project

---

*Built with ❤️ for Bangladesh*
