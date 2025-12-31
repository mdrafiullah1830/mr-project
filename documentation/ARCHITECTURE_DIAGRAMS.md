# 🏗️ Order Tracking Architecture & Flow Diagrams

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      MR SHOP SYSTEM                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────┐         ┌──────────────┐   ┌─────────────────┐
│  │   Frontend   │         │  Backend     │   │  Data Storage   │
│  │   (Browser)  │         │  (Flask)     │   │  (JSON Files)   │
│  ├──────────────┤         ├──────────────┤   ├─────────────────┤
│  │              │         │              │   │                 │
│  │ • index.html │──────→  │ • admin.py   │─→ │ • orders.json   │
│  │ • orders.html│  API    │   (5 routes) │   │ • users.json    │
│  │ • cart.html  │ calls   │              │   │ • products.json │
│  │ • checkout   │         │ • GET /api/  │   │                 │
│  │              │←────────│   orders     │   │                 │
│  │ JavaScript   │ JSON    │ • GET /api/  │   │                 │
│  │ • Fetch API  │response │   orders/<id>│   │                 │
│  │ • DOM render │         │ • GET /api/  │   │                 │
│  │ • Filtering  │         │   orders/<id>/│  │                 │
│  │ • Modal UI   │         │   timeline   │   │                 │
│  │              │         │ • POST /api/ │   │                 │
│  │              │         │   orders     │   │                 │
│  │              │         │ • PUT /api/  │   │                 │
│  │              │         │   orders/<id>/│  │                 │
│  │              │         │   status     │   │                 │
│  └──────────────┘         └──────────────┘   └─────────────────┘
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## API Request/Response Flow

```
┌──────────────────────────────────────────────────────────────────┐
│ CUSTOMER VIEWING ORDERS                                           │
├──────────────────────────────────────────────────────────────────┤
│                                                                    │
│  1. PAGE LOAD                                                     │
│     ┌─────────────────────┐                                       │
│     │ orders.html loaded  │                                       │
│     │ JavaScript starts   │                                       │
│     │ DOMContentLoaded    │                                       │
│     └──────────┬──────────┘                                       │
│                │                                                  │
│  2. FETCH REQUEST                                                 │
│     ┌──────────┴──────────┐                                       │
│     │ fetch('/api/orders')│                                       │
│     │ user_id: 1          │                                       │
│     └──────────┬──────────┘                                       │
│                │                                                  │
│  3. BACKEND PROCESSING                                            │
│     ┌──────────┴──────────────────────────┐                      │
│     │ admin.py @app.get('/api/orders')   │                      │
│     │ • Read data/orders.json             │                      │
│     │ • Filter by user_id                 │                      │
│     │ • Sort by created_at (desc)         │                      │
│     └──────────┬──────────────────────────┘                      │
│                │                                                  │
│  4. JSON RESPONSE                                                 │
│     ┌──────────┴──────────────────────────┐                      │
│     │ HTTP 200 OK                         │                      │
│     │ {                                    │                      │
│     │   "success": true,                  │                      │
│     │   "count": 3,                       │                      │
│     │   "orders": [                       │                      │
│     │     {...order1...},                 │                      │
│     │     {...order2...},                 │                      │
│     │     {...order3...}                  │                      │
│     │   ]                                 │                      │
│     │ }                                    │                      │
│     └──────────┬──────────────────────────┘                      │
│                │                                                  │
│  5. FRONTEND RENDERING                                            │
│     ┌──────────┴──────────┐                                       │
│     │ Process JSON        │                                       │
│     │ Render HTML cards   │                                       │
│     │ Set up event        │                                       │
│     │ listeners           │                                       │
│     └──────────┬──────────┘                                       │
│                │                                                  │
│  6. DISPLAY TO USER                                               │
│     ┌──────────┴──────────────────────────┐                      │
│     │ Order cards visible                 │                      │
│     │ Filter buttons active               │                      │
│     │ "Track Order" buttons ready         │                      │
│     └──────────────────────────────────────┘                      │
│                                                                    │
└──────────────────────────────────────────────────────────────────┘
```

---

## Order State Diagram

