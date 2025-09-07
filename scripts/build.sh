#!/bin/bash

# ============================================
# 241 RUNNERS AWARENESS - BUILD SCRIPT
# ============================================
# 
# Build script for hashed assets and version management
# P0 Implementation: Add hashed assets + version file for instant deploys

set -e

# Configuration
BUILD_DIR="dist"
VERSION_FILE="version.json"
ASSETS_DIR="assets"
HASH_LENGTH=8

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Get current git commit hash
get_git_commit() {
    if command -v git &> /dev/null && git rev-parse --git-dir &> /dev/null; then
        git rev-parse --short HEAD
    else
        echo "unknown"
    fi
}

# Generate content hash for a file
generate_hash() {
    local file="$1"
    if command -v md5sum &> /dev/null; then
        md5sum "$file" | cut -c1-$HASH_LENGTH
    elif command -v md5 &> /dev/null; then
        md5 -q "$file" | cut -c1-$HASH_LENGTH
    else
        # Fallback to timestamp
        date +%s | tail -c $HASH_LENGTH
    fi
}

# Create build directory
create_build_dir() {
    log_info "Creating build directory..."
    rm -rf "$BUILD_DIR"
    mkdir -p "$BUILD_DIR"
    log_success "Build directory created"
}

# Copy and hash assets
process_assets() {
    log_info "Processing assets..."
    
    # Create assets directory in build
    mkdir -p "$BUILD_DIR/$ASSETS_DIR"
    
    # Process CSS files
    if [ -d "assets/styles" ]; then
        mkdir -p "$BUILD_DIR/$ASSETS_DIR/styles"
        for css_file in assets/styles/*.css; do
            if [ -f "$css_file" ]; then
                local filename=$(basename "$css_file" .css)
                local hash=$(generate_hash "$css_file")
                local new_filename="${filename}.${hash}.css"
                cp "$css_file" "$BUILD_DIR/$ASSETS_DIR/styles/$new_filename"
                log_info "Processed CSS: $css_file -> $new_filename"
            fi
        done
    fi
    
    # Process JS files
    if [ -d "assets/js" ]; then
        mkdir -p "$BUILD_DIR/$ASSETS_DIR/js"
        for js_file in assets/js/*.js; do
            if [ -f "$js_file" ]; then
                local filename=$(basename "$js_file" .js)
                local hash=$(generate_hash "$js_file")
                local new_filename="${filename}.${hash}.js"
                cp "$js_file" "$BUILD_DIR/$ASSETS_DIR/js/$new_filename"
                log_info "Processed JS: $js_file -> $new_filename"
            fi
        done
    fi
    
    # Process images
    if [ -d "assets/images" ]; then
        mkdir -p "$BUILD_DIR/$ASSETS_DIR/images"
        for img_file in assets/images/*; do
            if [ -f "$img_file" ]; then
                local filename=$(basename "$img_file")
                local extension="${filename##*.}"
                local name="${filename%.*}"
                local hash=$(generate_hash "$img_file")
                local new_filename="${name}.${hash}.${extension}"
                cp "$img_file" "$BUILD_DIR/$ASSETS_DIR/images/$new_filename"
                log_info "Processed image: $img_file -> $new_filename"
            fi
        done
    fi
    
    log_success "Assets processed successfully"
}

# Copy HTML files and update asset references
process_html() {
    log_info "Processing HTML files..."
    
    # Copy all HTML files
    find . -name "*.html" -not -path "./$BUILD_DIR/*" -not -path "./node_modules/*" | while read -r html_file; do
        local relative_path=$(echo "$html_file" | sed 's|^\./||')
        local dir_path=$(dirname "$relative_path")
        
        # Create directory structure in build
        if [ "$dir_path" != "." ]; then
            mkdir -p "$BUILD_DIR/$dir_path"
        fi
        
        # Copy HTML file
        cp "$html_file" "$BUILD_DIR/$relative_path"
        
        # Update asset references with hashes
        update_asset_references "$BUILD_DIR/$relative_path"
        
        log_info "Processed HTML: $html_file"
    done
    
    log_success "HTML files processed successfully"
}

# Update asset references in HTML files
update_asset_references() {
    local html_file="$1"
    
    # Update CSS references
    if [ -d "$BUILD_DIR/$ASSETS_DIR/styles" ]; then
        for css_file in "$BUILD_DIR/$ASSETS_DIR/styles"/*.css; do
            if [ -f "$css_file" ]; then
                local filename=$(basename "$css_file")
                local original_name=$(echo "$filename" | sed 's/\.[^.]*\.css$/.css/')
                local original_path="assets/styles/$original_name"
                
                # Replace references in HTML
                sed -i.bak "s|$original_path|$ASSETS_DIR/styles/$filename|g" "$html_file"
                rm -f "$html_file.bak"
            fi
        done
    fi
    
    # Update JS references
    if [ -d "$BUILD_DIR/$ASSETS_DIR/js" ]; then
        for js_file in "$BUILD_DIR/$ASSETS_DIR/js"/*.js; do
            if [ -f "$js_file" ]; then
                local filename=$(basename "$js_file")
                local original_name=$(echo "$filename" | sed 's/\.[^.]*\.js$/.js/')
                local original_path="assets/js/$original_name"
                
                # Replace references in HTML
                sed -i.bak "s|$original_path|$ASSETS_DIR/js/$filename|g" "$html_file"
                rm -f "$html_file.bak"
            fi
        done
    fi
    
    # Update image references
    if [ -d "$BUILD_DIR/$ASSETS_DIR/images" ]; then
        for img_file in "$BUILD_DIR/$ASSETS_DIR/images"/*; do
            if [ -f "$img_file" ]; then
                local filename=$(basename "$img_file")
                local extension="${filename##*.}"
                local name="${filename%.*}"
                local hash="${name##*.}"
                local original_name="${name%.*}.${extension}"
                local original_path="assets/images/$original_name"
                
                # Replace references in HTML
                sed -i.bak "s|$original_path|$ASSETS_DIR/images/$filename|g" "$html_file"
                rm -f "$html_file.bak"
            fi
        done
    fi
}

# Copy additional files
copy_additional_files() {
    log_info "Copying additional files..."
    
    # Copy config files
    [ -f "config.json" ] && cp "config.json" "$BUILD_DIR/"
    
    # Copy partials
    [ -d "partials" ] && cp -r "partials" "$BUILD_DIR/"
    
    # Copy admin assets
    [ -d "admin/assets" ] && cp -r "admin/assets" "$BUILD_DIR/admin/"
    
    # Copy js directory
    [ -d "js" ] && cp -r "js" "$BUILD_DIR/"
    
    # Copy manifest and other PWA files
    [ -f "manifest.json" ] && cp "manifest.json" "$BUILD_DIR/"
    [ -f "sw-optimized.js" ] && cp "sw-optimized.js" "$BUILD_DIR/sw.js"
    
    log_success "Additional files copied"
}

# Create service worker
create_service_worker() {
    log_info "Creating service worker..."
    
    # Copy existing service worker if it exists
    if [ -f "sw-optimized.js" ]; then
        cp "sw-optimized.js" "$BUILD_DIR/sw.js"
        log_success "Service worker copied"
    else
        # Create basic service worker
        cat > "$BUILD_DIR/sw.js" << 'SW_EOF'
// Service Worker for 241 Runners Awareness
const CACHE_NAME = '241ra-v1';
const VERSION_URL = '/version.json';

// Install event
self.addEventListener('install', (event) => {
    console.log('Service Worker installing...');
    self.skipWaiting();
});

// Activate event
self.addEventListener('activate', (event) => {
    console.log('Service Worker activating...');
    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames.map((cacheName) => {
                    if (cacheName !== CACHE_NAME) {
                        console.log('Deleting old cache:', cacheName);
                        return caches.delete(cacheName);
                    }
                })
            );
        }).then(() => {
            return self.clients.claim();
        })
    );
});

// Fetch event
self.addEventListener('fetch', (event) => {
    const url = new URL(event.request.url);
    
    // Skip non-GET requests
    if (event.request.method !== 'GET') {
        return;
    }
    
    // Skip version.json - always fetch from network
    if (url.pathname === VERSION_URL) {
        return;
    }
    
    // Cache strategy: Cache first for assets, Network first for HTML
    if (url.pathname.match(/\.(css|js|png|jpg|jpeg|gif|svg|ico|woff|woff2)$/)) {
        // Cache first for assets
        event.respondWith(
            caches.match(event.request).then((response) => {
                return response || fetch(event.request).then((fetchResponse) => {
                    const responseClone = fetchResponse.clone();
                    caches.open(CACHE_NAME).then((cache) => {
                        cache.put(event.request, responseClone);
                    });
                    return fetchResponse;
                });
            })
        );
    } else {
        // Network first for HTML
        event.respondWith(
            fetch(event.request).then((response) => {
                const responseClone = response.clone();
                caches.open(CACHE_NAME).then((cache) => {
                    cache.put(event.request, responseClone);
                });
                return response;
            }).catch(() => {
                return caches.match(event.request);
            })
        );
    }
});

// Message event for update notifications
self.addEventListener('message', (event) => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
});
SW_EOF
        log_success "Service worker created"
    fi
}

# Generate version file
generate_version_file() {
    log_info "Generating version file..."
    
    local commit=$(get_git_commit)
    local build_time=$(date -u +"%Y-%m-%dT%H:%M:%S.000Z")
    local build_number=$(date +"%Y%m%d-%H%M")
    
    cat > "$BUILD_DIR/$VERSION_FILE" << VERSION_EOF
{
  "version": "1.0.0",
  "build": "$build_number",
  "commit": "$commit",
  "builtAt": "$build_time",
  "environment": "production",
  "apiVersion": "2.0.0",
  "assets": {
    "css": $(ls "$BUILD_DIR/$ASSETS_DIR/styles"/*.css 2>/dev/null | wc -l || echo 0),
    "js": $(ls "$BUILD_DIR/$ASSETS_DIR/js"/*.js 2>/dev/null | wc -l || echo 0),
    "images": $(ls "$BUILD_DIR/$ASSETS_DIR/images"/* 2>/dev/null | wc -l || echo 0)
  }
}
VERSION_EOF
    
    log_success "Version file generated: $build_number"
}

# Main build function
main() {
    log_info "Starting build process..."
    
    create_build_dir
    process_assets
    process_html
    copy_additional_files
    create_service_worker
    generate_version_file
    
    log_success "Build completed successfully!"
    log_info "Build directory: $BUILD_DIR"
    log_info "Version: $(cat "$BUILD_DIR/$VERSION_FILE" | grep '"build"' | cut -d'"' -f4)"
    
    # Show build summary
    echo ""
    echo "Build Summary:"
    echo "=============="
    echo "CSS files: $(ls "$BUILD_DIR/$ASSETS_DIR/styles"/*.css 2>/dev/null | wc -l || echo 0)"
    echo "JS files: $(ls "$BUILD_DIR/$ASSETS_DIR/js"/*.js 2>/dev/null | wc -l || echo 0)"
    echo "Images: $(ls "$BUILD_DIR/$ASSETS_DIR/images"/* 2>/dev/null | wc -l || echo 0)"
    echo "HTML files: $(find "$BUILD_DIR" -name "*.html" | wc -l)"
    echo ""
}

# Run main function
main "$@"
