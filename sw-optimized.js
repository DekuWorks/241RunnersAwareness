/**
 * ============================================
 * 241 RUNNERS AWARENESS - OPTIMIZED SERVICE WORKER
 * ============================================
 * 
 * This service worker provides offline functionality, caching,
 * and automatic update notifications for improved performance.
 */

const CACHE_NAME = '241runners-v1.4.3';
const STATIC_CACHE = 'static-v1.4.3';
const DYNAMIC_CACHE = 'dynamic-v1.4.3';

// Files to cache immediately
const STATIC_FILES = [
    '/',
    '/index.html',
    '/styles.css',
    '/forms.css',
    '/auth-utils.js',
    '/js/auth.js',
    '/js/cases.js',
    '/js/public-cases.js',
    '/js/my-cases.js',
    '/js/admin-auth.js',
    '/js/update-banner.js',
    '/assets/js/config.js',
    '/assets/js/api-utils.js',
    '/assets/images/241-logo.jpg',
    '/manifest.json',
    '/version.json'
];

// Auth endpoints that should NEVER be cached
const AUTH_ENDPOINTS = [
    '/api/auth/',
    '/api/Auth/',
    '/admin/login.html'
    // Removed /admin/admindash.html to prevent service worker interference with SignalR
];

// Install event - cache static files and skip waiting
self.addEventListener('install', (event) => {
    console.log('Service Worker installing...');
    
    event.waitUntil(
        caches.open(STATIC_CACHE)
            .then(cache => {
                console.log('Caching static files');
                return cache.addAll(STATIC_FILES);
            })
            .then(() => {
                // Skip waiting to activate immediately
                return self.skipWaiting();
            })
            .catch(error => {
                console.error('Failed to cache static files:', error);
            })
    );
});

// Activate event - clean up old caches and claim clients
self.addEventListener('activate', (event) => {
    console.log('Service Worker activating...');
    
    event.waitUntil(
        Promise.all([
            // Clean up old caches
            caches.keys().then(cacheNames => {
                return Promise.all(
                    cacheNames.map(cacheName => {
                        if (cacheName !== STATIC_CACHE && cacheName !== DYNAMIC_CACHE) {
                            console.log('Deleting old cache:', cacheName);
                            return caches.delete(cacheName);
                        }
                    })
                );
            }),
            // Claim all clients immediately
            self.clients.claim()
        ]).then(() => {
            // Notify all clients about the update
            return self.clients.matchAll().then(clients => {
                clients.forEach(client => {
                    client.postMessage({
                        type: 'SW_UPDATE_AVAILABLE',
                        message: 'A new version is available'
                    });
                });
            });
        })
    );
});

// Check if URL is an auth endpoint
function isAuthEndpoint(url) {
    return AUTH_ENDPOINTS.some(endpoint => url.includes(endpoint));
}

