# MR Shop - Database Schema (MongoDB Atlas)

**Database Name:** `mrshop`

---

## Collection: users

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| _id | ObjectId | Auto | Unique identifier |
| Name | string | Yes | Full name |
| Email | string | Yes | Email address (lowercase) |
| PasswordHash | string | Yes | SHA-256 hash + static salt |
| Phone | string | No | Phone number |
| Address | string | No | Physical address |
| Role | string | Yes | "customer", "seller", or "admin" |
| ProfilePhoto | string | No | Profile image URL |
| CreatedAt | DateTime | Yes | Account creation timestamp |
| UpdatedAt | DateTime | Yes | Last update timestamp |

**Indexes:** None configured (MISSING - should add unique index on Email)

```json
{
  "_id": ObjectId("64a1b2c3d4e5f6a7b8c9d0e1"),
  "Name": "John Doe",
  "Email": "john@example.com",
  "PasswordHash": "abc123hash...",
  "Phone": "+8801712345678",
  "Address": "Dhaka, Bangladesh",
  "Role": "customer",
  "ProfilePhoto": null,
  "CreatedAt": ISODate("2026-07-06T10:00:00Z"),
  "UpdatedAt": ISODate("2026-07-06T10:00:00Z")
}
```

---

## Collection: products

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| _id | ObjectId | Auto | Unique identifier |
| Name | string | Yes | Product name |
| Description | string | Yes | Product description |
| Price | decimal | Yes | Current price (BDT) |
| OriginalPrice | decimal | No | Original price for discount display |
| Category | string | Yes | Category slug |
| Subcategory | string | No | Subcategory slug |
| Image | string | Yes | Primary image URL |
| Images | array[string] | No | Additional image URLs |
| Stock | int | Yes | Available stock quantity |
| Rating | double | Yes | Average rating (0-5) |
| ReviewCount | int | Yes | Number of reviews |
| SellerId | string | No | Seller reference (not populated) |
| IsActive | boolean | Yes | Product visibility (default: true) |
| CreatedAt | DateTime | Yes | Creation timestamp |
| UpdatedAt | DateTime | Yes | Last update timestamp |

**Indexes:** None configured

```json
{
  "_id": ObjectId("64a1b2c3d4e5f6a7b8c9d0e2"),
  "Name": "Sundarbans Pure Honey",
  "Description": "100% raw honey from Sundarbans...",
  "Price": 450.00,
  "OriginalPrice": 550.00,
  "Category": "food-natural",
  "Subcategory": "honey",
  "Image": "assets/images/honey.jpg",
  "Images": [],
  "Stock": 50,
  "Rating": 4.5,
  "ReviewCount": 23,
  "SellerId": null,
  "IsActive": true,
  "CreatedAt": ISODate("2026-07-06T10:00:00Z"),
  "UpdatedAt": ISODate("2026-07-06T10:00:00Z")
}
```

---

## Collection: cartItems

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| _id | ObjectId | Auto | Unique identifier |
| UserId | string | Yes | User reference |
| ProductId | string | Yes | Product reference |
| ProductName | string | Yes | Denormalized product name |
| Price | decimal | Yes | Denormalized price |
| Image | string | Yes | Denormalized image URL |
| Quantity | int | Yes | Item quantity |
| CreatedAt | DateTime | Yes | Addition timestamp |

**Indexes:** None configured (should add index on UserId)

```json
{
  "_id": ObjectId("64a1b2c3d4e5f6a7b8c9d0e3"),
  "UserId": "64a1b2c3d4e5f6a7b8c9d0e1",
  "ProductId": "64a1b2c3d4e5f6a7b8c9d0e2",
  "ProductName": "Sundarbans Pure Honey",
  "Price": 450.00,
  "Image": "assets/images/honey.jpg",
  "Quantity": 2,
  "CreatedAt": ISODate("2026-07-06T10:00:00Z")
}
```

---

## Collection: orders

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| _id | ObjectId | Auto | Unique identifier |
| UserId | string | Yes | User reference |
| Items | array[OrderItem] | Yes | Embedded order items |
| TotalAmount | decimal | Yes | Order total (BDT) |
| ShippingAddress | string | Yes | Delivery address |
| PaymentMethod | string | Yes | bkash, nagad, cod, card |
| Status | string | Yes | Order status |
| CreatedAt | DateTime | Yes | Order timestamp |
| UpdatedAt | DateTime | Yes | Last status update |

**OrderItem Schema:**
| Field | Type | Description |
|-------|------|-------------|
| ProductId | string | Product reference |
| ProductName | string | Denormalized name |
| Price | decimal | Price at time of order |
| Quantity | int | Ordered quantity |
| Image | string | Denormalized image |

**Valid Statuses:** `pending`, `confirmed`, `shipped`, `delivered`, `cancelled`

```json
{
  "_id": ObjectId("64a1b2c3d4e5f6a7b8c9d0e4"),
  "UserId": "64a1b2c3d4e5f6a7b8c9d0e1",
  "Items": [
    {
      "ProductId": "64a1b2c3d4e5f6a7b8c9d0e2",
      "ProductName": "Sundarbans Pure Honey",
      "Price": 450.00,
      "Quantity": 2,
      "Image": "assets/images/honey.jpg"
    }
  ],
  "TotalAmount": 1095.00,
  "ShippingAddress": "Dhaka, Bangladesh",
  "PaymentMethod": "bkash",
  "Status": "pending",
  "CreatedAt": ISODate("2026-07-06T10:00:00Z"),
  "UpdatedAt": ISODate("2026-07-06T10:00:00Z")
}
```

---

## Collection: wishlistItems

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| _id | ObjectId | Auto | Unique identifier |
| UserId | string | Yes | User reference |
| ProductId | string | Yes | Product reference |
| ProductName | string | Yes | Denormalized product name |
| Price | decimal | Yes | Denormalized price |
| Image | string | Yes | Denormalized image URL |
| CreatedAt | DateTime | Yes | Addition timestamp |

**Indexes:** None configured (should add index on UserId)

```json
{
  "_id": ObjectId("64a1b2c3d4e5f6a7b8c9d0e5"),
  "UserId": "64a1b2c3d4e5f6a7b8c9d0e1",
  "ProductId": "64a1b2c3d4e5f6a7b8c9d0e2",
  "ProductName": "Sundarbans Pure Honey",
  "Price": 450.00,
  "Image": "assets/images/honey.jpg",
  "CreatedAt": ISODate("2026-07-06T10:00:00Z")
}
```

---

## Schema Issues

| Issue | Severity | Description |
|-------|----------|-------------|
| No indexes configured | HIGH | Full collection scans on every query |
| No unique index on Email | HIGH | Duplicate emails possible |
| Denormalized cart/order data | MEDIUM | Price changes don't reflect in existing carts |
| SellerId never populated | MEDIUM | Product-seller relationship unused |
| No soft delete | LOW | Hard delete breaks order history |
| No transactions | LOW | Order creation + cart clear not atomic |
