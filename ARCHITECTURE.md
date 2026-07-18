# MR Shop — System Architecture & Workflow

> Generated: 2026-07-18  
> Project: MR Shop (mrmart18) — Bangladesh's #1 Online Marketplace

## High-Level Workflow

```mermaid
flowchart TD
    User([User / Browser]) -->|HTTPS| FE[Frontend<br/>Static HTML/CSS/JS<br/>Vercel / localhost:8080]

    subgraph Client[Client-side Modules]
        API[api.js - MR_API client]
        Auth[auth-shared.js - MR_Auth]
        Cart[cart.js - MR_Cart]
        Wish[wishlist.js - MR_Wishlist]
        Search[search.js - MR_Search]
    end

    FE --> Client

    Client -->|JWT Bearer| APIGW{API Gateway<br/>MRShop.API<br/>:5000 / Azure}

    subgraph Backend[C# ASP.NET Core 8]
        AuthC[AuthController /customerauth]
        ProdC[ProductsController /products]
        CartC[CartController /cart]
        WishC[WishlistController /wishlist]
        OrdC[OrdersController /orders]
        JWT[JwtService]
        Rate[Rate Limiting Middleware]
    end

    APIGW --> Rate --> AuthC & ProdC & CartC & WishC & OrdC
    AuthC -.generates.-> JWT

    subgraph Data[(MongoDB Atlas)]
        Users[(users)]
        Products[(products)]
        CartItems[(cartItems)]
        Wishlist[(wishlistItems)]
        Orders[(orders)]
    end

    AuthC --> Users
    ProdC --> Products
    CartC --> CartItems
    WishC --> Wishlist
    OrdC --> Orders & CartItems

    User -->|Login/Register| Auth
    Auth -->|POST /customerauth/login| AuthC
    AuthC -->|returns JWT| Auth
    Auth -->|stores mr_shop_token| FE

    Cart -->|POST /cart| CartC
    CartC -->|on checkout| OrdC
    OrdC -->|clears cart| CartItems

    User -->|Ask question| Chat[chat.html]
    Chat -->|POST /api/query| PySrv[Python Chat Server :8000 TF-IDF]
    PySrv -->|cosine similarity| Site[Site .html/.md files]

    AuthC -->|Google OAuth| Google[Google Identity]
```

## Components

| Layer | Technology | Port |
|-------|-----------|------|
| Frontend | Static HTML + Vanilla CSS/JS | 8080 (Vercel in prod) |
| API | C# ASP.NET Core 8 + MongoDB | 5000 (Azure) |
| Chat | Python 3 (TF-IDF Q&A) | 8000 |
| Database | MongoDB Atlas | 27017 |

## Key Flows

- **Auth:** `auth-shared.js` → `POST /customerauth/login|register` → JWT stored in `localStorage` (`mr_shop_token`)
- **Cart → Order:** items in `cartItems` → `POST /orders` creates order and clears cart
- **Wishlist:** synced to `wishlistItems` per user
- **Chat:** separate Python TF-IDF service (not part of the C# API)
- **External:** Google OAuth for social login

## Known Issues (from analysis)

1. 🔴 Insecure password hashing (SHA256 + static salt) — should use BCrypt
2. 🔴 Dockerfile targets `net10.0` but csproj is `net8.0`
3. 🔴 Committed `bin/` and `obj/` build artifacts
4. 🟡 No model validation on DTOs
5. 🟡 Order creation doesn't decrement product stock
6. 🟡 Hardcoded Google Client ID
