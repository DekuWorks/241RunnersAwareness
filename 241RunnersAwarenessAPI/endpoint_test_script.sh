#!/bin/bash

# 241 Runners Awareness API Endpoint Testing Script
# This script tests all available API endpoints

API_BASE="http://localhost:5001"
echo "=== 241 Runners Awareness API Endpoint Testing ==="
echo "API Base URL: $API_BASE"
echo "Date: $(date)"
echo ""

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to test endpoint
test_endpoint() {
    local method=$1
    local endpoint=$2
    local description=$3
    local auth_header=$4
    local data=$5
    
    echo -e "${BLUE}Testing:${NC} $method $endpoint"
    echo -e "${YELLOW}Description:${NC} $description"
    
    if [ -n "$data" ]; then
        if [ -n "$auth_header" ]; then
            response=$(curl -s -w "HTTP_STATUS:%{http_code}" -X $method "$API_BASE$endpoint" \
                -H "Content-Type: application/json" \
                -H "$auth_header" \
                -d "$data")
        else
            response=$(curl -s -w "HTTP_STATUS:%{http_code}" -X $method "$API_BASE$endpoint" \
                -H "Content-Type: application/json" \
                -d "$data")
        fi
    else
        if [ -n "$auth_header" ]; then
            response=$(curl -s -w "HTTP_STATUS:%{http_code}" -X $method "$API_BASE$endpoint" \
                -H "Accept: application/json" \
                -H "$auth_header")
        else
            response=$(curl -s -w "HTTP_STATUS:%{http_code}" -X $method "$API_BASE$endpoint" \
                -H "Accept: application/json")
        fi
    fi
    
    http_code=$(echo $response | grep -o "HTTP_STATUS:[0-9]*" | cut -d: -f2)
    body=$(echo $response | sed -E 's/HTTP_STATUS:[0-9]*$//')
    
    if [ "$http_code" -ge 200 ] && [ "$http_code" -lt 300 ]; then
        echo -e "${GREEN}✓ SUCCESS${NC} (HTTP $http_code)"
    elif [ "$http_code" -ge 400 ] && [ "$http_code" -lt 500 ]; then
        echo -e "${YELLOW}⚠ CLIENT ERROR${NC} (HTTP $http_code)"
    else
        echo -e "${RED}✗ ERROR${NC} (HTTP $http_code)"
    fi
    
    # Show first 200 characters of response
    if [ ${#body} -gt 200 ]; then
        echo "Response: ${body:0:200}..."
    else
        echo "Response: $body"
    fi
    echo ""
}

echo "=== AUTHENTICATION ENDPOINTS ==="
test_endpoint "GET" "/api/auth/test" "Test endpoint to verify API is running"
test_endpoint "GET" "/api/auth/health" "Health check with database connection test"

echo "=== USER REGISTRATION & LOGIN ==="
test_endpoint "POST" "/api/auth/register" "Register new user" "" '{
    "email": "test@example.com",
    "password": "TestPassword123!",
    "firstName": "Test",
    "lastName": "User",
    "role": "user",
    "phoneNumber": "555-0123"
}'

test_endpoint "POST" "/api/auth/login" "User login" "" '{
    "email": "test@example.com",
    "password": "TestPassword123!"
}'

test_endpoint "POST" "/api/auth/create-first-admin" "Create first admin user" "" '{
    "email": "admin@example.com",
    "password": "AdminPassword123!",
    "firstName": "Admin",
    "lastName": "User",
    "role": "admin",
    "phoneNumber": "555-0124"
}'

echo "=== PUBLIC CASES ENDPOINTS ==="
test_endpoint "GET" "/api/publiccases" "Get public cases with pagination"
test_endpoint "GET" "/api/publiccases/stats/houston" "Get Houston area statistics"

echo "=== RUNNERS ENDPOINTS ==="
test_endpoint "GET" "/api/runners" "Get all runners with optional filtering"
test_endpoint "POST" "/api/runners/init-table" "Initialize runners table"
test_endpoint "POST" "/api/runners/create-sample-cases" "Create sample Houston NamUs cases"

echo "=== ADMIN ENDPOINTS (require authentication) ==="
test_endpoint "GET" "/api/admin/users" "Get all users (admin only)" "Authorization: Bearer FAKE_TOKEN"
test_endpoint "GET" "/api/admin/stats" "Get system statistics (admin only)" "Authorization: Bearer FAKE_TOKEN"

echo "=== IMAGE UPLOAD ENDPOINTS (require authentication) ==="
test_endpoint "GET" "/api/imageupload/entity/user/1" "Get images for user entity" "Authorization: Bearer FAKE_TOKEN"

echo ""
echo "=== TESTING COMPLETE ==="
echo "Note: Database-dependent endpoints will show connection errors until database is configured."
echo "Non-database endpoints (like /api/auth/test) should work properly."
