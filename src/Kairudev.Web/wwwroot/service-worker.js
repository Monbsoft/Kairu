// Dev service worker — pass-through, no caching
// In development, always fetch from network
self.addEventListener('fetch', () => {});
