# QUICK WINS: Daraz-like Features (Immediate - 2–4 Weeks)

These features can be **implemented today** using your current Flask + HTML/JS stack without major refactoring.

---

## 1. FUNCTIONAL SHOPPING CART

### Current State:
- Cart icon exists, but clicking it shows empty modal
- No add-to-cart logic
- No persistence across page reloads

### What to Build (2–3 days):
```javascript
// assets/js/cart.js (new file)
class ShoppingCart {
  constructor() {
    this.items = JSON.parse(localStorage.getItem('cart')) || [];
  }

  add(product) {
    const existing = this.items.find(i => i.id === product.id);
    if (existing) {
      existing.quantity += 1;
    } else {
      this.items.push({ ...product, quantity: 1 });
    }
    this.save();
    this.render();
  }

  remove(productId) {
    this.items = this.items.filter(i => i.id !== productId);
    this.save();
    this.render();
  }

  getTotal() {
    return this.items.reduce((sum, item) => sum + (item.price * item.quantity), 0);
  }

  save() {
    localStorage.setItem('cart', JSON.stringify(this.items));
  }

  render() {
    // Update cart UI count and total
  }

  checkout() {
    // Redirect to checkout page
    window.location.href = '/checkout.html';
  }
}

const cart = new ShoppingCart();
```

### UI Changes:
- Add "Add to Cart" button on all product pages
- Show cart count in navbar
- Create `checkout.html` page with:
  - Item review
  - Shipping address form
  - Payment method selection
  - Order summary

---

## 2. PRODUCT SEARCH & FILTERING

### Current State:
- Filters on category pages but search missing
- No global search bar

### What to Build (2–3 days):
```html
<!-- In index.html navbar -->
<input type="search" id="globalSearch" placeholder="Search products..." />

<script>
const search = document.getElementById('globalSearch');
const allProducts = [
  // Combine all products from all pages
  { name: 'The Art of Code', category: 'books', price: 1450 },
  { name: 'Rasgulla', category: 'sweets', price: 180 },
  // ... more
];

search.addEventListener('input', (e) => {
  const query = e.target.value.toLowerCase();
  const results = allProducts.filter(p => 
    p.name.toLowerCase().includes(query)
  );
  renderSearchResults(results);
});
</script>
```

### API Enhancement (Backend):
Add a `/api/products/search` endpoint:
```python
@app.get('/api/products/search')
def search_products():
  query = request.args.get('q', '').lower()
  # Search across all product categories
  results = [p for p in all_products if query in p['name'].lower()]
  return jsonify(results)
```

---

## 3. ORDER TRACKING

### Current State:
- Orders mentioned in user profile but not implemented
- No order history

### What to Build (3–4 days):
```html
<!-- orders.html (new page) -->
<div class="orders-list">
  <!-- Mock orders initially -->
  <div class="order-card">
    <h3>Order #MR-2025-001</h3>
    <p>Status: <span class="status-shipped">Shipped</span></p>
    <p>Items: 3</p>
    <p>Total: ৳ 5,450</p>
    <button onclick="trackOrder('MR-2025-001')">Track</button>
  </div>
</div>

<script>
function trackOrder(orderId) {
  // Show timeline: Confirmed → Packed → Shipped → Out for Delivery → Delivered
  const stages = ['confirmed', 'packed', 'shipped', 'out-for-delivery', 'delivered'];
  renderTimeline(stages);
}
</script>
```

### Backend Storage:
Update `data/orders.json`:
```json
[
  {
    "id": "MR-2025-001",
    "user_id": 1,
    "items": [
      { "product": "The Art of Code", "quantity": 1, "price": 1450 }
    ],
    "total": 5450,
    "status": "shipped",
    "tracking": "DHC-2025-123456",
    "created": "2025-12-01T10:00:00Z"
  }
]
```

---

## 4. PRODUCT REVIEWS & RATINGS

### Current State:
- Seller ratings exist in data but not shown
- No product reviews visible

### What to Build (3–4 days):
```html
<!-- Add to book.html, coin.html, etc. -->
<section class="product-reviews">
  <h2>Customer Reviews (4.5 ⭐ 32 reviews)</h2>
  
  <div class="review-form">
    <h3>Write a Review</h3>
    <input type="number" min="1" max="5" placeholder="Rating (1-5)" />
    <textarea placeholder="Your review..."></textarea>
    <button>Submit Review</button>
  </div>

  <div class="reviews-list">
    <div class="review-item">
      <span class="rating">⭐⭐⭐⭐⭐</span>
      <p class="review-title">Excellent Book!</p>
      <p class="review-text">Very informative and well-written...</p>
      <p class="reviewer">- Rafi Ullah <small>2 days ago</small></p>
    </div>
  </div>
</section>
```

### Data File (`data/reviews.json`):
```json
[
  {
    "product_id": 401,
    "user": "Rafi Ullah",
    "rating": 5,
    "title": "Excellent Book!",
    "comment": "Very informative...",
    "date": "2025-12-05"
  }
]
```

---

## 5. SELLER PROFILES & RATINGS

### Current State:
- Seller data exists in `data/sellers.json`
- Not displayed on product pages

### What to Build (2–3 days):
```html
<!-- Add to product pages -->
<div class="seller-info">
  <img src="seller-avatar.png" alt="Seller" />
  <h3>Best Books Store</h3>
  <p class="rating">⭐ 4.8 (142 reviews)</p>
  <p class="sales">15,234 sales</p>
  <button onclick="contactSeller()">Contact Seller</button>
</div>

<script>
function contactSeller() {
  // Open seller chat/messaging modal
  console.log('Open seller message form');
}
</script>
```

