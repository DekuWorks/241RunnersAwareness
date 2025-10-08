/**
 * 241 Runners Awareness - API Optimizer
 * 
 * This file provides API optimization including request batching,
 * debouncing, and intelligent caching strategies.
 */

// Import utilities
import { debounce, throttle } from './utils.js';

/**
 * ============================================
 * API OPTIMIZER CLASS
 * ============================================
 */

class ApiOptimizer {
    constructor() {
        this.requestQueue = new Map();
        this.batchQueue = [];
        this.batchTimeout = null;
        this.batchDelay = 100; // 100ms batch delay
        this.maxBatchSize = 10;
        this.retryAttempts = new Map();
        this.maxRetries = 3;
        this.retryDelay = 1000; // 1 second base delay
        
        this.setupOptimizations();
    }

    /**
     * Setup API optimizations
     */
    setupOptimizations() {
        // Setup request batching
        this.setupRequestBatching();
        
        // Setup retry logic
        this.setupRetryLogic();
        
        // Setup request deduplication
        this.setupRequestDeduplication();
    }

    /**
     * Setup request batching
     */
    setupRequestBatching() {
        // Process batch queue periodically
        setInterval(() => {
            this.processBatchQueue();
        }, this.batchDelay);
    }

    /**
     * Setup retry logic
     */
    setupRetryLogic() {
        // Clean up old retry attempts
        setInterval(() => {
            this.cleanupRetryAttempts();
        }, 60000); // Every minute
    }

    /**
     * Setup request deduplication
     */
    setupRequestDeduplication() {
        // This is handled in the makeRequest method
    }

    /**
     * Make optimized API request
     * @param {string} url - API URL
     * @param {Object} options - Request options
     * @param {Object} optimizationOptions - Optimization options
     * @returns {Promise<Object>} - API response
     */
    async makeRequest(url, options = {}, optimizationOptions = {}) {
        const {
            batch = false,
            debounce: debounceMs = 0,
            throttle: throttleMs = 0,
            retry = true,
            cache = true,
            priority = 'normal'
        } = optimizationOptions;

        // Generate request key for deduplication
        const requestKey = this.generateRequestKey(url, options);
        
        // Check for duplicate request
        if (this.requestQueue.has(requestKey)) {
            return this.requestQueue.get(requestKey);
        }

        // Create request promise
        const requestPromise = this.executeRequest(url, options, {
            batch,
            debounceMs,
            throttleMs,
            retry,
            cache,
            priority
        });

        // Store in queue for deduplication
        this.requestQueue.set(requestKey, requestPromise);

        // Clean up after request completes
        requestPromise.finally(() => {
            this.requestQueue.delete(requestKey);
        });

        return requestPromise;
    }

    /**
     * Execute the actual request
     * @param {string} url - API URL
     * @param {Object} options - Request options
     * @param {Object} optimizationOptions - Optimization options
     * @returns {Promise<Object>} - API response
     */
    async executeRequest(url, options, optimizationOptions) {
        const { batch, debounceMs, throttleMs, retry, cache, priority } = optimizationOptions;

        // Handle batching
        if (batch) {
            return this.addToBatch(url, options, priority);
        }

        // Handle debouncing
        if (debounceMs > 0) {
            return this.debouncedRequest(url, options, debounceMs);
        }

        // Handle throttling
        if (throttleMs > 0) {
            return this.throttledRequest(url, options, throttleMs);
        }

        // Make direct request
        return this.directRequest(url, options, { retry, cache });
    }

    /**
     * Add request to batch queue
     * @param {string} url - API URL
     * @param {Object} options - Request options
     * @param {string} priority - Request priority
     * @returns {Promise<Object>} - API response
     */
    addToBatch(url, options, priority) {
        return new Promise((resolve, reject) => {
            const batchItem = {
                url,
                options,
                priority,
                resolve,
                reject,
                timestamp: Date.now()
            };

            this.batchQueue.push(batchItem);
            
            // Sort by priority
            this.batchQueue.sort((a, b) => {
                const priorityOrder = { high: 0, normal: 1, low: 2 };
                return priorityOrder[a.priority] - priorityOrder[b.priority];
            });

            // Process batch if it's full
            if (this.batchQueue.length >= this.maxBatchSize) {
                this.processBatchQueue();
            }
        });
    }