```
                     New Order Created
                            │
                            ↓
                    ┌─────────────────┐
                    │   CONFIRMED ✓   │ ← Customer pays
                    │ (Order placed)   │
                    └────────┬────────┘
                             │
                    (Seller packs order)
                             │
                             ↓
                    ┌─────────────────┐
                    │   PACKED 📦     │
                    │ (Ready to ship)  │
                    └────────┬────────┘
                             │
                    (Handed to courier)
                             │
                             ↓
                    ┌─────────────────┐
                    │   SHIPPED 🚚    │
                    │ (In warehouse)   │
                    └────────┬────────┘
                             │
                    (Out with delivery)
                             │
                             ↓
                    ┌─────────────────┐
                    │  IN TRANSIT 🏃  │
                    │ (On the way)     │
                    └────────┬────────┘
                             │
                    (Reached destination)
                             │
                             ↓
                    ┌─────────────────┐
                    │  DELIVERED ✅   │
                    │ (Customer got)   │
                    └─────────────────┘

    (Optional: Customer can cancel before SHIPPED)
                             │
                             ↓
                    ┌─────────────────┐
                    │   CANCELLED ❌  │
                    │ (Order cancelled)│
                    └─────────────────┘
```

---

## Order Card Component Structure

```
┌──────────────────────────────────────────────────────┐
│ ORDER CARD                                           │
├──────────────────────────────────────────────────────┤
│                                                      │
│ ┌────────────────────────────────────────────────┐  │
│ │ ORDER HEADER                                   │  │
│ │ ┌─────────────────┐    ┌──────────────────┐   │  │
│ │ │ Order #MR-2025 │    │ ✅ DELIVERED    │   │  │
│ │ │ 6 days ago     │    │ (Status Badge)   │   │  │
│ │ └─────────────────┘    └──────────────────┘   │  │
│ └────────────────────────────────────────────────┘  │
│                                                      │
│ ┌────────────────────────────────────────────────┐  │
│ │ ORDER ITEMS                                    │  │
│ │ ┌─────┐ ┌──────────────────────┐ ┌─────────┐  │  │
│ │ │ Img │ │ Item: The Art Code   │ │ ৳1,450 │  │  │
│ │ │     │ │ Qty: 1               │ │        │  │  │
│ │ └─────┘ └──────────────────────┘ └─────────┘  │  │
│ │                                                │  │
│ │ ┌─────┐ ┌──────────────────────┐ ┌─────────┐  │  │
│ │ │ Img │ │ Item: Silver Coin x2 │ │ ৳1,700 │  │  │
│ │ │     │ │ Qty: 2               │ │        │  │  │
│ │ └─────┘ └──────────────────────┘ └─────────┘  │  │
│ └────────────────────────────────────────────────┘  │
│                                                      │
│ ┌────────────────────────────────────────────────┐  │
│ │ SUMMARY                                        │  │
│ │ Payment: 🏦 bKash  │  Total: ৳3,150          │  │
│ │ Tracking: DHC-2025-123456                     │  │
│ └────────────────────────────────────────────────┘  │
│                                                      │
│ ┌────────────────────────────────────────────────┐  │
│ │ TIMELINE PREVIEW                               │  │
│ │ ✓ ━ 📦 ━ 🚚 ━ 🏃 ━ ✅ Delivered              │  │
│ └────────────────────────────────────────────────┘  │
│                                                      │
│ ┌────────────────────────────────────────────────┐  │
│ │ ACTIONS                                        │  │
│ │ [📍 Track Order]    [💬 Contact Seller]       │  │
│ └────────────────────────────────────────────────┘  │
│                                                      │
└──────────────────────────────────────────────────────┘
```

---

## Timeline Modal Visualization

```
╔════════════════════════════════════════════════════════════╗
║ 📦 ORDER TIMELINE                                [✕]       ║
╠════════════════════════════════════════════════════════════╣
║                                                             ║
║ Order ID: MR-2025-0001                                    ║
║ Tracking ID: DHC-2025-123456                              ║
║                                                             ║
║ ┌──────────────────────────────────────────────────────┐  ║
║ │ COMPLETE TIMELINE                                    │  ║
║ │                                                      │  ║
║ │ ● Order Confirmed                                   │  ║
║ │   Nov 28, 2025 10:00 AM                            │  ║
║ │   Order confirmed                                  │  ║
║ │                                                      │  ║
║ │ ● Packed                                            │  ║
║ │   Nov 29, 2025 2:30 PM                             │  ║
║ │   Order packed and ready for shipment              │  ║
║ │                                                      │  ║
║ │ ● Shipped                                           │  ║
║ │   Nov 30, 2025 8:00 AM                             │  ║
║ │   Order shipped from warehouse                     │  ║
║ │                                                      │  ║
║ │ ● Out for Delivery                                 │  ║
║ │   Dec 5, 2025 9:00 AM                              │  ║
║ │   Out for delivery                                 │  ║
║ │                                                      │  ║
║ │ ◉ Delivered (CURRENT)                              │  ║
║ │   Dec 6, 2025 4:45 PM                              │  ║
║ │   Order delivered successfully                     │  ║
║ │                                                      │  ║
║ └──────────────────────────────────────────────────────┘  ║
║                                                             ║
║ Shipping Address:                                          ║
║ Rafi Ullah                                                 ║
║ Dhaka, Bangladesh                                          ║
║ +880 1712-345678                                           ║
║                                                             ║
╚════════════════════════════════════════════════════════════╝
```

