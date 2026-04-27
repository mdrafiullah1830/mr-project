(function (window) {
  const CART_KEY = 'mr_shop_cart';
  const LEGACY_CART_KEY = 'mrshop_cart';
  const USER_KEY = 'mr_shop_user';
  const PENDING_CART_KEY = 'mrshop_pending_cart_action';
  const RETURN_URL_KEY = 'mrshop_auth_return_url';

  function safeParse(rawValue, fallbackValue) {
    if (!rawValue) return fallbackValue;
    try { return JSON.parse(rawValue); } catch (error) { return fallbackValue; }
  }

  function getCurrentUser() {
    return safeParse(localStorage.getItem(USER_KEY), null);
  }

  function isAuthenticated() {
    return Boolean(getCurrentUser());
  }

  function normalizeProduct(product) {
    if (!product) return null;
    return {
      id: product.id,
      name: product.name || 'Untitled product',
      price: Number(product.final_price ?? product.price ?? 0) || 0,
      originalPrice: Number(product.originalPrice ?? product.price ?? product.final_price ?? 0) || 0,
      category: product.category || '',
      image: product.image || product.image_path || product.imageBase64 || product.img || '',
      seller: product.seller || product.shop_name || 'MR Shop',
      description: product.description || product.desc || '',
      quantity: Math.max(1, Number(product.quantity || product.qty || 1))
    };
  }

  function getCartItems() {
    const currentItems = safeParse(localStorage.getItem(CART_KEY), []);
    if (Array.isArray(currentItems) && currentItems.length > 0) {
      return currentItems;
    }

    const legacyItems = safeParse(localStorage.getItem(LEGACY_CART_KEY), []);
    if (Array.isArray(legacyItems) && legacyItems.length > 0) {
      setCartItems(legacyItems);
      return legacyItems;
    }

    return Array.isArray(currentItems) ? currentItems : [];
  }

  function setCartItems(items) {
    const serializedItems = JSON.stringify(Array.isArray(items) ? items : []);
    localStorage.setItem(CART_KEY, serializedItems);
    localStorage.setItem(LEGACY_CART_KEY, serializedItems);
    syncCartBadges();
  }

  function syncCartBadges() {
    const cartCount = getCartItems().reduce((total, item) => total + Math.max(1, Number(item.quantity || 1)), 0);
    document.querySelectorAll('.js-cart-count, .cart-count, [data-cart-count]').forEach(element => {
      element.textContent = String(cartCount);
    });
    return cartCount;
  }

  function sanitizeReturnUrl(returnUrl, defaultUrl) {
    const value = String(returnUrl || '').trim();
    if (!value || value.includes('auth.html')) {
      return defaultUrl;
    }
    return value;
  }

  function storePendingCartAction(product, quantity, returnUrl) {
    const normalizedProduct = normalizeProduct(product);
    if (!normalizedProduct) {
      return null;
    }

    const pendingAction = {
      type: 'add-to-cart',
      product: normalizedProduct,
      quantity: Math.max(1, Number(quantity || 1)),
      returnUrl: sanitizeReturnUrl(returnUrl || window.location.href, window.location.href)
    };

    sessionStorage.setItem(PENDING_CART_KEY, JSON.stringify(pendingAction));
    sessionStorage.setItem(RETURN_URL_KEY, pendingAction.returnUrl);
    return pendingAction;
  }

  function consumePendingCartAction() {
    const pendingAction = safeParse(sessionStorage.getItem(PENDING_CART_KEY), null);
    sessionStorage.removeItem(PENDING_CART_KEY);

    if (!pendingAction || pendingAction.type !== 'add-to-cart' || !pendingAction.product) {
      return null;
    }

    return pendingAction;
  }

  function consumeReturnUrl(defaultUrl) {
    const storedUrl = sessionStorage.getItem(RETURN_URL_KEY);
    sessionStorage.removeItem(RETURN_URL_KEY);
    return sanitizeReturnUrl(storedUrl, defaultUrl);
  }

  function appendToCart(product, quantity) {
    const normalizedProduct = normalizeProduct(product);
    if (!normalizedProduct) {
      return false;
    }

    const cartItems = getCartItems();
    const existingItem = cartItems.find(item => String(item.id) === String(normalizedProduct.id));

    if (existingItem) {
      existingItem.quantity = Math.max(1, Number(existingItem.quantity || 1) + Math.max(1, Number(quantity || 1)));
    } else {
      cartItems.push({
        id: normalizedProduct.id,
        name: normalizedProduct.name,
        price: normalizedProduct.price,
        originalPrice: normalizedProduct.originalPrice,
        category: normalizedProduct.category,
        image: normalizedProduct.image,
        seller: normalizedProduct.seller,
        description: normalizedProduct.description,
        quantity: Math.max(1, Number(quantity || 1))
      });
    }

    setCartItems(cartItems);
    return true;
  }

  function addToCart(product, options = {}) {
    const quantity = Math.max(1, Number(options.quantity || 1));

    if (!isAuthenticated()) {
      storePendingCartAction(product, quantity, options.returnUrl || window.location.href);
      window.location.href = options.authUrl || 'auth.html#login';
      return false;
    }

    return appendToCart(product, quantity);
  }

  function finalizeAuthenticatedSession(defaultRedirect = 'userprofile.html') {
    const pendingAction = consumePendingCartAction();

    if (pendingAction) {
      appendToCart(pendingAction.product, pendingAction.quantity || 1);
    }

    syncCartBadges();

    return {
      pendingAction,
      redirectUrl: consumeReturnUrl(defaultRedirect)
    };
  }

  window.MRShopCartFlow = {
    getCurrentUser,
    isAuthenticated,
    normalizeProduct,
    getCartItems,
    setCartItems,
    syncCartBadges,
    storePendingCartAction,
    consumePendingCartAction,
    consumeReturnUrl,
    appendToCart,
    addToCart,
    finalizeAuthenticatedSession
  };

  window.addEventListener('storage', (event) => {
    if (event.key === CART_KEY || event.key === LEGACY_CART_KEY) {
      syncCartBadges();
    }
  });

  document.addEventListener('DOMContentLoaded', syncCartBadges);
})(window);