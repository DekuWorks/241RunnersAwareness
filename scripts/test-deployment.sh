#!/bin/bash

# ============================================
# 241 RUNNERS AWARENESS - DEPLOYMENT TEST
# ============================================
# 
# Test script to verify deployment readiness

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

# Test results
TESTS_PASSED=0
TESTS_FAILED=0

# Test function
run_test() {
    local test_name="$1"
    local test_command="$2"
    
    log_info "Testing: $test_name"
    
    if eval "$test_command" 2>/dev/null; then
        log_success "$test_name - PASSED"
        ((TESTS_PASSED++))
    else
        log_error "$test_name - FAILED"
        ((TESTS_FAILED++))
    fi
}

# Test 1: Check if build script exists and is executable
test_build_script() {
    [ -f "scripts/build.sh" ] && [ -x "scripts/build.sh" ]
}

# Test 2: Check if dist directory exists after build
test_build_output() {
    ./scripts/build.sh > /dev/null 2>&1
    [ -d "dist" ] && [ -f "dist/version.json" ]
}

# Test 3: Check if API builds successfully
test_api_build() {
    cd 241RunnersAwarenessAPI
    dotnet build --configuration Release > /dev/null 2>&1
    local result=$?
    cd ..
    return $result
}

# Test 4: Check if required files exist
test_required_files() {
    local files=(
        "js/admin-auth.js"
        "js/admin-realtime.js"
        "js/update-banner.js"
        "241RunnersAwarenessAPI/Hubs/AdminHub.cs"
        "241RunnersAwarenessAPI/Services/RealtimeNotificationService.cs"
        ".github/workflows/frontend.yml"
        ".github/workflows/api.yml"
        "package.json"
        "version.json"
    )
    
    for file in "${files[@]}"; do
        if [ ! -f "$file" ]; then
            return 1
        fi
    done
    return 0
}

# Test 5: Check if HTML files have proper structure
test_html_structure() {
    local html_files=(
        "admin/admindash.html"
        "admin/login.html"
        "index.html"
    )
    
    for file in "${html_files[@]}"; do
        if [ ! -f "$file" ]; then
            return 1
        fi
        
        # Check for basic HTML structure
        if ! grep -q "<!DOCTYPE html>" "$file"; then
            return 1
        fi
    done
    return 0
}

# Test 6: Check if service worker exists
test_service_worker() {
    [ -f "sw-optimized.js" ] || [ -f "dist/sw.js" ]
}

# Test 7: Check if version.json is valid
test_version_json() {
    if [ -f "version.json" ]; then
        # Check if it's valid JSON
        python3 -m json.tool version.json > /dev/null 2>&1
    else
        return 1
    fi
}

# Test 8: Check if package.json is valid
test_package_json() {
    if [ -f "package.json" ]; then
        # Check if it's valid JSON
        python3 -m json.tool package.json > /dev/null 2>&1
    else
        return 1
    fi
}

# Test 9: Check if CNAME file exists
test_cname() {
    [ -f "CNAME" ] && [ -s "CNAME" ]
}

# Test 10: Check if config.json exists
test_config() {
    [ -f "config.json" ]
}

# Main test function
main() {
    log_info "Starting deployment readiness tests..."
    echo ""
    
    # Run all tests
    run_test "Build Script" "test_build_script"
    run_test "Build Output" "test_build_output"
    run_test "API Build" "test_api_build"
    run_test "Required Files" "test_required_files"
    run_test "HTML Structure" "test_html_structure"
    run_test "Service Worker" "test_service_worker"
    run_test "Version JSON" "test_version_json"
    run_test "Package JSON" "test_package_json"
    run_test "CNAME File" "test_cname"
    run_test "Config File" "test_config"
    
    echo ""
    log_info "Test Results:"
    echo "=============="
    log_success "Passed: $TESTS_PASSED"
    if [ $TESTS_FAILED -gt 0 ]; then
        log_error "Failed: $TESTS_FAILED"
    else
        log_success "Failed: $TESTS_FAILED"
    fi
    
    echo ""
    if [ $TESTS_FAILED -eq 0 ]; then
        log_success "🎉 All tests passed! Ready for deployment."
        echo ""
        log_info "Next steps:"
        echo "1. Configure GitHub Actions secrets"
        echo "2. Set up Azure App Service"
        echo "3. Configure DNS"
        echo "4. Push to main branch to trigger deployment"
        return 0
    else
        log_error "❌ Some tests failed. Please fix the issues before deploying."
        return 1
    fi
}

# Run main function
main "$@"