    /**
     * Process batch queue
     */
    async processBatchQueue() {
        if (this.batchQueue.length === 0) return;

        const batch = this.batchQueue.splice(0, this.maxBatchSize);
        
        try {
            // Group requests by endpoint
            const groupedRequests = this.groupRequestsByEndpoint(batch);
            
            // Execute grouped requests
            const results = await this.executeBatchRequests(groupedRequests);
            
            // Resolve individual promises
            batch.forEach((item, index) => {
                const result = results[index];
                if (result.success) {
                    item.resolve(result.data);
                } else {
                    item.reject(new Error(result.error));
                }
            });
        } catch (error) {
            // Reject all batch items
            batch.forEach(item => {
                item.reject(error);
            });
        }
    }

    /**
     * Group requests by endpoint
     * @param {Array} batch - Batch of requests
     * @returns {Object} - Grouped requests
     */
    groupRequestsByEndpoint(batch) {
        const grouped = {};
        
        batch.forEach(item => {
            const endpoint = this.getEndpointFromUrl(item.url);
            if (!grouped[endpoint]) {
                grouped[endpoint] = [];
            }
            grouped[endpoint].push(item);
        });
        
        return grouped;
    }

    /**
     * Execute batch requests
     * @param {Object} groupedRequests - Grouped requests
     * @returns {Promise<Array>} - Batch results
     */
    async executeBatchRequests(groupedRequests) {
        const results = [];
        
        for (const [endpoint, requests] of Object.entries(groupedRequests)) {
            try {
                const batchResponse = await this.makeBatchRequest(endpoint, requests);
                results.push(...batchResponse);
            } catch (error) {
                // Add error results for this batch
                requests.forEach(() => {
                    results.push({ success: false, error: error.message });
                });
            }
        }
        
        return results;
    }

