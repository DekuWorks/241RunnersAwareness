#!/bin/bash

# Script to test all admin user logins
API_BASE_URL="https://241runners-api.azurewebsites.net/api"

log_info() {
    echo "[INFO] $1"
}

log_error() {
    echo "[ERROR] $1" >&2
}

# Function to test admin login
test_admin_login() {
    local email="$1"
    local password="$2"
    local name="$3"
    
    log_info "Testing login for: $name ($email)"
    
    local response=$(curl -s -X POST "$API_BASE_URL/auth/login" \
        -H "Content-Type: application/json" \
        -d "{\"email\":\"$email\",\"password\":\"$password\"}")
    
    if echo "$response" | grep -q '"success":true'; then
        log_info "✅ Login successful: $name"
        # Extract user ID and role
        local user_id=$(echo "$response" | grep -o '"id":[0-9]*' | grep -o '[0-9]*')
        local role=$(echo "$response" | grep -o '"role":"[^"]*"' | cut -d'"' -f4)
        log_info "   User ID: $user_id, Role: $role"
        return 0
    else
        log_error "❌ Login failed: $name"
        echo "Response: $response"
        return 1
    fi
}

log_info "Testing all admin user logins..."

# Test all admin users
test_admin_login "dekuworks1@gmail.com" "Marcus2025!" "Marcus Brown"
test_admin_login "danielcarey9770@yahoo.com" "Daniel2025!" "Daniel Carey"
test_admin_login "lthomas3350@gmail.com" "Lisa2025!" "Lisa Thomas"
test_admin_login "tinaleggins@yahoo.com" "Tina2025!" "Tina Matthews"
test_admin_login "mmelasky@iplawconsulting.com" "Mark2025!" "Mark Melasky"
test_admin_login "ralphfrank900@gmail.com" "Ralph2025!" "Ralph Frank"

log_info "Admin login testing completed!"

# Test the /me endpoint with one admin
log_info "Testing /me endpoint with Marcus Brown..."
login_response=$(curl -s -X POST "$API_BASE_URL/auth/login" \
    -H "Content-Type: application/json" \
    -d '{"email":"dekuworks1@gmail.com","password":"Marcus2025!"}')

token=$(echo "$login_response" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)

if [ -n "$token" ]; then
    me_response=$(curl -s -H "Authorization: Bearer $token" "$API_BASE_URL/auth/me")
    if echo "$me_response" | grep -q '"email":"dekuworks1@gmail.com"'; then
        log_info "✅ /me endpoint test successful!"
    else
        log_error "❌ /me endpoint test failed!"
        echo "Response: $me_response"
    fi
else
    log_error "❌ Could not extract token for /me test"
fi
