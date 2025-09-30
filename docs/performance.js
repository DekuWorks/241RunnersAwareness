/**
 * ============================================
 * 241 RUNNERS AWARENESS - PERFORMANCE OPTIMIZATION
 * ============================================
 * 
 * This file provides performance optimizations for the application
 * including lazy loading, resource preloading, and performance monitoring.
 */

// Performance monitoring
const performanceObserver = new PerformanceObserver((list) => {
    for (const entry of list.getEntries()) {
        if (entry.entryType === 'navigation') {
            console.log('Page load time:', entry.loadEventEnd - entry.loadEventStart, 'ms');
        }
    }
});

// Observe navigation timing
if ('PerformanceObserver' in window) {
    try {
        performanceObserver.observe({ entryTypes: ['navigation'] });
    } catch (e) {
        console.warn('PerformanceObserver not supported');
    }
}

// Lazy load images
function lazyLoadImages() {
    const images = document.querySelectorAll('img[data-src]');
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.removeAttribute('data-src');
                observer.unobserve(img);
            }
        });
    });

    images.forEach(img => imageObserver.observe(img));
}

// Preload critical resources
function preloadCriticalResources() {
    const criticalResources = [
        '/auth-utils.js',
        '/js/auth.js',
        '/styles.css',
        '/forms.css'
    ];

    criticalResources.forEach(resource => {
        const link = document.createElement('link');
        link.rel = 'preload';
        link.href = resource;
        link.as = resource.endsWith('.js') ? 'script' : 'style';
        document.head.appendChild(link);
    });
}

// Initialize performance optimizations
document.addEventListener('DOMContentLoaded', () => {
    lazyLoadImages();
    preloadCriticalResources();
});

// Export for use in other modules
window.PerformanceUtils = {
    lazyLoadImages,
    preloadCriticalResources
}; 