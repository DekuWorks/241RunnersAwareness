/**
 * 241 Runners Awareness - Cache Manager
 * 
 * This file provides intelligent caching for API responses,
 * user data, and static content to improve performance.
 */

/**
 * ============================================
 * CACHE MANAGER CLASS
 * ============================================
 */

class CacheManager {
    constructor() {
        this.cache = new Map();
        this.maxCacheSize = 50; // Maximum number of cached items
        this.defaultTTL = 5 * 60 * 1000; // 5 minutes default TTL
        this.cacheStats = {
            hits: 0,
            misses: 0,
            evictions: 0
        };
        
        this.setupCacheCleanup();
    }

    /**
     * Setup automatic cache cleanup
     */
    setupCacheCleanup() {
        // Clean expired entries every 5 minutes
        setInterval(() => {
            this.cleanExpiredEntries();
        }, 5 * 60 * 1000);
    }

    /**
     * Get cached item
     * @param {string} key - Cache key
     * @returns {*} - Cached value or null
     */
    get(key) {
        const item = this.cache.get(key);
        
        if (!item) {
            this.cacheStats.misses++;
            return null;
        }

        // Check if expired
        if (Date.now() > item.expiresAt) {
            this.cache.delete(key);
            this.cacheStats.misses++;
            return null;
        }

        this.cacheStats.hits++;
        return item.value;
    }

    /**
     * Set cached item
     * @param {string} key - Cache key
     * @param {*} value - Value to cache
     * @param {number} ttl - Time to live in milliseconds
     */
    set(key, value, ttl = this.defaultTTL) {
        // Remove oldest entries if cache is full
        if (this.cache.size >= this.maxCacheSize) {
            this.evictOldest();
        }

        const item = {
            value: value,
            expiresAt: Date.now() + ttl,
            createdAt: Date.now(),
            accessCount: 0
        };

        this.cache.set(key, item);
    }

    /**
     * Check if key exists and is not expired
     * @param {string} key - Cache key
     * @returns {boolean} - Whether key exists and is valid
     */
    has(key) {
        const item = this.cache.get(key);
        return item && Date.now() <= item.expiresAt;
    }

    /**
     * Delete cached item
     * @param {string} key - Cache key
     * @returns {boolean} - Whether item was deleted
     */
    delete(key) {
        return this.cache.delete(key);
    }

    /**
     * Clear all cache
     */
    clear() {
        this.cache.clear();
        this.cacheStats = { hits: 0, misses: 0, evictions: 0 };
    }

    /**
     * Get cache statistics
     * @returns {Object} - Cache statistics
     */
    getStats() {
        const total = this.cacheStats.hits + this.cacheStats.misses;
        return {
            ...this.cacheStats,
            hitRate: total > 0 ? (this.cacheStats.hits / total * 100).toFixed(2) + '%' : '0%',
            size: this.cache.size,
            maxSize: this.maxCacheSize
        };
    }

    /**
     * Clean expired entries
     */
    cleanExpiredEntries() {
        const now = Date.now();
        for (const [key, item] of this.cache.entries()) {
            if (now > item.expiresAt) {
                this.cache.delete(key);
            }
        }
    }

    /**
     * Evict oldest entry
     */
    evictOldest() {
        let oldestKey = null;
        let oldestTime = Date.now();

        for (const [key, item] of this.cache.entries()) {
            if (item.createdAt < oldestTime) {
                oldestTime = item.createdAt;
                oldestKey = key;
            }
        }

        if (oldestKey) {
            this.cache.delete(oldestKey);
            this.cacheStats.evictions++;
        }
    }

    /**
     * Generate cache key for API request
     * @param {string} endpoint - API endpoint
     * @param {Object} params - Request parameters
     * @returns {string} - Cache key
     */
    generateApiKey(endpoint, params = {}) {
        const sortedParams = Object.keys(params)
            .sort()
            .map(key => `${key}=${params[key]}`)
            .join('&');
        
        return `api:${endpoint}:${sortedParams}`;
    }

