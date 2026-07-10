// ==================== MR SHOP - SHARED CART MODULE ====================
// Cart with API sync and localStorage persistence

const MR_Cart = {
  API_BASE: window.location.protocol === 'https:' ? 'https://localhost:5000/api' : 'http://localhost:5000/api',

  getCart() {
    try {
      return JSON.parse(localStorage.getItem('mr_shop_cart')) || [];
    } catch {
      return [];
    }
  },

  saveCart(cart) {
    localStorage.setItem('mr_shop_cart', JSON.stringify(cart));
    this.updateCartCount();
    this.updateCartBadge();
  },

  // Sync cart from server
  async syncFromServer() {
    if (!MR_API.isLoggedIn()) return;

    try {
      const result = await MR_API.get('/cart');
      if (result && result.ok) {
        const serverCart = result.data.map(item => ({
          id: item.product.id,
          name: item.product.name,
          price: item.product.price,
          originalPrice: item.product.originalPrice,
          image: item.product.image,
          category: item.product.category,
          quantity: item.quantity,
          stock: item.product.stock,
          cartItemId: item.id
        }));
        this.saveCart(serverCart);
        console.log('Cart synced from server:', serverCart.length, 'items');
      }
    } catch (err) {
      console.log('Failed to sync cart from server');
    }
  },

  async addItem(productId, quantity = 1) {
    const product = MR_getProductById(productId);
    if (!product) return false;

    // Update localStorage immediately
    const cart = this.getCart();
    const existing = cart.find(item => item.id === productId);

    if (existing) {
      existing.quantity += quantity;
    } else {
      cart.push({
        id: product.id,
        name: product.name,
        price: product.price,
        originalPrice: product.originalPrice,
        image: product.image,
        category: product.category,
        quantity: quantity,
        stock: product.stock
      });
    }

    this.saveCart(cart);
    this.showToast(`${product.name} added to cart!`);

    // Sync to server if logged in
    if (MR_API.isLoggedIn()) {
      try {
        await MR_API.post('/cart', { productId, quantity });
      } catch (err) {
        console.log('Failed to sync cart to server');
      }
    }

    return true;
  },

  async removeItem(productId) {
    let cart = this.getCart();
    const item = cart.find(i => i.id === productId);
    cart = cart.filter(item => item.id !== productId);
    this.saveCart(cart);

    // Sync to server if logged in
    if (MR_API.isLoggedIn() && item && item.cartItemId) {
      try {
        await MR_API.delete(`/cart/${item.cartItemId}`);
      } catch (err) {
        console.log('Failed to sync cart removal to server');
      }
    }
  },

  async updateQuantity(productId, quantity) {
    const cart = this.getCart();
    const item = cart.find(i => i.id === productId);
    if (item) {
      if (quantity <= 0) {
        await this.removeItem(productId);
      } else {
        item.quantity = quantity;
        this.saveCart(cart);

        // Sync to server if logged in
        if (MR_API.isLoggedIn() && item.cartItemId) {
          try {
            await MR_API.put(`/cart/${item.cartItemId}`, { quantity });
          } catch (err) {
            console.log('Failed to sync cart quantity to server');
          }
        }
      }
    }
  },

  getTotalItems() {
    return this.getCart().reduce((sum, item) => sum + item.quantity, 0);
  },

  getSubtotal() {
    return this.getCart().reduce((sum, item) => sum + (item.price * item.quantity), 0);
  },

  getTax() {
    return Math.round(this.getSubtotal() * 0.05);
  },

  getShipping() {
    return this.getSubtotal() > 5000 ? 0 : 150;
  },

  getTotal() {
    return this.getSubtotal() + this.getTax() + this.getShipping();
  },

  async clearCart() {
    localStorage.removeItem('mr_shop_cart');
    this.updateCartCount();
    this.updateCartBadge();

    // Sync to server if logged in
    if (MR_API.isLoggedIn()) {
      try {
        await MR_API.delete('/cart');
      } catch (err) {
        console.log('Failed to sync cart clear to server');
      }
    }
  },

  updateCartCount() {
    const count = this.getTotalItems();
    document.querySelectorAll('.amz-cart-count').forEach(el => {
      el.textContent = count;
    });
  },

  updateCartBadge() {
    const badge = document.querySelector('.amz-cart-count');
    if (badge) {
      badge.style.transform = 'scale(1.3)';
      setTimeout(() => badge.style.transform = 'scale(1)', 200);
    }
  },

  showToast(message, type = 'success') {
    const existing = document.querySelector('.mr-toast');
    if (existing) existing.remove();

    const toast = document.createElement('div');
    toast.className = 'mr-toast';
    toast.style.cssText = `
      position: fixed;
      bottom: 20px;
      left: 50%;
      transform: translateX(-50%) translateY(100px);
      background: ${type === 'success' ? '#007600' : type === 'error' ? '#cc0c39' : '#131921'};
      color: white;
      padding: 12px 24px;
      border-radius: 8px;
      font-size: 14px;
      font-weight: 600;
      z-index: 9999;
      box-shadow: 0 4px 12px rgba(0,0,0,0.3);
      transition: transform 0.3s ease;
      max-width: 90vw;
      text-align: center;
    `;
    toast.textContent = message;
    document.body.appendChild(toast);

    setTimeout(() => {
      toast.style.transform = 'translateX(-50%) translateY(0)';
    }, 10);

    setTimeout(() => {
      toast.style.transform = 'translateX(-50%) translateY(100px)';
      setTimeout(() => toast.remove(), 300);
    }, 2500);
  }
};

function MR_addToCart(productId, quantity = 1) {
  MR_Cart.addItem(productId, quantity);
}

document.addEventListener('DOMContentLoaded', () => {
  MR_Cart.updateCartCount();
  // Sync cart from server if logged in
  if (MR_API.isLoggedIn()) {
    MR_Cart.syncFromServer();
  }
});
