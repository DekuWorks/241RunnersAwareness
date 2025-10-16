#!/usr/bin/env node

/**
 * Comprehensive API Endpoint Testing Script
 * Tests all CRUD operations, authentication, and admin functionality
 */

const https = require('https');

const API_BASE_URL = 'https://241runners-api-v2.azurewebsites.net/api/v1';

// Test configuration
const ADMIN_EMAIL = 'dekuworks1@gmail.com';
const ADMIN_PASSWORD = 'marcus2025';

// Test results tracking
const testResults = {
    passed: 0,
    failed: 0,
    errors: []
};

// Helper function to make API requests
function makeRequest(method, endpoint, data = null, token = null) {
    return new Promise((resolve, reject) => {
        const url = new URL(`${API_BASE_URL}${endpoint}`);
        
        const options = {
            hostname: url.hostname,
            port: 443,
            path: url.pathname + url.search,
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'X-Client': 'EndpointTester/1.0'
            }
        };

        if (token) {
            options.headers['Authorization'] = `Bearer ${token}`;
        }

        const req = https.request(options, (res) => {
            let body = '';
            res.on('data', (chunk) => body += chunk);
            res.on('end', () => {
                try {
                    const response = JSON.parse(body);
                    resolve({ 
                        status: res.statusCode, 
                        data: response,
                        headers: res.headers
                    });
                } catch (e) {
                    resolve({ 
                        status: res.statusCode, 
                        data: body,
                        headers: res.headers
                    });
                }
            });
        });

        req.on('error', reject);
        req.setTimeout(30000, () => {
            req.destroy();
            reject(new Error('Request timeout'));
        });

        if (data) {
            req.write(JSON.stringify(data));
        }

        req.end();
    });
}

// Test result tracking
function recordTest(testName, success, details = '') {
    if (success) {
        testResults.passed++;
        console.log(`✅ ${testName} - PASSED ${details}`);
    } else {
        testResults.failed++;
        testResults.errors.push(`${testName}: ${details}`);
        console.log(`❌ ${testName} - FAILED ${details}`);
    }
}

// Login as admin
async function loginAsAdmin() {
    console.log('\n🔐 Testing Authentication Endpoints...\n');
    
    try {
        const response = await makeRequest('POST', '/auth/login', {
            email: ADMIN_EMAIL,
            password: ADMIN_PASSWORD
        });

        const success = response.status === 200 && response.data.success;
        recordTest('Admin Login', success, success ? '' : `Status: ${response.status}, Data: ${JSON.stringify(response.data)}`);
        
        return success ? (response.data.accessToken || response.data.token) : null;
    } catch (error) {
        recordTest('Admin Login', false, error.message);
        return null;
    }
}

// Test authentication endpoints
async function testAuthEndpoints() {
    console.log('📋 Testing Auth Endpoints...');
    
    // Test health endpoint
    try {
        const response = await makeRequest('GET', '/auth/health');
        recordTest('Auth Health Check', response.status === 200);
    } catch (error) {
        recordTest('Auth Health Check', false, error.message);
    }
    
    // Test roles endpoint
    try {
        const response = await makeRequest('GET', '/auth/roles');
        const success = response.status === 200 && response.data.success && response.data.roles;
        recordTest('Get Available Roles', success, success ? `Found ${response.data.roles.length} roles` : `Status: ${response.status}`);
    } catch (error) {
        recordTest('Get Available Roles', false, error.message);
    }
    
    // Test invalid login
    try {
        const response = await makeRequest('POST', '/auth/login', {
            email: 'invalid@example.com',
            password: 'wrongpassword'
        });
        recordTest('Invalid Login Rejection', response.status === 401);
    } catch (error) {
        recordTest('Invalid Login Rejection', false, error.message);
    }
}