    /**
     * Cache API response
     * @param {string} endpoint - API endpoint
     * @param {Object} params - Request parameters
     * @param {*} data - Response data
     * @param {number} ttl - Time to live
     */
    cacheApiResponse(endpoint, params, data, ttl = this.defaultTTL) {
        const key = this.generateApiKey(endpoint, params);
        this.set(key, data, ttl);
    }

    /**
     * Get cached API response
     * @param {string} endpoint - API endpoint
     * @param {Object} params - Request parameters
     * @returns {*} - Cached data or null
     */
    getCachedApiResponse(endpoint, params = {}) {
        const key = this.generateApiKey(endpoint, params);
        return this.get(key);
    }

    /**
     * Invalidate cache by pattern
     * @param {string} pattern - Pattern to match
     */
    invalidatePattern(pattern) {
        for (const key of this.cache.keys()) {
            if (key.includes(pattern)) {
                this.cache.delete(key);
            }
        }
    }
}

/**
 * ============================================
 * API CACHE WRAPPER
 * ============================================
 */

class ApiCacheWrapper {
    constructor() {
        this.cacheManager = new CacheManager();
        this.requestQueue = new Map(); // Prevent duplicate requests
    }

    /**
     * Make cached API request
     * @param {string} url - API URL
     * @param {Object} options - Fetch options
     * @param {Object} cacheOptions - Cache options
     * @returns {Promise<Object>} - API response
     */
    async request(url, options = {}, cacheOptions = {}) {
        const {
            cache = true,
            ttl = 5 * 60 * 1000, // 5 minutes
            forceRefresh = false
        } = cacheOptions;

        // Generate cache key
        const cacheKey = this.cacheManager.generateApiKey(url, options);

        // Check cache first (unless force refresh)
        if (cache && !forceRefresh) {
            const cachedData = this.cacheManager.get(cacheKey);
            if (cachedData) {
                return cachedData;
            }
        }

        // Check if request is already in progress
        if (this.requestQueue.has(cacheKey)) {
            return this.requestQueue.get(cacheKey);
        }

        // Make API request
        const requestPromise = this.makeRequest(url, options);
        this.requestQueue.set(cacheKey, requestPromise);

        try {
            const response = await requestPromise;
            
            // Cache successful responses
            if (cache && response.success !== false) {
                this.cacheManager.cacheApiResponse(url, options, response, ttl);
            }
            
            return response;
        } finally {
            this.requestQueue.delete(cacheKey);
        }
    }

    /**
     * Make actual API request
     * @param {string} url - API URL
     * @param {Object} options - Fetch options
     * @returns {Promise<Object>} - API response
     */
    async makeRequest(url, options) {
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
            },
            timeout: 30000
        };

        const mergedOptions = { ...defaultOptions, ...options };
        
        // Add auth token if available
        const token = this.getAuthToken();
        if (token) {
            mergedOptions.headers['Authorization'] = `Bearer ${token}`;
        }

        // Add timeout
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), mergedOptions.timeout);
        mergedOptions.signal = controller.signal;

        try {
            const response = await fetch(url, mergedOptions);
            clearTimeout(timeoutId);

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            return await response.json();
        } catch (error) {
            clearTimeout(timeoutId);
            throw error;
        }
    }

    /**
     * Get authentication token
     * @returns {string|null} - Auth token
     */
    getAuthToken() {
        return localStorage.getItem('userToken') || localStorage.getItem('jwtToken');
    }

    /**
     * Invalidate cache for specific endpoint
     * @param {string} endpoint - API endpoint
     */
    invalidateEndpoint(endpoint) {
        this.cacheManager.invalidatePattern(endpoint);
    }

    /**
     * Clear all cache
     */
    clearCache() {
        this.cacheManager.clear();
    }

    /**
     * Get cache statistics
     * @returns {Object} - Cache statistics
     */
    getCacheStats() {
        return this.cacheManager.getStats();
    }
}

/**
 * ============================================
 * PERFORMANCE OPTIMIZATIONS
 * ============================================
 */