    /**
     * Make batch request to endpoint
     * @param {string} endpoint - API endpoint
     * @param {Array} requests - Requests to batch
     * @returns {Promise<Array>} - Batch response
     */
    async makeBatchRequest(endpoint, requests) {
        const batchData = requests.map(item => ({
            url: item.url,
            method: item.options.method || 'GET',
            body: item.options.body,
            headers: item.options.headers
        }));

        const response = await fetch(`${endpoint}/batch`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${this.getAuthToken()}`
            },
            body: JSON.stringify({ requests: batchData })
        });

        if (!response.ok) {
            throw new Error(`Batch request failed: ${response.statusText}`);
        }

        return await response.json();
    }

    /**
     * Debounced request
     * @param {string} url - API URL
     * @param {Object} options - Request options
     * @param {number} delay - Debounce delay
     * @returns {Promise<Object>} - API response
     */
    debouncedRequest(url, options, delay) {
        return new Promise((resolve, reject) => {
            const debouncedFn = debounce(async () => {
                try {
                    const result = await this.directRequest(url, options);
                    resolve(result);
                } catch (error) {
                    reject(error);
                }
            }, delay);
            
            debouncedFn();
        });
    }

    /**
     * Throttled request
     * @param {string} url - API URL
     * @param {Object} options - Request options
     * @param {number} delay - Throttle delay
     * @returns {Promise<Object>} - API response
     */
    throttledRequest(url, options, delay) {
        return new Promise((resolve, reject) => {
            const throttledFn = throttle(async () => {
                try {
                    const result = await this.directRequest(url, options);
                    resolve(result);
                } catch (error) {
                    reject(error);
                }
            }, delay);
            
            throttledFn();
        });
    }

    /**
     * Direct request with retry logic
     * @param {string} url - API URL
     * @param {Object} options - Request options
     * @param {Object} requestOptions - Request options
     * @returns {Promise<Object>} - API response
     */
    async directRequest(url, options, requestOptions = {}) {
        const { retry = true, cache = true } = requestOptions;
        
        try {
            // Check cache first
            if (cache && window.apiCache) {
                const cachedData = window.apiCache.getCachedApiResponse(url, options);
                if (cachedData) {
                    return cachedData;
                }
            }

            const response = await this.makeHttpRequest(url, options);
            
            // Cache successful responses
            if (cache && window.apiCache && response.success !== false) {
                window.apiCache.cacheApiResponse(url, options, response);
            }
            
            return response;
        } catch (error) {
            if (retry && this.shouldRetry(url, error)) {
                return this.retryRequest(url, options, requestOptions);
            }
            throw error;
        }
    }

    /**
     * Make HTTP request
     * @param {string} url - API URL
     * @param {Object} options - Request options
     * @returns {Promise<Object>} - API response
     */
    async makeHttpRequest(url, options) {
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
            },
            timeout: 30000
        };

        const mergedOptions = { ...defaultOptions, ...options };
        
        // Add auth token
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
     * Retry request with exponential backoff
     * @param {string} url - API URL
     * @param {Object} options - Request options
     * @param {Object} requestOptions - Request options
     * @returns {Promise<Object>} - API response
     */
    async retryRequest(url, options, requestOptions) {
        const retryKey = this.generateRequestKey(url, options);
        const retryCount = this.retryAttempts.get(retryKey) || 0;
        
        if (retryCount >= this.maxRetries) {
            this.retryAttempts.delete(retryKey);
            throw new Error('Max retries exceeded');
        }

        this.retryAttempts.set(retryKey, retryCount + 1);
        
        // Exponential backoff
        const delay = this.retryDelay * Math.pow(2, retryCount);
        await this.sleep(delay);

        try {
            const result = await this.makeHttpRequest(url, options);
            this.retryAttempts.delete(retryKey);
            return result;
        } catch (error) {
            if (retryCount + 1 >= this.maxRetries) {
                this.retryAttempts.delete(retryKey);
                throw error;
            }
            return this.retryRequest(url, options, requestOptions);
        }
    }

    /**
     * Check if request should be retried
     * @param {string} url - API URL
     * @param {Error} error - Error object
     * @returns {boolean} - Whether to retry
     */
    shouldRetry(url, error) {
        // Don't retry client errors (4xx) except 408, 429
        if (error.message.includes('4')) {
            return error.message.includes('408') || error.message.includes('429');
        }
        
        // Retry server errors (5xx) and network errors
        return error.message.includes('5') || error.message.includes('Network');
    }

    /**
     * Generate request key for deduplication
     * @param {string} url - API URL
     * @param {Object} options - Request options
     * @returns {string} - Request key
     */
    generateRequestKey(url, options) {
        const method = options.method || 'GET';
        const body = options.body ? JSON.stringify(options.body) : '';
        return `${method}:${url}:${body}`;
    }

    /**
     * Get endpoint from URL
     * @param {string} url - Full URL
     * @returns {string} - Endpoint
     */
    getEndpointFromUrl(url) {
        const urlObj = new URL(url);
        return `${urlObj.protocol}//${urlObj.host}`;
    }

    /**
     * Get authentication token
     * @returns {string|null} - Auth token
     */
    getAuthToken() {
        return localStorage.getItem('userToken') || localStorage.getItem('jwtToken');
    }

    /**
     * Sleep utility
     * @param {number} ms - Milliseconds to sleep
     * @returns {Promise} - Sleep promise
     */
    sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    /**
     * Cleanup retry attempts
     */
    cleanupRetryAttempts() {
        const now = Date.now();
        const maxAge = 5 * 60 * 1000; // 5 minutes
        
        for (const [key, timestamp] of this.retryAttempts.entries()) {
            if (now - timestamp > maxAge) {
                this.retryAttempts.delete(key);
            }
        }
    }

    /**
     * Get optimization statistics
     * @returns {Object} - Optimization stats
     */
    getStats() {
        return {
            queuedRequests: this.requestQueue.size,
            batchQueue: this.batchQueue.length,
            retryAttempts: this.retryAttempts.size,
            cacheStats: window.apiCache ? window.apiCache.getCacheStats() : null
        };
    }
}

// Initialize API optimizer
let apiOptimizer;
document.addEventListener('DOMContentLoaded', () => {
    apiOptimizer = new ApiOptimizer();
    
    // Make globally available
    window.apiOptimizer = apiOptimizer;
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ApiOptimizer;
}