// Test admin user management endpoints
async function testAdminUserEndpoints(token) {
    if (!token) return;
    
    console.log('\n👥 Testing Admin User Management Endpoints...\n');
    
    // Get all users
    try {
        const response = await makeRequest('GET', '/Admin/users', null, token);
        const success = response.status === 200 && response.data.success;
        recordTest('Get All Users', success, success ? `Found ${response.data.users?.length || 0} users` : `Status: ${response.status}`);
    } catch (error) {
        recordTest('Get All Users', false, error.message);
    }
    
    // Test user creation via auth endpoint (since there's no direct admin user creation)
    let testUserId = null;
    try {
        const testUser = {
            email: 'test-crud-user@example.com',
            password: 'TestPassword123!',
            firstName: 'Test',
            lastName: 'User',
            role: 'user'
        };
        
        const response = await makeRequest('POST', '/auth/register', testUser);
        const success = response.status === 200 || response.status === 201;
        recordTest('Create Test User', success, success ? 'User created successfully' : `Status: ${response.status}`);
        
        if (success && response.data.user) {
            testUserId = response.data.user.id;
        }
    } catch (error) {
        recordTest('Create Test User', false, error.message);
    }
    
    // Test user update
    if (testUserId) {
        try {
            const updateData = {
                firstName: 'Updated',
                lastName: 'TestUser'
            };
            
            const response = await makeRequest('PUT', `/Admin/users/${testUserId}`, updateData, token);
            recordTest('Update User', response.status === 200, response.status === 200 ? 'User updated' : `Status: ${response.status}`);
        } catch (error) {
            recordTest('Update User', false, error.message);
        }
    }
    
    // Test user deletion
    if (testUserId) {
        try {
            const response = await makeRequest('DELETE', `/Admin/users/${testUserId}`, null, token);
            recordTest('Delete User', response.status === 200 || response.status === 204, response.status === 200 || response.status === 204 ? 'User deleted' : `Status: ${response.status}`);
        } catch (error) {
            recordTest('Delete User', false, error.message);
        }
    }
}

// Test cases endpoints
async function testCasesEndpoints(token) {
    if (!token) return;
    
    console.log('\n📋 Testing Cases Endpoints...\n');
    
    // Get all cases
    try {
        const response = await makeRequest('GET', '/cases', null, token);
        recordTest('Get All Cases', response.status === 200, `Status: ${response.status}`);
    } catch (error) {
        recordTest('Get All Cases', false, error.message);
    }
    
    // Test case creation
    let testCaseId = null;
    try {
        const testCase = {
            title: 'Test Case',
            description: 'Test case description',
            status: 'open',
            priority: 'medium',
            individualId: 'ind_1',  // Using existing runner ID 1
            lastSeenLocation: 'Test Location, Test City'
        };
        
        const response = await makeRequest('POST', '/cases', testCase, token);
        const success = response.status === 200 || response.status === 201;
        recordTest('Create Test Case', success, success ? 'Case created' : `Status: ${response.status} - ${JSON.stringify(response.data)}`);
        
        if (success && response.data.case) {
            testCaseId = response.data.case.id;
        }
    } catch (error) {
        recordTest('Create Test Case', false, error.message);
    }
    
    // Test case update
    if (testCaseId) {
        try {
            const updateData = {
                title: 'Updated Test Case',
                status: 'in_progress'
            };
            
            const response = await makeRequest('PUT', `/cases/${testCaseId}`, updateData, token);
            recordTest('Update Case', response.status === 200, response.status === 200 ? 'Case updated' : `Status: ${response.status}`);
        } catch (error) {
            recordTest('Update Case', false, error.message);
        }
    }
    
    // Test case deletion
    if (testCaseId) {
        try {
            const response = await makeRequest('DELETE', `/cases/${testCaseId}`, null, token);
            recordTest('Delete Case', response.status === 200 || response.status === 204, response.status === 200 || response.status === 204 ? 'Case deleted' : `Status: ${response.status}`);
        } catch (error) {
            recordTest('Delete Case', false, error.message);
        }
    }
}

// Test runners endpoints
async function testRunnersEndpoints(token) {
    if (!token) return;
    
    console.log('\n🏃 Testing Runners Endpoints...\n');
    
    // Get all runners
    try {
        const response = await makeRequest('GET', '/runner', null, token);
        recordTest('Get All Runners', response.status === 200, `Status: ${response.status}`);
    } catch (error) {
        recordTest('Get All Runners', false, error.message);
    }
    
    // Test runner creation
    let testRunnerId = null;
    try {
        const testRunner = {
            name: 'Test Runner',
            firstName: 'Test',
            lastName: 'Runner',
            age: 25,
            gender: 'Other',
            description: 'Test runner description',
            lastSeenLocation: 'Test Location',
            latitude: 40.7128,
            longitude: -74.0060
        };
        
        const response = await makeRequest('POST', '/runner', testRunner, token);
        const success = response.status === 200 || response.status === 201 || response.status === 409;
        recordTest('Create Test Runner', success, success ? (response.status === 409 ? 'Runner already exists (expected)' : 'Runner created') : `Status: ${response.status} - ${JSON.stringify(response.data)}`);
        
        if (success && response.data.runner) {
            testRunnerId = response.data.runner.id;
        }
    } catch (error) {
        recordTest('Create Test Runner', false, error.message);
    }
    
    // Test runner update
    if (testRunnerId) {
        try {
            const updateData = {
                firstName: 'Updated',
                lastName: 'TestRunner'
            };
            
            const response = await makeRequest('PUT', `/runner/${testRunnerId}`, updateData, token);
            recordTest('Update Runner', response.status === 200, response.status === 200 ? 'Runner updated' : `Status: ${response.status}`);
        } catch (error) {
            recordTest('Update Runner', false, error.message);
        }
    }
    
    // Test runner deletion
    if (testRunnerId) {
        try {
            const response = await makeRequest('DELETE', `/runner/${testRunnerId}`, null, token);
            recordTest('Delete Runner', response.status === 200 || response.status === 204, response.status === 200 || response.status === 204 ? 'Runner deleted' : `Status: ${response.status}`);
        } catch (error) {
            recordTest('Delete Runner', false, error.message);
        }
    }
}

