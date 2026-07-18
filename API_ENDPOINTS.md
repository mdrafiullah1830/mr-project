# MR Shop - API Endpoints Reference

**Base URL:** `https://localhost:5000/api` (local) or `https://<deployed-url>/api`

---

## Authentication

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/customerauth/register` | None | Register new user |
| POST | `/customerauth/login` | None | Login with email/password |
| POST | `/customerauth/google` | None | Login/register with Google OAuth |
| GET | `/customerauth/me` | JWT | Get current user profile |
| PUT | `/customerauth/profile` | JWT | Update user profile |
| PUT | `/customerauth/password` | JWT | Change password |

### Request/Response Examples

#### POST /customerauth/register
```json
// Request
{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "password123",
  "phone": "+8801712345678",
  "address": "Dhaka, Bangladesh"
}

// Response 200
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": "64a1b2c3d4e5f6a7b8c9d0e1",
    "name": "John Doe",
    "email": "john@example.com",
    "phone": "+8801712345678",
    "address": "Dhaka, Bangladesh",
    "role": "customer",
    "profilePhoto": null
  }
}
```

#### POST /customerauth/login
```json
// Request
{
  "email": "john@example.com",
  "password": "password123"
}

// Response 200
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": { ... }
}
```

#### POST /customerauth/google
```json
// Request
{
  "credential": "eyJhbGciOiJSUzI1NiIs..."  // Google JWT
}

// Response 200
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": "64a1b2c3d4e5f6a7b8c9d0e1",
    "name": "John Doe",
    "email": "john@gmail.com",
    "role": "customer",
    "profilePhoto": "https://lh3.googleusercontent.com/..."
  }
}
```

---

## Products

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/products` | None | List/search products |
| GET | `/products/{id}` | None | Get single product |
| POST | `/products` | Admin/Seller | Create product |
| PUT | `/products/{id}` | Admin/Seller | Update product |
| DELETE | `/products/{id}` | Admin/Seller | Delete product |

### Query Parameters (GET /products)
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| category | string | - | Filter by category |
| search | string | - | Regex search on name/description |
| sort | string | newest | Sort: price_asc, price_desc, rating, newest |
| page | int | 1 | Page number |
| limit | int | 20 | Items per page |

---

## Cart

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/cart` | JWT | Get user's cart |
| POST | `/cart` | JWT | Add item to cart |
| DELETE | `/cart/{id}` | JWT | Remove single item |
| DELETE | `/cart` | JWT | Clear entire cart |

### POST /cart Request
```json
{
  "productId": "64a1b2c3d4e5f6a7b8c9d0e1",
  "productName": "Sundarbans Honey",
  "price": 450,
  "image": "assets/images/honey.jpg",
  "quantity": 2
}
```

---

## Orders

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/orders` | JWT | Get user's orders |
| GET | `/orders/{id}` | JWT | Get single order |
| POST | `/orders` | JWT | Create order from cart |
| PUT | `/orders/{id}/status` | Admin/Seller | Update order status |

### Order Statuses
`pending` → `confirmed` → `shipped` → `delivered` | `cancelled`

---

## Wishlist

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/wishlist` | JWT | Get user's wishlist |
| POST | `/wishlist` | JWT | Add item to wishlist |
| DELETE | `/wishlist/{id}` | JWT | Remove by document ID |
| DELETE | `/wishlist/product/{productId}` | JWT | Remove by product ID |
| DELETE | `/wishlist` | JWT | Clear entire wishlist |

---

## System

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/` | None | API info/metadata |
| GET | `/health` | None | Health check |
| GET | `/swagger` | None | Swagger UI |

---

## Python Chat Backend

**Base URL:** `http://localhost:8000`

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/query` | Ask a question |
| POST | `/api/reindex` | Rebuild content index |
| POST | `/api/report` | Upload bug report |

### POST /api/query
```json
// Request
{ "question": "What payment methods do you accept?" }

// Response
{
  "results": [
    { "path": "payment.html", "score": 0.85, "snippet": "We accept bKash, Nagad..." }
  ]
}
```

---

## Error Responses

All endpoints return errors in this format:
```json
{
  "message": "Error description",
  "detail": "Technical details (dev only)"
}
```

| Status Code | Meaning |
|-------------|---------|
| 400 | Bad request / validation error |
| 401 | Unauthorized / invalid token |
| 403 | Forbidden / insufficient permissions |
| 404 | Resource not found |
| 409 | Conflict (e.g., email already exists) |
| 500 | Internal server error |
