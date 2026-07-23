// Service Worker for MR Shop PWA
const CACHE_NAME = 'mrshop-v2';
const urlsToCache = [
  './',
  './index.html',
  './assets/css/amazon-style.css',
  './assets/css/dark-mode.css',
  './assets/js/loader.js',
  './assets/js/i18n.js',
  './assets/js/darkmode.js',
  './assets/images/mrlogo.png',
  './signin.html',
  './signup.html',
  './cart.html',
  './checkout.html',
  './search-results.html',
  './review.html',
  './order-tracking.html',
  './payment.html',
  './compare.html',
  './blog.html',
  './email-notifications.html',
  './seller.html',
  './admin.html'
];

// Install event
self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => cache.addAll(urlsToCache))
      .then(() => self.skipWaiting())
  );
});

// Activate event
self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames.filter(name => name !== CACHE_NAME)
          .map(name => caches.delete(name))
      );
    }).then(() => self.clients.claim())
  );
});

// Fetch event - Network first, fallback to cache (skip API calls)
self.addEventListener('fetch', event => {
  const url = new URL(event.request.url);

  // Skip API calls - let them pass through directly
  if (url.pathname.startsWith('/api/') || url.hostname.includes('azurewebsites.net') || url.hostname.includes('googleapis.com') || url.hostname.includes('google.com')) {
    return;
  }

  event.respondWith(
    fetch(event.request)
      .then(response => {
        const responseClone = response.clone();
        caches.open(CACHE_NAME).then(cache => {
          cache.put(event.request, responseClone);
        });
        return response;
      })
      .catch(() => caches.match(event.request))
  );
});

// Background sync for offline orders
self.addEventListener('sync', event => {
  if (event.tag === 'sync-orders') {
    event.waitUntil(syncOrders());
  }
});

async function getPendingOrders() {
  // Get pending orders from localStorage via client
  const clients = await self.clients.matchAll();
  if (clients.length > 0) {
    return new Promise((resolve) => {
      const channel = new MessageChannel();
      channel.port1.onmessage = (e) => resolve(e.data);
      clients[0].postMessage({ type: 'getPendingOrders' }, [channel.port2]);
      setTimeout(() => resolve([]), 1000);
    });
  }
  return [];
}

async function removePendingOrder(orderId) {
  const clients = await self.clients.matchAll();
  if (clients.length > 0) {
    clients[0].postMessage({ type: 'removePendingOrder', orderId });
  }
}

async function syncOrders() {
  // Sync pending orders when back online
  const pendingOrders = await getPendingOrders();
  for (const order of pendingOrders) {
    try {
      await fetch('./api/orders', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(order)
      });
      await removePendingOrder(order.id);
    } catch (e) {
      console.log('Sync failed, will retry');
    }
  }
}

// Push notifications
self.addEventListener('push', event => {
  const data = event.data ? event.data.json() : {};
  const options = {
    body: data.body || 'You have a new notification from MR Shop',
    icon: './assets/images/mrlogo.png',
    badge: './assets/images/mrlogo.png',
    vibrate: [200, 100, 200],
    data: { url: data.url || '/index.html' },
    actions: [
      { action: 'open', title: 'Open', icon: './assets/images/mrlogo.png' },
      { action: 'close', title: 'Close' }
    ]
  };
  event.waitUntil(
    self.registration.showNotification(data.title || 'MR Shop', options)
  );
});

self.addEventListener('notificationclick', event => {
  event.notification.close();
  if (event.action === 'open' || !event.action) {
    event.waitUntil(
      clients.openWindow(event.notification.data.url)
    );
  }
});