---

## Data Model Hierarchy

```
Order (Root)
├── id: string (MR-2025-0001)
├── user_id: number (1)
├── user_name: string (Rafi Ullah)
│
├── items: array [
│   ├── id: number (401)
│   ├── name: string (The Art of Code)
│   ├── category: string (books)
│   ├── quantity: number (1)
│   ├── price: number (1450)
│   └── image: string (path/to/image.jpg)
│   ... more items
│ ]
│
├── total: number (3150)
├── payment_method: string (bkash|nagad|cod)
│
├── shipping_address: object
│   ├── name: string
│   ├── phone: string
│   ├── address: string
│   └── zip_code: string
│
├── status: string (confirmed|packed|shipped|
│            out-for-delivery|delivered|cancelled)
├── tracking_id: string (DHC-2025-123456)
│
├── timeline: array [
│   ├── stage: string
│   ├── date: string (ISO 8601)
│   └── description: string
│   ... more stages
│ ]
│
├── created_at: string (ISO 8601)
└── updated_at: string (ISO 8601)
```

---

## Filter Flow

```
┌─────────────────────────────────────────────────────────┐
│ USER CLICKS FILTER BUTTON                               │
├─────────────────────────────────────────────────────────┤
│                                                           │
│  Filter Button Clicked                                   │
│         │                                                │
│         ↓                                                │
│  ┌──────────────────┐                                    │
│  │ Update Visual    │                                    │
│  │ (Add .active)    │                                    │
│  └────────┬─────────┘                                    │
│           │                                              │
│           ↓                                              │
│  ┌──────────────────────────┐                            │
│  │ Get selected status      │                            │
│  │ (all/confirmed/shipped) │                            │
│  └────────┬─────────────────┘                            │
│           │                                              │
│           ↓                                              │
│  ┌──────────────────────────┐                            │
│  │ Filter allOrders array   │                            │
│  │ Keep only matching       │                            │
│  └────────┬─────────────────┘                            │
│           │                                              │
│           ↓                                              │
│  ┌──────────────────────────┐                            │
│  │ Call renderOrders()      │                            │
│  │ with filtered array      │                            │
│  └────────┬─────────────────┘                            │
│           │                                              │
│           ↓                                              │
│  ┌──────────────────────────┐                            │
│  │ Generate HTML for        │                            │
│  │ matching orders          │                            │
│  └────────┬─────────────────┘                            │
│           │                                              │
│           ↓                                              │
│  ┌──────────────────────────┐                            │
│  │ Update DOM with HTML     │                            │
│  │ (innerHTML)              │                            │
│  └────────┬─────────────────┘                            │
│           │                                              │
│           ↓                                              │
│  ┌──────────────────────────┐                            │
│  │ User sees filtered       │                            │
│  │ orders                   │                            │
│  └──────────────────────────┘                            │
│                                                           │
└─────────────────────────────────────────────────────────┘
```

---

## Mobile Responsive Breakpoints

```
Desktop (1200px+)
┌────────────────────────────────────────┐
│ 📦 My Orders                           │
│ Track your orders and manage purchases│
│ [All] [Confirmed] [Shipped] [Transit] │
├────────────────────────────────────────┤
│ ┌──────────────┐  ┌──────────────┐    │
│ │ Order Card 1 │  │ Order Card 2 │    │
│ │ (side by     │  │ (side by     │    │
│ │  side)       │  │  side)       │    │
│ └──────────────┘  └──────────────┘    │
│ ┌──────────────┐  ┌──────────────┐    │
│ │ Order Card 3 │  │ Order Card 4 │    │
│ └──────────────┘  └──────────────┘    │
└────────────────────────────────────────┘

Tablet (768px - 1199px)
┌──────────────────────────┐
│ 📦 My Orders             │
│ Track your orders...     │
│ [All] [Confirmed]        │
│ [Shipped] [Transit]      │
├──────────────────────────┤
│ ┌──────────────────────┐ │
│ │ Order Card 1         │ │
│ │ (2 per row)          │ │
│ └──────────────────────┘ │
│ ┌──────────────────────┐ │
│ │ Order Card 2         │ │
│ └──────────────────────┘ │
│ ┌──────────────────────┐ │
│ │ Order Card 3         │ │
│ └──────────────────────┘ │
└──────────────────────────┘

Mobile (<768px)
┌──────────────┐
│ 📦 My Orders │
│ Track your.. │
│ [All]        │
│ [Confirmed]  │
│ [Shipped]    │
│ [Transit]    │
├──────────────┤
│ ┌────────────┐
│ │ Order Card1│
│ │ (full)     │
│ │            │
│ └────────────┘
│ ┌────────────┐
│ │ Order Card2│
│ │ (full)     │
│ │            │
│ └────────────┘
│ ┌────────────┐
│ │ Order Card3│
│ │ (full)     │
│ │            │
│ └────────────┘
└──────────────┘
```

