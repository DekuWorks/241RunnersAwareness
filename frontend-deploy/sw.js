/**
 * 241 Runners Awareness - Service Worker
 * 
 * This service worker provides advanced caching strategies
 * for improved performance and offline functionality.
 */

const CACHE_NAME = '241runners-v1.0.0';
const STATIC_CACHE = 'static-v1.0.0';
const DYNAMIC_CACHE = 'dynamic-v1.0.0';
const API_CACHE = 'api-v1.0.0';

// Resources to cache immediately
const STATIC_RESOURCES = [
    '/',
    '/index.html',
    '/styles.css',
    '/forms.css',
    '/js/validation.js',
    '/js/error-handler.js',
    '/js/loading.js',
    '/js/utils.js',
    '/js/cache-manager.js',
    '/js/report-validation.js',
    '/js/report-submission.js',
    '/login.html',
    '/signup.html',
    '/dashboard.html',
    '/report-case.html',
    '/cases.html',
    '/my-cases.html',
    '/profile.html'
];

// API endpoints to cache
const API_ENDPOINTS = [
    '/api/v1/auth/health',
    '/api/v1/runners',
    '/api/v1/cases'
];

/**
 * Install event - cache static resources
 */
self.addEventListener('install', event => {
    console.log('Service Worker installing...');
    
    event.waitUntil(
        caches.open(STATIC_CACHE)
            .then(cache => {
                console.log('Caching static resources...');
                return cache.addAll(STATIC_RESOURCES);
            })
            .then(() => {
                console.log('Static resources cached successfully');
                return self.skipWaiting();
            })
            .catch(error => {
                console.error('Failed to cache static resources:', error);
            })
    );
});

/**
 * Activate event - clean up old caches
 */
self.addEventListener('activate', event => {
    console.log('Service Worker activating...');
    
    event.waitUntil(
        caches.keys()
            .then(cacheNames => {
                return Promise.all(
                    cacheNames.map(cacheName => {
                        if (cacheName !== STATIC_CACHE && 
                            cacheName !== DYNAMIC_CACHE && 
                            cacheName !== API_CACHE) {
                            console.log('Deleting old cache:', cacheName);
                            return caches.delete(cacheName);
                        }
                    })
                );
            })
            .then(() => {
                console.log('Service Worker activated');
                return self.clients.claim();
            })
    );
});

/**
 * Fetch event - implement caching strategies
 */
self.addEventListener('fetch', event => {
    const { request } = event;
    const url = new URL(request.url);
    
    // Skip non-GET requests
    if (request.method !== 'GET') {
        return;
    }
    
    // Skip chrome-extension and other non-http requests
    if (!url.protocol.startsWith('http')) {
        return;
    }
    
    event.respondWith(handleRequest(request));
});

/**
 * Handle different types of requests with appropriate caching strategies
 * @param {Request} request - The request to handle
 * @returns {Promise<Response>} - The response
 */
async function handleRequest(request) {
    const url = new URL(request.url);
    
    try {
        // Static resources - Cache First strategy
        if (isStaticResource(url)) {
            return await cacheFirst(request, STATIC_CACHE);
        }
        
        // API requests - Network First with fallback
        if (isApiRequest(url)) {
            return await networkFirst(request, API_CACHE);
        }
        
        // Images - Cache First with long TTL
        if (isImageRequest(url)) {
            return await cacheFirst(request, DYNAMIC_CACHE);
        }
        
        // Other requests - Network First
        return await networkFirst(request, DYNAMIC_CACHE);
        
    } catch (error) {
        console.error('Fetch error:', error);
        
        // Return offline page for navigation requests
        if (request.mode === 'navigate') {
            return await caches.match('/offline.html') || 
                   new Response('Offline - Please check your connection', {
                       status: 503,
                       statusText: 'Service Unavailable'
                   });
        }
        
        throw error;
    }
}

/**
 * Cache First strategy - check cache first, then network
 * @param {Request} request - The request
 * @param {string} cacheName - Cache name
 * @returns {Promise<Response>} - The response
 */
async function cacheFirst(request, cacheName) {
    const cache = await caches.open(cacheName);
    const cachedResponse = await cache.match(request);
    
    if (cachedResponse) {
        return cachedResponse;
    }
    
    try {
        const networkResponse = await fetch(request);
        
        // Cache successful responses
        if (networkResponse.ok) {
            cache.put(request, networkResponse.clone());
        }
        
        return networkResponse;
    } catch (error) {
        console.error('Network error:', error);
        throw error;
    }
}

/**
 * Network First strategy - try network first, fallback to cache
 * @param {Request} request - The request
 * @param {string} cacheName - Cache name
 * @returns {Promise<Response>} - The response
 */