// Test admin-specific endpoints
async function testAdminSpecificEndpoints(token) {
    if (!token) return;
    
    console.log('\n🔧 Testing Admin-Specific Endpoints...\n');
    
    // Test admin stats
    try {
        const response = await makeRequest('GET', '/Admin/stats', null, token);
        recordTest('Admin Stats', response.status === 200, `Status: ${response.status}`);
    } catch (error) {
        recordTest('Admin Stats', false, error.message);
    }
    
    // Test admin activity
    try {
        const response = await makeRequest('GET', '/Admin/activity', null, token);
        recordTest('Admin Activity', response.status === 200, `Status: ${response.status}`);
    } catch (error) {
        recordTest('Admin Activity', false, error.message);
    }
    
    // Test system status
    try {
        const response = await makeRequest('GET', '/Admin/system-status', null, token);
        recordTest('System Status', response.status === 200, `Status: ${response.status}`);
    } catch (error) {
        recordTest('System Status', false, error.message);
    }
    
    // Test monitoring data
    try {
        const response = await makeRequest('GET', '/Admin/monitoring-data', null, token);
        recordTest('Monitoring Data', response.status === 200, `Status: ${response.status}`);
    } catch (error) {
        recordTest('Monitoring Data', false, error.message);
    }
    
    // Test active sessions
    try {
        const response = await makeRequest('GET', '/Admin/active-sessions', null, token);
        recordTest('Active Sessions', response.status === 200, `Status: ${response.status}`);
    } catch (error) {
        recordTest('Active Sessions', false, error.message);
    }
}

// Main test function
async function runAllTests() {
    console.log('🚀 Starting Comprehensive API Endpoint Testing...\n');
    console.log(`🌐 Testing API: ${API_BASE_URL}\n`);
    
    const startTime = Date.now();
    
    // Test authentication endpoints
    await testAuthEndpoints();
    
    // Login as admin
    const token = await loginAsAdmin();
    
    // Test all admin endpoints
    await testAdminUserEndpoints(token);
    await testCasesEndpoints(token);
    await testRunnersEndpoints(token);
    await testAdminSpecificEndpoints(token);
    
    const endTime = Date.now();
    const duration = (endTime - startTime) / 1000;
    
    // Print summary
    console.log('\n' + '='.repeat(60));
    console.log('📊 TEST SUMMARY');
    console.log('='.repeat(60));
    console.log(`✅ Passed: ${testResults.passed}`);
    console.log(`❌ Failed: ${testResults.failed}`);
    console.log(`⏱️  Duration: ${duration.toFixed(2)}s`);
    console.log(`📈 Success Rate: ${((testResults.passed / (testResults.passed + testResults.failed)) * 100).toFixed(1)}%`);
    
    if (testResults.errors.length > 0) {
        console.log('\n❌ FAILED TESTS:');
        testResults.errors.forEach((error, index) => {
            console.log(`   ${index + 1}. ${error}`);
        });
    }
    
    console.log('\n🎯 RECOMMENDATIONS:');
    if (testResults.failed === 0) {
        console.log('   🎉 All tests passed! Your API is working perfectly.');
    } else {
        console.log('   🔧 Some endpoints need attention. Check the failed tests above.');
        console.log('   💡 Consider reviewing error logs and endpoint implementations.');
    }
    
    console.log('\n✨ Endpoint testing completed!');
}

// Run the tests
if (require.main === module) {
    runAllTests().catch(console.error);
}

module.exports = { runAllTests, testResults };
