// ==================== MR SHOP - WISHLIST MODULE ====================
// Wishlist with API sync and localStorage persistence

const MR_Wishlist = {
  getWishlist() {
    try {
      return JSON.parse(localStorage.getItem('mr_shop_wishlist')) || [];
    } catch {
      return [];
    }
  },

  saveWishlist(wishlist) {
    try {
      localStorage.setItem('mr_shop_wishlist', JSON.stringify(wishlist));
    } catch (e) {
      MR_Cart.showToast('Storage full. Please clear some data.', 'error');
      return;
    }
    this.updateWishlistCount();
  },

  // Sync wishlist from server
  async syncFromServer() {
    if (!MR_API.isLoggedIn()) return;

    try {
      const result = await MR_API.get('/wishlist');
      if (result && result.ok) {
        const serverWishlist = result.data.map(item => ({
          id: item.product.id,
          name: item.product.name,
          price: item.product.price,
          originalPrice: item.product.originalPrice,
          image: item.product.image,
          category: item.product.category,
          rating: item.product.rating,
          reviews: item.product.reviews,
          addedAt: item.addedAt,
          wishlistItemId: item.id
        }));
        this.saveWishlist(serverWishlist);
        console.log('Wishlist synced from server:', serverWishlist.length, 'items');
      }
    } catch (err) {
      console.log('Failed to sync wishlist from server');
    }
  },

  async toggle(productId) {
    const product = MR_getProductById(productId);
    if (!product) return false;

    const wishlist = this.getWishlist();
    const index = wishlist.findIndex(item => item.id === productId);

    if (index > -1) {
      // Remove from wishlist
      wishlist.splice(index, 1);
      MR_Cart.showToast('Removed from wishlist', 'info');

      // Sync to server if logged in
      if (MR_API.isLoggedIn()) {
        try {
          await MR_API.delete(`/wishlist/product/${productId}`);
        } catch (err) {
          console.log('Failed to sync wishlist removal to server');
        }
      }
    } else {
      // Add to wishlist
      wishlist.push({
        id: product.id,
        name: product.name,
        price: product.price,
        originalPrice: product.originalPrice,
        image: product.image,
        category: product.category,
        rating: product.rating,
        reviews: product.reviews,
        addedAt: new Date().toISOString()
      });
      MR_Cart.showToast('Added to wishlist!', 'success');

      // Sync to server if logged in
      if (MR_API.isLoggedIn()) {
        try {
          await MR_API.post('/wishlist', { productId });
        } catch (err) {
          console.log('Failed to sync wishlist to server');
        }
      }
    }

    this.saveWishlist(wishlist);
    return index === -1;
  },

  isInWishlist(productId) {
    return this.getWishlist().some(item => item.id === productId);
  },

  async remove(productId) {
    let wishlist = this.getWishlist();
    wishlist = wishlist.filter(item => item.id !== productId);
    this.saveWishlist(wishlist);

    // Sync to server if logged in
    if (MR_API.isLoggedIn()) {
      try {
        await MR_API.delete(`/wishlist/product/${productId}`);
      } catch (err) {
        console.log('Failed to sync wishlist removal to server');
      }
    }
  },

  async clear() {
    localStorage.removeItem('mr_shop_wishlist');
    this.updateWishlistCount();

    // Sync to server if logged in
    if (MR_API.isLoggedIn()) {
      try {
        await MR_API.delete('/wishlist');
      } catch (err) {
        console.log('Failed to sync wishlist clear to server');
      }
    }
  },

  getCount() {
    return this.getWishlist().length;
  },

  updateWishlistCount() {
    const count = this.getCount();
    document.querySelectorAll('.amz-wishlist-count').forEach(el => {
      el.textContent = count;
    });
  }
};

function MR_toggleWishlist(productId) {
  MR_Wishlist.toggle(productId);
}

document.addEventListener('DOMContentLoaded', () => {
  MR_Wishlist.updateWishlistCount();
  // Sync wishlist from server if logged in
  if (MR_API.isLoggedIn()) {
    MR_Wishlist.syncFromServer();
  }
});