async function networkFirst(request, cacheName) {
    const cache = await caches.open(cacheName);
    
    try {
        const networkResponse = await fetch(request);
        
        // Cache successful responses
        if (networkResponse.ok) {
            cache.put(request, networkResponse.clone());
        }
        
        return networkResponse;
    } catch (error) {
        console.log('Network failed, trying cache:', error);
        
        const cachedResponse = await cache.match(request);
        if (cachedResponse) {
            return cachedResponse;
        }
        
        throw error;
    }
}

/**
 * Check if URL is a static resource
 * @param {URL} url - The URL to check
 * @returns {boolean} - Whether it's a static resource
 */
function isStaticResource(url) {
    return url.pathname.endsWith('.html') ||
           url.pathname.endsWith('.css') ||
           url.pathname.endsWith('.js') ||
           url.pathname === '/';
}

/**
 * Check if URL is an API request
 * @param {URL} url - The URL to check
 * @returns {boolean} - Whether it's an API request
 */
function isApiRequest(url) {
    return url.pathname.startsWith('/api/') ||
           url.hostname.includes('241runners-api-v2.azurewebsites.net');
}

/**
 * Check if URL is an image request
 * @param {URL} url - The URL to check
 * @returns {boolean} - Whether it's an image request
 */
function isImageRequest(url) {
    return url.pathname.match(/\.(jpg|jpeg|png|gif|webp|svg)$/i);
}

/**
 * Background sync for offline actions
 */
self.addEventListener('sync', event => {
    console.log('Background sync:', event.tag);
    
    if (event.tag === 'report-sync') {
        event.waitUntil(syncReports());
    }
});

/**
 * Sync offline reports when back online
 */
async function syncReports() {
    try {
        const offlineReports = await getOfflineReports();
        
        for (const report of offlineReports) {
            try {
                await submitReport(report);
                await removeOfflineReport(report.id);
            } catch (error) {
                console.error('Failed to sync report:', error);
            }
        }
    } catch (error) {
        console.error('Background sync failed:', error);
    }
}

/**
 * Get offline reports from IndexedDB
 */
async function getOfflineReports() {
    // This would integrate with IndexedDB
    // For now, return empty array
    return [];
}

/**
 * Submit report to API
 */
async function submitReport(report) {
    const response = await fetch('/api/v1/runners', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${report.token}`
        },
        body: JSON.stringify(report.data)
    });
    
    if (!response.ok) {
        throw new Error('Failed to submit report');
    }
    
    return response.json();
}

/**
 * Remove offline report after successful sync
 */
async function removeOfflineReport(reportId) {
    // This would remove from IndexedDB
    console.log('Removing offline report:', reportId);
}

/**
 * Push notification handling
 */
self.addEventListener('push', event => {
    console.log('Push notification received:', event);
    
    const options = {
        body: event.data ? event.data.text() : 'New notification from 241 Runners Awareness',
        icon: '/icon-192x192.png',
        badge: '/badge-72x72.png',
        vibrate: [100, 50, 100],
        data: {
            dateOfArrival: Date.now(),
            primaryKey: 1
        },
        actions: [
            {
                action: 'explore',
                title: 'View Details',
                icon: '/icon-192x192.png'
            },
            {
                action: 'close',
                title: 'Close',
                icon: '/icon-192x192.png'
            }
        ]
    };
    
    event.waitUntil(
        self.registration.showNotification('241 Runners Awareness', options)
    );
});

/**
 * Notification click handling
 */
self.addEventListener('notificationclick', event => {
    console.log('Notification clicked:', event);
    
    event.notification.close();
    
    if (event.action === 'explore') {
        event.waitUntil(
            clients.openWindow('/dashboard.html')
        );
    }
});

/**
 * Message handling for cache management
 */
self.addEventListener('message', event => {
    const { type, data } = event.data;
    
    switch (type) {
        case 'CLEAR_CACHE':
            clearCache(data.cacheName);
            break;
        case 'CACHE_URLS':
            cacheUrls(data.urls);
            break;
        case 'GET_CACHE_STATS':
            getCacheStats().then(stats => {
                event.ports[0].postMessage(stats);
            });
            break;
    }
});

/**
 * Clear specific cache
 */
async function clearCache(cacheName) {
    const cache = await caches.open(cacheName);
    const keys = await cache.keys();
    
    for (const key of keys) {
        await cache.delete(key);
    }
}

/**
 * Cache specific URLs
 */
async function cacheUrls(urls) {
    const cache = await caches.open(DYNAMIC_CACHE);
    
    for (const url of urls) {
        try {
            const response = await fetch(url);
            if (response.ok) {
                await cache.put(url, response);
            }
        } catch (error) {
            console.error('Failed to cache URL:', url, error);
        }
    }
}

/**
 * Get cache statistics
 */
async function getCacheStats() {
    const cacheNames = await caches.keys();
    const stats = {};
    
    for (const cacheName of cacheNames) {
        const cache = await caches.open(cacheName);
        const keys = await cache.keys();
        stats[cacheName] = keys.length;
    }
    
    return stats;
}