### Backend Update:
```python
@app.get('/api/sellers/<seller_id>')
def get_seller_profile(seller_id):
  sellers = read_json(DATA_DIR / 'sellers.json', [])
  seller = next((s for s in sellers if s['id'] == seller_id), None)
  if not seller:
    return jsonify({'error': 'Not found'}), 404
  
  # Add computed fields
  seller['review_count'] = 142
  seller['sales'] = 15234
  return jsonify(seller)
```

---

## 6. WISHLIST FUNCTIONALITY

### Current State:
- Wishlist mentioned but not functional
- No heart icon on products

### What to Build (2 days):
```javascript
// assets/js/wishlist.js
class Wishlist {
  constructor() {
    this.items = JSON.parse(localStorage.getItem('wishlist')) || [];
  }

  toggle(productId) {
    const exists = this.items.includes(productId);
    if (exists) {
      this.items = this.items.filter(id => id !== productId);
    } else {
      this.items.push(productId);
    }
    this.save();
    return !exists; // return true if just added
  }

  save() {
    localStorage.setItem('wishlist', JSON.stringify(this.items));
  }

  isWishlisted(productId) {
    return this.items.includes(productId);
  }
}

const wishlist = new Wishlist();
```

### UI Update:
```html
<button 
  onclick="toggleWishlist(productId)" 
  class="wishlist-btn"
  id="wishlist-btn-${productId}"
>
  ♡ Add to Wishlist
</button>

<script>
function toggleWishlist(productId) {
  const added = wishlist.toggle(productId);
  const btn = document.getElementById(`wishlist-btn-${productId}`);
  btn.classList.toggle('added', added);
  btn.textContent = added ? '♥ Added to Wishlist' : '♡ Add to Wishlist';
}
</script>
```

---

## 7. BASIC PAYMENT GATEWAY MOCK

### Current State:
- Payment methods listed but not functional
- No integration with Bkash, Nagad, etc.

### What to Build (3–4 days):
```html
<!-- checkout.html -->
<section class="payment-methods">
  <h2>Select Payment Method</h2>
  
  <label>
    <input type="radio" name="payment" value="bkash" />
    <span>🇧🇩 bKash (Recommended)</span>
  </label>
  
  <label>
    <input type="radio" name="payment" value="nagad" />
    <span>📱 Nagad</span>
  </label>

  <label>
    <input type="radio" name="payment" value="cod" />
    <span>💵 Cash on Delivery</span>
  </label>

  <button onclick="proceedToPayment()">Proceed to Payment</button>
</section>

<script>
function proceedToPayment() {
  const method = document.querySelector('input[name="payment"]:checked').value;
  
  if (method === 'bkash') {
    // Redirect to Bkash sandbox/production
    window.location.href = 'https://sandbox.bkashcluster.com/...';
  } else if (method === 'cod') {
    // Store order as "pending payment"
    submitOrder({ paymentMethod: 'cod', status: 'pending-payment' });
  }
}
</script>
```

### Backend:
```python
@app.post('/api/orders')
def create_order():
  payload = request.get_json()
  order = {
    'id': f"MR-{datetime.now().strftime('%Y%m%d%H%M%S')}",
    'items': payload['items'],
    'total': payload['total'],
    'payment_method': payload['payment_method'],
    'status': 'pending' if payload['payment_method'] != 'cod' else 'confirmed',
    'created_at': datetime.utcnow().isoformat()
  }
  orders = read_json(DATA_DIR / 'orders.json', [])
  orders.append(order)
  write_json(DATA_DIR / 'orders.json', orders)
  return jsonify({'success': True, 'order_id': order['id']})
```

---

## 8. EMAIL NOTIFICATIONS

### Current State:
- Notification system exists locally
- No email sending

### What to Build (2 days):
```python
# backend/notifications.py
from flask_mail import Mail, Message

mail = Mail()

def send_order_confirmation_email(user_email, order_id):
  msg = Message(
    subject=f'Order Confirmation: {order_id}',
    recipients=[user_email],
    html=f'''
    <h2>Thank you for your order!</h2>
    <p>Order ID: {order_id}</p>
    <p><a href="https://mrshop.com/orders/{order_id}">Track your order</a></p>
    '''
  )
  mail.send(msg)

# In admin.py
from notifications import send_order_confirmation_email

@app.post('/api/orders')
def create_order():
  # ... existing code ...
  send_order_confirmation_email(user['email'], order['id'])
  return jsonify({'success': True})
```

### Configuration:
```python
# config.py
MAIL_SERVER = 'smtp.gmail.com'  # or SendGrid
MAIL_PORT = 587
MAIL_USE_TLS = True
MAIL_USERNAME = 'your-email@gmail.com'
MAIL_PASSWORD = 'app-password'
```

---

## IMPLEMENTATION PRIORITY

```
WEEK 1: Shopping Cart + Checkout UI
WEEK 2: Order History + Tracking
WEEK 3: Product Search + Filters
WEEK 4: Reviews & Wishlist
WEEK 5: Email Notifications
```

---

## TOTAL EFFORT
- **Hours:** ~80–100 hours
- **Team:** 1–2 developers
- **Timeline:** 2–4 weeks

After these, you'll have a **functional e-commerce platform**. Then tackle the bigger refactor (database migration, payment gateway, etc.).

---

**Start with #1 (Shopping Cart) this week—it's the highest impact and unblocks everything else!**
