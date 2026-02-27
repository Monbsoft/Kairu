// Service Worker registration for Kairudev PWA
// Skips silently if the browser does not support service workers.
if ('serviceWorker' in navigator) {
    window.addEventListener('load', function () {
        navigator.serviceWorker.register('service-worker.js')
            .catch(function (err) {
                console.warn('[SW] Registration failed:', err);
            });
    });
}