---

## Integration Points

```
┌─────────────────────────────────────────────────────────────┐
│ ORDER TRACKING INTEGRATION WITH OTHER FEATURES             │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────┐                                           │
│  │   SHOPPING   │                                           │
│  │    CART      │                                           │
│  └─────┬────────┘                                           │
│        │ Add items to cart                                 │
│        ↓                                                    │
│  ┌──────────────┐                                           │
│  │   CHECKOUT   │                                           │
│  │   PAGE       │                                           │
│  └─────┬────────┘                                           │
│        │ Customer reviews & pays                           │
│        ↓                                                    │
│  ┌──────────────┐                                           │
│  │  PAYMENT     │                                           │
│  │  GATEWAY     │                                           │
│  └─────┬────────┘                                           │
│        │ POST /api/orders (create order)                   │
│        ↓                                                    │
│  ┌──────────────────────────┐                              │
│  │  ORDER TRACKING (YOU)    │                              │
│  │  • View orders           │                              │
│  │  • Track delivery        │                              │
│  │  • See timeline          │                              │
│  └─────┬────────────────────┘                              │
│        │ PUT /api/orders/<id>/status                      │
│        ↓                                                    │
│  ┌──────────────┐                                           │
│  │  ADMIN       │                                           │
│  │  DASHBOARD   │                                           │
│  └─────┬────────┘                                           │
│        │ Update order status                               │
│        ↓                                                    │
│  ┌──────────────┐                                           │
│  │  EMAIL/SMS   │                                           │
│  │  NOTIF       │                                           │
│  └──────────────┘                                           │
│  Send customer updates                                      │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## Component Lifecycle

```
┌──────────────────────────────────────────────────────┐
│ ORDERS.HTML LIFECYCLE                                │
├──────────────────────────────────────────────────────┤
│                                                        │
│ 1. BROWSER LOADS FILE                                 │
│    ↓                                                  │
│ 2. HTML PARSES (DOM created)                         │
│    ↓                                                  │
│ 3. CSS LOADS (styles applied)                        │
│    ↓                                                  │
│ 4. JAVASCRIPT EXECUTES                               │
│    ├─ allOrders = []                                 │
│    ├─ currentFilter = 'all'                          │
│    └─ Event listeners registered                     │
│    ↓                                                  │
│ 5. DOM CONTENT LOADED EVENT                          │
│    └─ Triggers loadOrders()                          │
│    ↓                                                  │
│ 6. FETCH /API/ORDERS                                 │
│    └─ allOrders populated                            │
│    ↓                                                  │
│ 7. RENDER ORDERS                                     │
│    └─ Cards inserted to DOM                          │
│    ↓                                                  │
│ 8. USER INTERACTION READY                            │
│    ├─ Can click filter buttons                       │
│    ├─ Can click "Track Order"                        │
│    └─ Can click "Contact Seller"                     │
│    ↓                                                  │
│ 9. USER FILTERS ORDERS                               │
│    ├─ filterOrders(status) called                    │
│    ├─ Array filtered client-side                     │
│    └─ renderOrders() called again                    │
│    ↓                                                  │
│ 10. USER CLICKS TRACK ORDER                          │
│     ├─ viewTimeline(orderId) called                  │
│     ├─ Modal opens with timeline                     │
│     └─ User can read full details                    │
│    ↓                                                  │
│ 11. USER CLOSES MODAL                                │
│     └─ closeModal() removes display                  │
│    ↓                                                  │
│ PAGE CONTINUES RUNNING...                            │
│ (Ready for more interactions)                         │
│                                                        │
└──────────────────────────────────────────────────────┘
```

---

**All diagrams are ASCII-based and will display correctly in markdown viewers, code editors, and terminal windows.** 📊