class PerformanceOptimizer {
    constructor() {
        this.apiCache = new ApiCacheWrapper();
        this.imageCache = new Map();
        this.setupImageOptimization();
        this.setupLazyLoading();
    }

    /**
     * Setup image optimization
     */
    setupImageOptimization() {
        // Intersection Observer for lazy loading
        if ('IntersectionObserver' in window) {
            this.imageObserver = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        this.loadImage(entry.target);
                        this.imageObserver.unobserve(entry.target);
                    }
                });
            }, {
                rootMargin: '50px'
            });
        }
    }

    /**
     * Setup lazy loading for images
     */
    setupLazyLoading() {
        // Add lazy loading to all images
        const images = document.querySelectorAll('img[data-src]');
        images.forEach(img => {
            if (this.imageObserver) {
                this.imageObserver.observe(img);
            } else {
                // Fallback for browsers without IntersectionObserver
                this.loadImage(img);
            }
        });
    }

    /**
     * Load image with optimization
     * @param {HTMLImageElement} img - Image element
     */
    loadImage(img) {
        const src = img.dataset.src;
        if (!src) return;

        // Check if image is already cached
        if (this.imageCache.has(src)) {
            img.src = this.imageCache.get(src);
            img.classList.remove('lazy');
            return;
        }

        // Create optimized image
        const optimizedImg = new Image();
        optimizedImg.onload = () => {
            img.src = optimizedImg.src;
            img.classList.remove('lazy');
            this.imageCache.set(src, optimizedImg.src);
        };
        optimizedImg.onerror = () => {
            img.classList.add('error');
        };
        optimizedImg.src = src;
    }

    /**
     * Optimize image for web
     * @param {File} file - Image file
     * @param {Object} options - Optimization options
     * @returns {Promise<Blob>} - Optimized image blob
     */
    async optimizeImage(file, options = {}) {
        const {
            maxWidth = 1920,
            maxHeight = 1080,
            quality = 0.8,
            format = 'jpeg'
        } = options;

        return new Promise((resolve, reject) => {
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');
            const img = new Image();

            img.onload = () => {
                // Calculate new dimensions
                let { width, height } = img;
                
                if (width > maxWidth || height > maxHeight) {
                    const ratio = Math.min(maxWidth / width, maxHeight / height);
                    width *= ratio;
                    height *= ratio;
                }
                
                canvas.width = width;
                canvas.height = height;
                
                // Draw and compress
                ctx.drawImage(img, 0, 0, width, height);
                canvas.toBlob(resolve, `image/${format}`, quality);
            };
            
            img.onerror = reject;
            img.src = URL.createObjectURL(file);
        });
    }

    /**
     * Preload critical resources
     */
    preloadCriticalResources() {
        const criticalResources = [
            '/styles.css',
            '/js/validation.js',
            '/js/error-handler.js',
            '/js/loading.js'
        ];

        criticalResources.forEach(resource => {
            const link = document.createElement('link');
            link.rel = 'preload';
            link.href = resource;
            link.as = resource.endsWith('.css') ? 'style' : 'script';
            document.head.appendChild(link);
        });
    }

    /**
     * Setup service worker for caching
     */
    setupServiceWorker() {
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.register('/sw.js')
                .then(registration => {
                    console.log('Service Worker registered:', registration);
                })
                .catch(error => {
                    console.log('Service Worker registration failed:', error);
                });
        }
    }
}

// Initialize cache manager and performance optimizer
let cacheManager, apiCache, performanceOptimizer;
document.addEventListener('DOMContentLoaded', () => {
    cacheManager = new CacheManager();
    apiCache = new ApiCacheWrapper();
    performanceOptimizer = new PerformanceOptimizer();
    
    // Make globally available
    window.cacheManager = cacheManager;
    window.apiCache = apiCache;
    window.performanceOptimizer = performanceOptimizer;
    
    // Preload critical resources
    performanceOptimizer.preloadCriticalResources();
    performanceOptimizer.setupServiceWorker();
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { CacheManager, ApiCacheWrapper, PerformanceOptimizer };
}