// Fetch event - serve from cache with network fallback
self.addEventListener('fetch', (event) => {
    const { request } = event;
    const url = new URL(request.url);

    // CRITICAL: Never intercept SignalR requests - let them pass through
    if (url.pathname.includes('/adminHub') || url.pathname.includes('/userHub') || 
        url.hostname.includes('241runners-api-v2.azurewebsites.net')) {
        // Let SignalR requests pass through without service worker interference
        return;
    }

    // CRITICAL: Never cache auth endpoints - always fetch fresh
    if (isAuthEndpoint(url.href)) {
        console.log('Auth endpoint detected, bypassing cache:', url.href);
        event.respondWith(
            fetch(request, {
                cache: 'no-store'
            }).catch(error => {
                console.error('Auth fetch failed:', error);
                throw error;
            })
        );
        return;
    }

    // CRITICAL: Never cache config.json - always fetch fresh
    if (url.pathname === '/config.json') {
        console.log('Config.json detected, bypassing cache:', url.href);
        event.respondWith(
            fetch(request, {
                cache: 'no-store',
                headers: {
                    'Cache-Control': 'no-store, no-cache, must-revalidate',
                    'Pragma': 'no-cache',
                    'Expires': '0'
                }
            }).catch(error => {
                console.error('Config fetch failed:', error);
                throw error;
            })
        );
        return;
    }

    // Skip non-GET requests
    if (request.method !== 'GET') {
        return;
    }

    // Skip external requests
    if (url.origin !== location.origin) {
        return;
    }

    // API requests - Network first with cache fallback
    if (url.pathname.startsWith('/api/')) {
        event.respondWith(
            fetch(request, {
                cache: 'no-cache',
                headers: {
                    'Cache-Control': 'no-cache, no-store, must-revalidate',
                    'Pragma': 'no-cache',
                    'Expires': '0'
                }
            })
                .then(response => {
                    // Only cache GET requests that are successful and not auth-related
                    if (response.ok && request.method === 'GET' && !isAuthEndpoint(url.href)) {
                        const responseToCache = response.clone();
                        caches.open(DYNAMIC_CACHE)
                            .then(cache => cache.put(request, responseToCache));
                    }
                    return response;
                })
                .catch(() => {
                    // Fallback to cache if network fails (only for non-auth endpoints)
                    if (!isAuthEndpoint(url.href)) {
                        return caches.match(request);
                    }
                    throw new Error('Network error and no cache available');
                })
        );
        return;
    }

    // Static assets - Cache first with network fallback
    if (url.pathname.match(/\.(js|css|png|jpg|jpeg|gif|svg|ico|woff|woff2|ttf|eot)$/)) {
        event.respondWith(
            caches.match(request)
                .then(response => {
                    if (response) {
                        return response;
                    }
                    return fetch(request)
                        .then(fetchResponse => {
                            if (fetchResponse.ok) {
                                const responseToCache = fetchResponse.clone();
                                caches.open(STATIC_CACHE)
                                    .then(cache => cache.put(request, responseToCache));
                            }
                            return fetchResponse;
                        });
                })
        );
        return;
    }

    // HTML pages - Network first with cache fallback
    event.respondWith(
        fetch(request)
            .then(response => {
                if (response.ok) {
                    const responseToCache = response.clone();
                    caches.open(DYNAMIC_CACHE)
                        .then(cache => cache.put(request, responseToCache));
                }
                return response;
            })
            .catch(() => {
                // Fallback to cache, then to index.html for navigation
                return caches.match(request)
                    .then(response => {
                        if (response) {
                            return response;
                        }
                        if (request.mode === 'navigate') {
                            return caches.match('/index.html');
                        }
                        throw new Error('No cached response available');
                    });
            })
    );
});

// Message event - handle messages from main thread
self.addEventListener('message', (event) => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
    
    if (event.data && event.data.type === 'GET_VERSION') {
        event.ports[0].postMessage({
            type: 'VERSION_INFO',
            version: CACHE_NAME,
            timestamp: Date.now()
        });
    }
});

// Background sync for offline actions
self.addEventListener('sync', (event) => {
    if (event.tag === 'background-sync') {
        event.waitUntil(
            // Handle background sync tasks
            handleBackgroundSync()
        );
    }
});

// Push notifications
self.addEventListener('push', (event) => {
    if (event.data) {
        const data = event.data.json();
        
        const options = {
            body: data.body,
            icon: '/assets/images/241-logo.jpg',
            badge: '/assets/images/241-logo.jpg',
            tag: '241runners-notification',
            requireInteraction: true,
            actions: [
                {
                    action: 'view',
                    title: 'View'
                },
                {
                    action: 'close',
                    title: 'Close'
                }
            ]
        };

        event.waitUntil(
            self.registration.showNotification(data.title, options)
        );
    }
});

// Notification click handler
self.addEventListener('notificationclick', (event) => {
    event.notification.close();

    if (event.action === 'view') {
        event.waitUntil(
            clients.openWindow('/')
        );
    }
});

// Helper function for background sync
async function handleBackgroundSync() {
    try {
        // Get pending actions from IndexedDB
        const pendingActions = await getPendingActions();
        
        for (const action of pendingActions) {
            try {
                await processPendingAction(action);
                await removePendingAction(action.id);
            } catch (error) {
                console.error('Failed to process pending action:', error);
            }
        }
    } catch (error) {
        console.error('Background sync failed:', error);
    }
}

// Helper functions for IndexedDB operations
async function getPendingActions() {
    // Implementation would depend on your IndexedDB setup
    return [];
}

async function processPendingAction(action) {
    // Process the pending action (e.g., sync form data)
    console.log('Processing pending action:', action);
}

async function removePendingAction(actionId) {
    // Remove the processed action from IndexedDB
    console.log('Removing pending action:', actionId);
}

console.log('Service Worker loaded successfully');
