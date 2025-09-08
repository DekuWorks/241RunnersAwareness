#!/bin/bash

# Script to test admin dashboard functionality
API_BASE_URL="https://241runners-api.azurewebsites.net/api"

log_info() {
    echo "[INFO] $1"
}

log_error() {
    echo "[ERROR] $1" >&2
}

log_info "Testing admin dashboard functionality..."

# Test 1: Health check endpoint
log_info "1. Testing health check endpoint..."
health_response=$(curl -s "$API_BASE_URL/auth/health")
if echo "$health_response" | grep -q '"status":"Healthy"'; then
    log_info "✅ Health check endpoint working"
else
    log_error "❌ Health check endpoint failed"
    echo "Response: $health_response"
fi

# Test 2: Admin login
log_info "2. Testing admin login..."
login_response=$(curl -s -X POST "$API_BASE_URL/auth/login" \
    -H "Content-Type: application/json" \
    -d '{"email":"dekuworks1@gmail.com","password":"Marcus2025!"}')

if echo "$login_response" | grep -q '"success":true'; then
    log_info "✅ Admin login successful"
    token=$(echo "$login_response" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
    log_info "Token extracted: ${token:0:50}..."
else
    log_error "❌ Admin login failed"
    echo "Response: $login_response"
    exit 1
fi

# Test 3: /me endpoint
log_info "3. Testing /me endpoint..."
me_response=$(curl -s -H "Authorization: Bearer $token" "$API_BASE_URL/auth/me")
if echo "$me_response" | grep -q '"email":"dekuworks1@gmail.com"'; then
    log_info "✅ /me endpoint working"
    user_id=$(echo "$me_response" | grep -o '"id":[0-9]*' | grep -o '[0-9]*')
    role=$(echo "$me_response" | grep -o '"role":"[^"]*"' | cut -d'"' -f4)
    log_info "User ID: $user_id, Role: $role"
else
    log_error "❌ /me endpoint failed"
    echo "Response: $me_response"
fi

# Test 4: Profile update
log_info "4. Testing profile update..."
profile_response=$(curl -s -X PUT -H "Authorization: Bearer $token" \
    -H "Content-Type: application/json" \
    -d '{"firstName":"Marcus","lastName":"Brown","phoneNumber":"15551234567"}' \
    "$API_BASE_URL/auth/profile")

if echo "$profile_response" | grep -q '"message":"Profile updated successfully"'; then
    log_info "✅ Profile update working"
else
    log_error "❌ Profile update failed"
    echo "Response: $profile_response"
fi

log_info "Admin dashboard API testing completed!"

# Test 5: Check if admin dashboard HTML is accessible
log_info "5. Testing admin dashboard HTML accessibility..."
if [ -f "admin/admindash.html" ]; then
    log_info "✅ Admin dashboard HTML file exists"
    # Check if it contains the updated API calls
    if grep -q "auth/me" admin/admindash.html; then
        log_info "✅ Admin dashboard has updated API calls"
    else
        log_error "❌ Admin dashboard may not have updated API calls"
    fi
else
    log_error "❌ Admin dashboard HTML file not found"
fi

log_info "All tests completed!"
