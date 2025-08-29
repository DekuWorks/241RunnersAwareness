// Performance Optimization Script for 241 Runners Awareness

// Lazy loading for images
function lazyLoadImages() {
  const images = document.querySelectorAll('img[data-src]');
  
  const imageObserver = new IntersectionObserver((entries, observer) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        const img = entry.target;
        img.src = img.dataset.src;
        img.classList.remove('lazy');
        imageObserver.unobserve(img);
      }
    });
  });

  images.forEach(img => imageObserver.observe(img));
}

// Defer non-critical JavaScript
function loadDeferredScripts() {
  const deferredScripts = [
    // Add any non-critical scripts here
  ];

  deferredScripts.forEach(script => {
    const scriptElement = document.createElement('script');
    scriptElement.src = script;
    scriptElement.async = true;
    document.body.appendChild(scriptElement);
  });
}

// Optimize CSS loading
function optimizeCSSLoading() {
  // Preload critical CSS
  const criticalCSS = document.createElement('link');
  criticalCSS.rel = 'preload';
  criticalCSS.href = '/styles.css';
  criticalCSS.as = 'style';
  document.head.appendChild(criticalCSS);
}

// Performance monitoring
function monitorPerformance() {
  if ('performance' in window) {
    window.addEventListener('load', () => {
      setTimeout(() => {
        const perfData = performance.getEntriesByType('navigation')[0];
        console.log('Page Load Time:', perfData.loadEventEnd - perfData.loadEventStart, 'ms');
        console.log('DOM Content Loaded:', perfData.domContentLoadedEventEnd - perfData.domContentLoadedEventStart, 'ms');
        
        // Send performance data to analytics if needed
        if (typeof gtag !== 'undefined') {
          gtag('event', 'timing_complete', {
            name: 'load',
            value: Math.round(perfData.loadEventEnd - perfData.loadEventStart)
          });
        }
      }, 0);
    });
  }
}

// Optimize fonts loading
function optimizeFonts() {
  // Preload critical fonts
  const fontLink = document.createElement('link');
  fontLink.rel = 'preload';
  fontLink.href = 'https://fonts.googleapis.com/css2?family=Segoe+UI:wght@400;600;700&display=swap';
  fontLink.as = 'style';
  document.head.appendChild(fontLink);
}

// Debounce function for performance
function debounce(func, wait) {
  let timeout;
  return function executedFunction(...args) {
    const later = () => {
      clearTimeout(timeout);
      func(...args);
    };
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
  };
}

// Optimize scroll events
function optimizeScrollEvents() {
  const scrollHandler = debounce(() => {
    // Handle scroll events efficiently
    const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
    
    // Add scroll-based optimizations here
    if (scrollTop > 100) {
      document.body.classList.add('scrolled');
    } else {
      document.body.classList.remove('scrolled');
    }
  }, 16); // ~60fps

  window.addEventListener('scroll', scrollHandler, { passive: true });
}

// Initialize performance optimizations
document.addEventListener('DOMContentLoaded', function() {
  // Initialize lazy loading
  lazyLoadImages();
  
  // Load deferred scripts
  loadDeferredScripts();
  
  // Optimize scroll events
  optimizeScrollEvents();
  
  // Monitor performance
  monitorPerformance();
});

// Optimize on page load
window.addEventListener('load', function() {
  // Optimize fonts
  optimizeFonts();
  
  // Optimize CSS loading
  optimizeCSSLoading();
});

// Service Worker performance monitoring
if ('serviceWorker' in navigator) {
  navigator.serviceWorker.addEventListener('message', function(event) {
    if (event.data && event.data.type === 'PERFORMANCE') {
      console.log('Service Worker Performance:', event.data.metrics);
    }
  });
}

// Memory optimization
function optimizeMemory() {
  // Clean up event listeners when elements are removed
  const observer = new MutationObserver((mutations) => {
    mutations.forEach((mutation) => {
      mutation.removedNodes.forEach((node) => {
        if (node.nodeType === Node.ELEMENT_NODE) {
          // Clean up any event listeners or references
          node.removeEventListener && node.removeEventListener();
        }
      });
    });
  });

  observer.observe(document.body, {
    childList: true,
    subtree: true
  });
}

// Initialize memory optimization
optimizeMemory();

// Export for use in other scripts
window.PerformanceOptimizer = {
  lazyLoadImages,
  debounce,
  monitorPerformance,
  optimizeMemory
}; 