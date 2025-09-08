#!/bin/bash

# Script to seed admin users via the API registration endpoint
# This approach uses the existing working API

API_BASE_URL="https://241runners-api.azurewebsites.net/api"

log_info() {
    echo "[INFO] $1"
}

log_error() {
    echo "[ERROR] $1" >&2
}

# Function to register an admin user
register_admin() {
    local email="$1"
    local password="$2"
    local first_name="$3"
    local last_name="$4"
    local organization="$5"
    local title="$6"
    
    log_info "Registering admin: $first_name $last_name ($email)"
    
    local response=$(curl -s -X POST "$API_BASE_URL/auth/register" \
        -H "Content-Type: application/json" \
        -d "{
            \"email\": \"$email\",
            \"password\": \"$password\",
            \"confirmPassword\": \"$password\",
            \"firstName\": \"$first_name\",
            \"lastName\": \"$last_name\",
            \"role\": \"admin\",
            \"organization\": \"$organization\",
            \"title\": \"$title\",
            \"phoneNumber\": \"15551234567\",
            \"address\": \"123 Admin Street\",
            \"city\": \"Administration City\",
            \"state\": \"Admin State\",
            \"zipCode\": \"12345\",
            \"emergencyContactName\": \"Emergency Services\",
            \"emergencyContactPhone\": \"1555911\",
            \"emergencyContactRelationship\": \"Emergency Contact\"
        }")
    
    if echo "$response" | grep -q '"success":true'; then
        log_info "✅ Successfully registered: $first_name $last_name"
        return 0
    else
        log_error "❌ Failed to register: $first_name $last_name"
        echo "Response: $response"
        return 1
    fi
}

log_info "Starting admin user seeding via API..."

# Register all admin users
register_admin "dekuworks1@gmail.com" "marcus2025" "Marcus" "Brown" "241 Runners Awareness" "System Administrator"
register_admin "danielcarey9770@yahoo.com" "Daniel2025!" "Daniel" "Carey" "241 Runners Awareness" "Administrator"
register_admin "lthomas3350@gmail.com" "Lisa2025!" "Lisa" "Thomas" "241 Runners Awareness" "Administrator"
register_admin "tinaleggins@yahoo.com" "Tina2025!" "Tina" "Matthews" "241 Runners Awareness" "Administrator"
register_admin "mmelasky@iplawconsulting.com" "Mark2025!" "Mark" "Melasky" "IP Law Consulting" "Legal Administrator"
register_admin "ralphfrank900@gmail.com" "Ralph2025!" "Ralph" "Frank" "241 Runners Awareness" "Administrator"

log_info "Admin user seeding completed!"

# Test login with one of the admin users
log_info "Testing login with Marcus Brown..."
login_response=$(curl -s -X POST "$API_BASE_URL/auth/login" \
    -H "Content-Type: application/json" \
    -d '{"email":"dekuworks1@gmail.com","password":"marcus2025"}')

if echo "$login_response" | grep -q '"success":true'; then
    log_info "✅ Admin login test successful!"
    echo "Login response: $login_response"
else
    log_error "❌ Admin login test failed!"
    echo "Login response: $login_response"
fi
