// ==================== MR SHOP - SHARED CART MODULE ====================
// API-first cart - localStorage only for offline caching

const MR_Cart = {
  API_BASE: window.location.hostname === 'localhost' ? 'http://localhost:5000/api' : 'https://mrshop-bd.azurewebsites.net/api',

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
  },

  async syncFromServer() {
    if (!MR_API.isLoggedIn()) return;
    try {
      const result = await MR_API.get('/cart');
      if (result && result.ok && result.data.items) {
        const serverCart = result.data.items.map(item => ({
          id: item.productId,
          name: item.productName,
          price: item.price,
          image: item.image,
          quantity: item.quantity,
          cartItemId: item.id
        }));
        this.saveCart(serverCart);
      }
    } catch (err) {
      console.log('Cart sync failed');
    }
  },

  async addItem(productId, quantity = 1) {
    if (MR_API.isLoggedIn()) {
      try {
        const result = await MR_API.post('/cart', { productId, quantity });
        if (result && result.ok) {
          await this.syncFromServer();
          MR_Cart.showToast('Added to cart!');
          return true;
        }
      } catch (err) {}
    }

    // Offline fallback - localStorage only
    const cart = this.getCart();
    const existing = cart.find(item => item.id === productId);
    if (existing) {
      existing.quantity += quantity;
    } else {
      cart.push({ id: productId, quantity });
    }
    this.saveCart(cart);
    MR_Cart.showToast('Added to cart (offline mode)');
    return true;
  },

  async removeItem(productId) {
    if (MR_API.isLoggedIn()) {
      const cart = this.getCart();
      const item = cart.find(i => i.id === productId);
      if (item && item.cartItemId) {
        try { await MR_API.delete(`/cart/${item.cartItemId}`); } catch(e) {}
      }
    }
    let cart = this.getCart();
    cart = cart.filter(item => item.id !== productId);
    this.saveCart(cart);
  },

  async updateQuantity(productId, quantity) {
    if (quantity <= 0) return this.removeItem(productId);

    if (MR_API.isLoggedIn()) {
      const cart = this.getCart();
      const item = cart.find(i => i.id === productId);
      if (item && item.cartItemId) {
        try { await MR_API.put(`/cart/${item.cartItemId}`, { quantity }); } catch(e) {}
      }
    }
    const cart = this.getCart();
    const item = cart.find(i => i.id === productId);
    if (item) {
      item.quantity = quantity;
      this.saveCart(cart);
    }
  },

  getTotalItems() {
    return this.getCart().reduce((sum, item) => sum + item.quantity, 0);
  },

  getSubtotal() {
    return this.getCart().reduce((sum, item) => sum + ((item.price || 0) * item.quantity), 0);
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
    if (MR_API.isLoggedIn()) {
      try { await MR_API.delete('/cart'); } catch(e) {}
    }
    localStorage.removeItem('mr_shop_cart');
    this.updateCartCount();
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
      position: fixed; bottom: 20px; left: 50%; transform: translateX(-50%) translateY(100px);
      background: ${type === 'success' ? '#007600' : type === 'error' ? '#cc0c39' : '#131921'};
      color: white; padding: 12px 24px; border-radius: 8px; font-size: 14px; font-weight: 600;
      z-index: 9999; box-shadow: 0 4px 12px rgba(0,0,0,0.3); transition: transform 0.3s ease;
      max-width: 90vw; text-align: center;
    `;
    toast.textContent = message;
    document.body.appendChild(toast);
    setTimeout(() => toast.style.transform = 'translateX(-50%) translateY(0)', 10);
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
  if (MR_API.isLoggedIn()) {
    MR_Cart.syncFromServer();
  }
});
