(function () {
  const CART_STORAGE_KEY = 'mr_shop_cart';
  const LEGACY_CART_STORAGE_KEY = 'mrshop_cart';
  const ORDERS_STORAGE_KEY = 'mr_shop_orders';
  const PENDING_ORDER_KEY = 'mr_shop_pending_order';

  const COUPONS = {
    WELCOME10: { type: 'percent', value: 10, label: '10% discount applied' },
    FREESHIP: { type: 'shipping', value: 0, label: 'Free shipping applied' },
    MRSHOP50: { type: 'fixed', value: 50, label: '৳50 discount applied' }
  };

  const state = {
    cart: [],
    coupon: null
  };

  function $(selector) {
    return document.querySelector(selector);
  }

  function escapeHtml(value) {
    return String(value ?? '')
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  function formatCurrency(value) {
    const amount = Number(value || 0);
    return `৳${amount.toLocaleString('en-BD', { maximumFractionDigits: 0 })}`;
  }

  function parseArray(rawValue) {
    try {
      const parsed = JSON.parse(rawValue);
      return Array.isArray(parsed) ? parsed : [];
    } catch (error) {
      return [];
    }
  }

  function readCartStorage() {
    const currentCart = parseArray(localStorage.getItem(CART_STORAGE_KEY));
    if (currentCart.length > 0) {
      return currentCart;
    }

    const legacyCart = parseArray(localStorage.getItem(LEGACY_CART_STORAGE_KEY));
    if (legacyCart.length > 0) {
      writeCartStorage(legacyCart);
      return legacyCart;
    }

    return currentCart;
  }

  function writeCartStorage(cart) {
    const serializedCart = JSON.stringify(cart);
    localStorage.setItem(CART_STORAGE_KEY, serializedCart);
    localStorage.setItem(LEGACY_CART_STORAGE_KEY, serializedCart);
  }

  function normalizeImageUrl(item) {
    const value = String(item?.image_path || item?.image || item?.imageBase64 || '').trim();

    if (!value) {
      return '../images/mrlogo.png';
    }

    if (value.startsWith('http://') || value.startsWith('https://') || value.startsWith('data:') || value.startsWith('/')) {
      return value;
    }

    const cleanPath = value
      .replace(/^\.\.\/images\//, '')
      .replace(/^\.\//, '')
      .replace(/^assets\/images\//, '')
      .replace(/^images\//, '');

    return `../images/${cleanPath}`;
  }

  function normalizeCartItem(item) {
    const quantity = Math.max(1, Number(item?.quantity || 1));
    const unitPriceSource = item?.final_price ?? item?.price ?? item?.originalPrice ?? 0;
    const unitPrice = Number(unitPriceSource) || 0;

    return {
      id: item?.id,
      name: item?.name || 'Untitled product',
      price: unitPrice,
      originalPrice: Number(item?.originalPrice ?? item?.price ?? unitPrice) || unitPrice,
      category: item?.category || 'General',
      seller: item?.seller || 'MR Shop',
      image: normalizeImageUrl(item),
      description: item?.description || '',
      quantity
    };
  }

  function formatItemMeta(item) {
    const pieces = [item.category, item.seller].filter(Boolean);
    return pieces.join(' · ');
  }

  function getCart() {
    return readCartStorage().map(normalizeCartItem);
  }

  function saveCart(cart) {
    state.cart = cart.map(normalizeCartItem);
    writeCartStorage(state.cart);
    updateCartBadge();
    renderCart();
  }

  function showEmptyState(isVisible) {
    const emptyState = $('#emptyState');
    if (!emptyState) {
      return;
    }

    emptyState.classList.toggle('hidden', !isVisible);
    emptyState.classList.toggle('is-visible', isVisible);
  }

  function updateCartBadge() {
    const badge = $('#cartBadge');
    if (!badge) {
      return;
    }

    const count = state.cart.reduce((total, item) => total + Math.max(1, Number(item.quantity || 1)), 0);
    badge.textContent = `${count} item${count === 1 ? '' : 's'}`;
  }

  function calculateSubtotal() {
    return state.cart.reduce((total, item) => total + (Number(item.price || 0) * Math.max(1, Number(item.quantity || 1))), 0);
  }

  function getDeliveryFee() {
    const deliveryMethod = $('#deliveryMethod')?.value || 'standard';
    const subtotal = calculateSubtotal();

    if (state.coupon?.type === 'shipping') {
      return 0;
    }

    if (subtotal <= 0) {
      return 0;
    }

    if (deliveryMethod === 'express') {
      return 120;
    }

    if (deliveryMethod === 'same_day') {
      return 180;
    }

    return 60;
  }

  function getDiscountAmount(subtotal) {
    if (!state.coupon) {
      return 0;
    }

    if (state.coupon.type === 'percent') {
      return Math.round((subtotal * state.coupon.value) / 100);
    }

    if (state.coupon.type === 'fixed') {
      return Math.min(subtotal, Number(state.coupon.value || 0));
    }

    return 0;
  }

  function renderCartItem(item) {
    const lineTotal = Number(item.price || 0) * Math.max(1, Number(item.quantity || 1));
    const originalLineTotal = Number(item.originalPrice || item.price || 0) * Math.max(1, Number(item.quantity || 1));
    const hasDiscount = originalLineTotal > lineTotal;

    return `
      <article class="cart-item" data-cart-id="${escapeHtml(item.id)}">
        <img class="item-image" src="${escapeHtml(item.image)}" alt="${escapeHtml(item.name)}" onerror="this.src='../images/mrlogo.png'" />
        <div class="item-info">
          <h3>${escapeHtml(item.name)}</h3>
          <div class="item-meta">
            <span>${escapeHtml(formatItemMeta(item))}</span>
            ${item.description ? `<span>${escapeHtml(item.description)}</span>` : ''}
          </div>
          <div class="item-pricing">
            <strong class="unit-price">${formatCurrency(item.price)}</strong>
            ${hasDiscount ? `<span class="line-total">Old total ${formatCurrency(originalLineTotal)}</span>` : ''}
          </div>
          <div class="item-actions">
            <div class="quantity-control" aria-label="Quantity controls">
              <button type="button" data-action="decrease" data-cart-id="${escapeHtml(item.id)}" aria-label="Decrease quantity">−</button>
              <input type="text" value="${escapeHtml(item.quantity)}" readonly aria-label="Quantity" />
              <button type="button" data-action="increase" data-cart-id="${escapeHtml(item.id)}" aria-label="Increase quantity">+</button>
            </div>
            <button type="button" class="remove-btn" data-action="remove" data-cart-id="${escapeHtml(item.id)}">Remove</button>
          </div>
        </div>
        <div class="item-total">
          <strong>${formatCurrency(lineTotal)}</strong>
        </div>
      </article>
    `;
  }

  function renderCart() {
    const itemsContainer = $('#cartItems');
    const subtotal = calculateSubtotal();
    const discount = getDiscountAmount(subtotal);
    const shipping = getDeliveryFee();
    const total = Math.max(0, subtotal - discount + shipping);

    if (itemsContainer) {
      if (state.cart.length === 0) {
        itemsContainer.innerHTML = '';
        showEmptyState(true);
      } else {
        itemsContainer.innerHTML = state.cart.map(renderCartItem).join('');
        showEmptyState(false);
      }
    }

    const subtotalValue = $('#subtotalValue');
    const discountValue = $('#discountValue');
    const shippingValue = $('#shippingValue');
    const totalValue = $('#totalValue');
    const couponMessage = $('#couponMessage');

    if (subtotalValue) subtotalValue.textContent = formatCurrency(subtotal);
    if (discountValue) discountValue.textContent = formatCurrency(discount);
    if (shippingValue) shippingValue.textContent = formatCurrency(shipping);
    if (totalValue) totalValue.textContent = formatCurrency(total);

    if (couponMessage) {
      couponMessage.textContent = state.coupon
        ? `${state.coupon.code} applied. ${state.coupon.label}`
        : 'Try WELCOME10, FREESHIP, or MRSHOP50.';
    }

    updateCartBadge();
    bindItemButtons();
  }

  function changeQuantity(cartId, delta) {
    const nextCart = state.cart
      .map(item => {
        if (String(item.id) !== String(cartId)) {
          return item;
        }

        return {
          ...item,
          quantity: Math.max(1, Number(item.quantity || 1) + delta)
        };
      })
      .filter(Boolean);

    saveCart(nextCart);
  }

  function removeItem(cartId) {
    const nextCart = state.cart.filter(item => String(item.id) !== String(cartId));
    saveCart(nextCart);
  }

  function clearCart() {
    state.coupon = null;
    const couponInput = $('#couponInput');
    const couponMessage = $('#couponMessage');

    if (couponInput) {
      couponInput.value = '';
    }

    if (couponMessage) {
      couponMessage.textContent = 'Try WELCOME10, FREESHIP, or MRSHOP50.';
    }

    saveCart([]);
  }

  function bindItemButtons() {
    document.querySelectorAll('[data-action]').forEach(button => {
      if (button.dataset.bound === 'true') {
        return;
      }

      button.dataset.bound = 'true';
      button.addEventListener('click', () => {
        const action = button.getAttribute('data-action');
        const cartId = button.getAttribute('data-cart-id');

        if (!cartId) {
          return;
        }

        if (action === 'increase') {
          changeQuantity(cartId, 1);
        } else if (action === 'decrease') {
          changeQuantity(cartId, -1);
        } else if (action === 'remove') {
          removeItem(cartId);
        }
      });
    });
  }

  function setOrderSuccess(message, isError = false) {
    const banner = $('#orderSuccess');
    if (!banner) {
      return;
    }

    banner.textContent = message;
    banner.classList.remove('hidden');
    banner.style.color = isError ? '#b42318' : '#067647';
    banner.style.background = isError
      ? 'linear-gradient(135deg, rgba(239, 68, 68, 0.12), rgba(245, 158, 11, 0.10))'
      : 'linear-gradient(135deg, rgba(16, 185, 129, 0.16), rgba(6, 182, 212, 0.14))';
  }

  function getOrders() {
    return parseArray(localStorage.getItem(ORDERS_STORAGE_KEY));
  }

  function saveOrders(orders) {
    localStorage.setItem(ORDERS_STORAGE_KEY, JSON.stringify(orders));
  }

  function createOrderId() {
    const timestamp = Date.now().toString(36).toUpperCase();
    return `MR-${timestamp}`;
  }

  function applyCoupon() {
    const couponInput = $('#couponInput');
    const couponMessage = $('#couponMessage');
    const code = String(couponInput?.value || '').trim().toUpperCase();

    if (!code) {
      state.coupon = null;
      if (couponMessage) {
        couponMessage.textContent = 'Enter a promo code to apply a discount.';
      }
      renderCart();
      return;
    }

    const coupon = COUPONS[code];
    if (!coupon) {
      state.coupon = null;
      if (couponMessage) {
        couponMessage.textContent = 'Invalid code. Try WELCOME10, FREESHIP, or MRSHOP50.';
      }
      renderCart();
      return;
    }

    state.coupon = { code, ...coupon };
    if (couponMessage) {
      couponMessage.textContent = `${code} applied. ${coupon.label}`;
    }
    renderCart();
  }

  function placeOrderDraft() {
    if (state.cart.length === 0) {
      setOrderSuccess('Add items to the cart before placing an order.', true);
      return;
    }

    const customerName = String($('#customerName')?.value || '').trim();
    const customerPhone = String($('#customerPhone')?.value || '').trim();
    const deliveryAddress = String($('#deliveryAddress')?.value || '').trim();
    const paymentMethod = $('#paymentMethod')?.value || 'cash_on_delivery';
    const deliveryMethod = $('#deliveryMethod')?.value || 'standard';

    if (!customerName || !customerPhone || !deliveryAddress) {
      setOrderSuccess('Please fill in name, phone, and delivery address.', true);
      return;
    }

    const subtotal = calculateSubtotal();
    const discount = getDiscountAmount(subtotal);
    const shipping = getDeliveryFee();
    const total = Math.max(0, subtotal - discount + shipping);

    const order = {
      orderId: createOrderId(),
      createdAt: new Date().toISOString(),
      customer: {
        name: customerName,
        phone: customerPhone,
        address: deliveryAddress
      },
      paymentMethod,
      deliveryMethod,
      couponCode: state.coupon?.code || null,
      items: state.cart,
      subtotal,
      discount,
      shipping,
      total,
      status: 'draft'
    };

    const orders = getOrders();
    orders.unshift(order);
    saveOrders(orders);
    localStorage.setItem(PENDING_ORDER_KEY, JSON.stringify(order));

    clearCart();
    setOrderSuccess(`Order draft saved successfully. Your order id is ${order.orderId}.`);
  }

  function bindControls() {
    $('#clearCartTopBtn')?.addEventListener('click', () => {
      if (state.cart.length === 0) {
        setOrderSuccess('Your cart is already empty.', true);
        return;
      }

      clearCart();
      setOrderSuccess('Cart cleared successfully.');
    });

    $('#applyCouponBtn')?.addEventListener('click', applyCoupon);
    $('#couponInput')?.addEventListener('keydown', event => {
      if (event.key === 'Enter') {
        event.preventDefault();
        applyCoupon();
      }
    });

    $('#deliveryMethod')?.addEventListener('change', renderCart);
    $('#placeOrderBtn')?.addEventListener('click', placeOrderDraft);
  }

  function init() {
    state.cart = getCart();
    bindControls();
    renderCart();
  }

  document.addEventListener('DOMContentLoaded', init);
})();