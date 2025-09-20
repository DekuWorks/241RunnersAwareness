#!/usr/bin/env node

/**
 * Comprehensive API Endpoint Tester for 241 Runners Awareness API
 * Tests all endpoints to ensure they are live and working
 */

const https = require('https');
const http = require('http');
const fs = require('fs');
const path = require('path');

// Configuration
const API_BASE_URL = 'https://241runners-api-v2.azurewebsites.net';
const TEST_RESULTS_FILE = 'api-test-results.json';

// Test results storage
let testResults = {
    timestamp: new Date().toISOString(),
    summary: {
        total: 0,
        passed: 0,
        failed: 0,
        skipped: 0
    },
    results: []
};

// Authentication token (will be set after login)
let authToken = null;
let testUserId = null;
let testRunnerId = null;
let testCaseId = null;

/**
 * Make HTTP request with proper error handling
 */
function makeRequest(url, options = {}) {
    return new Promise((resolve, reject) => {
        const isHttps = url.startsWith('https://');
        const client = isHttps ? https : http;
        
        const requestOptions = {
            method: options.method || 'GET',
            headers: {
                'Content-Type': 'application/json',
                'User-Agent': '241Runners-API-Tester/1.0',
                ...options.headers
            },
            timeout: 30000
        };

        const req = client.request(url, requestOptions, (res) => {
            let data = '';
            
            res.on('data', (chunk) => {
                data += chunk;
            });
            
            res.on('end', () => {
                try {
                    const responseData = data ? JSON.parse(data) : {};
                    resolve({
                        statusCode: res.statusCode,
                        headers: res.headers,
                        data: responseData
                    });
                } catch (e) {
                    resolve({
                        statusCode: res.statusCode,
                        headers: res.headers,
                        data: data
                    });
                }
            });
        });

        req.on('error', (error) => {
            reject(error);
        });

        req.on('timeout', () => {
            req.destroy();
            reject(new Error('Request timeout'));
        });

        if (options.body) {
            req.write(JSON.stringify(options.body));
        }

        req.end();
    });
}

/**
 * Test a single endpoint
 */
async function testEndpoint(name, method, path, options = {}) {
    const url = `${API_BASE_URL}${path}`;
    const startTime = Date.now();
    
    console.log(`\n🧪 Testing: ${method} ${path}`);
    
    try {
        const response = await makeRequest(url, {
            method,
            ...options
        });
        
        const duration = Date.now() - startTime;
        const success = response.statusCode >= 200 && response.statusCode < 300;
        
        const result = {
            name,
            method,
            path,
            url,
            statusCode: response.statusCode,
            success,
            duration,
            responseSize: JSON.stringify(response.data).length,
            timestamp: new Date().toISOString(),
            error: null
        };

        if (success) {
            console.log(`✅ ${name}: ${response.statusCode} (${duration}ms)`);
            testResults.summary.passed++;
        } else {
            console.log(`❌ ${name}: ${response.statusCode} (${duration}ms)`);
            console.log(`   Response: ${JSON.stringify(response.data).substring(0, 200)}...`);
            testResults.summary.failed++;
        }

        testResults.results.push(result);
        testResults.summary.total++;
        
        return { success, response, result };
        
    } catch (error) {
        const duration = Date.now() - startTime;
        console.log(`💥 ${name}: ERROR (${duration}ms) - ${error.message}`);
        
        const result = {
            name,
            method,
            path,
            url,
            statusCode: 0,
            success: false,
            duration,
            responseSize: 0,
            timestamp: new Date().toISOString(),
            error: error.message
        };

        testResults.results.push(result);
        testResults.summary.failed++;
        testResults.summary.total++;
        
        return { success: false, error, result };
    }
}

/**
 * Test authentication endpoints
 */
async function testAuthEndpoints() {
    console.log('\n🔐 Testing Authentication Endpoints...');
    
    // Test registration (will likely fail due to existing user, but tests endpoint)
    await testEndpoint(
        'User Registration',
        'POST',
        '/api/auth/register',
        {
            body: {
                email: 'test@example.com',
                password: 'TestPassword123!',
                firstName: 'Test',
                lastName: 'User'
            }
        }
    );

    // Test login with Marcus admin account
    const loginResult = await testEndpoint(
        'Admin Login',
        'POST',
        '/api/auth/login',
        {
            body: {
                email: 'dekuworks1@gmail.com',
                password: 'marcus2025'
            }
        }
    );

    if (loginResult.success && loginResult.response.data.accessToken) {
        authToken = loginResult.response.data.accessToken;
        testUserId = loginResult.response.data.user.id;
        console.log(`🔑 Authentication successful. Token obtained.`);
    }

    // Test token verification
    if (authToken) {
        await testEndpoint(
            'Token Verification',
            'POST',
            '/api/auth/verify',
            {
                headers: { 'Authorization': `Bearer ${authToken}` }
            }
        );

        // Test get current user
        await testEndpoint(
            'Get Current User',
            'GET',
            '/api/auth/me',
            {
                headers: { 'Authorization': `Bearer ${authToken}` }
            }
        );

        // Test logout
        await testEndpoint(
            'Logout',
            'POST',
            '/api/auth/logout',
            {
                headers: { 'Authorization': `Bearer ${authToken}` }
            }
        );
    }
}

/**
 * Test admin endpoints
 */
async function testAdminEndpoints() {
    if (!authToken) {
        console.log('⚠️  Skipping admin endpoints - no auth token');
        return;
    }

    console.log('\n👑 Testing Admin Endpoints...');
    
    const headers = { 'Authorization': `Bearer ${authToken}` };

    // Test admin stats
    await testEndpoint(
        'Admin Stats',
        'GET',
        '/api/admin/stats',
        { headers }
    );

    // Test get all users
    await testEndpoint(
        'Get All Users',
        'GET',
        '/api/admin/users',
        { headers }
    );

    // Test get admins
    await testEndpoint(
        'Get Admins',
        'GET',
        '/api/admin/admins',
        { headers }
    );

    // Test admin activity
    await testEndpoint(
        'Admin Activity',
        'GET',
        '/api/admin/activity',
        { headers }
    );

    // Test data version
    await testEndpoint(
        'Data Version',
        'GET',
        '/api/admin/data-version',
        { headers }
    );

    // Test create cases for runners
    await testEndpoint(
        'Create Cases for Runners',
        'POST',
        '/api/admin/create-cases-for-runners',
        { headers }
    );
}

/**
 * Test cases endpoints
 */
async function testCasesEndpoints() {
    if (!authToken) {
        console.log('⚠️  Skipping cases endpoints - no auth token');
        return;
    }

    console.log('\n📋 Testing Cases Endpoints...');
    
    const headers = { 'Authorization': `Bearer ${authToken}` };

    // Test get cases
    const casesResult = await testEndpoint(
        'Get Cases',
        'GET',
        '/api/cases',
        { headers }
    );

    // Test get public cases (no auth required)
    await testEndpoint(
        'Get Public Cases',
        'GET',
        '/api/cases/publiccases'
    );

    // Test get public cases stats
    await testEndpoint(
        'Get Public Cases Stats',
        'GET',
        '/api/cases/publiccases/stats/houston'
    );

    // If we have cases, test getting a specific case
    if (casesResult.success && casesResult.response.data.data && casesResult.response.data.data.length > 0) {
        const firstCase = casesResult.response.data.data[0];
        testCaseId = firstCase.id;
        
        await testEndpoint(
            'Get Case by ID',
            'GET',
            `/api/cases/${testCaseId}`,
            { headers }
        );
    }
}

/**
 * Test individuals endpoints
 */
async function testIndividualsEndpoints() {
    if (!authToken) {
        console.log('⚠️  Skipping individuals endpoints - no auth token');
        return;
    }

    console.log('\n👥 Testing Individuals Endpoints...');
    
    const headers = { 'Authorization': `Bearer ${authToken}` };

    // Test get individuals
    const individualsResult = await testEndpoint(
        'Get Individuals',
        'GET',
        '/api/individuals',
        { headers }
    );

    // If we have individuals, test getting a specific individual
    if (individualsResult.success && individualsResult.response.data.data && individualsResult.response.data.data.length > 0) {
        const firstIndividual = individualsResult.response.data.data[0];
        testRunnerId = firstIndividual.id;
        
        await testEndpoint(
            'Get Individual by ID',
            'GET',
            `/api/individuals/${testRunnerId}`,
            { headers }
        );
    }
}

/**
 * Test runner endpoints
 */
async function testRunnerEndpoints() {
    if (!authToken) {
        console.log('⚠️  Skipping runner endpoints - no auth token');
        return;
    }

    console.log('\n🏃 Testing Runner Endpoints...');
    
    const headers = { 'Authorization': `Bearer ${authToken}` };

    // Test get all runners
    await testEndpoint(
        'Get All Runners',
        'GET',
        '/api/runner',
        { headers }
    );

    // Test get my runner profile
    await testEndpoint(
        'Get My Runner Profile',
        'GET',
        '/api/runner/my-profile',
        { headers }
    );

    // Test get my cases
    await testEndpoint(
        'Get My Cases',
        'GET',
        '/api/runner/my-cases',
        { headers }
    );

    // If we have a runner ID, test specific runner endpoints
    if (testRunnerId) {
        const runnerId = testRunnerId.replace('ind_', '');
        
        await testEndpoint(
            'Get Runner by ID',
            'GET',
            `/api/runner/${runnerId}`,
            { headers }
        );
    }
}

/**
 * Test users endpoints
 */
async function testUsersEndpoints() {
    if (!authToken) {
        console.log('⚠️  Skipping users endpoints - no auth token');
        return;
    }

    console.log('\n👤 Testing Users Endpoints...');
    
    const headers = { 'Authorization': `Bearer ${authToken}` };

    // Test get users
    await testEndpoint(
        'Get Users',
        'GET',
        '/api/users',
        { headers }
    );
}

/**
 * Test map endpoints
 */
async function testMapEndpoints() {
    console.log('\n🗺️  Testing Map Endpoints...');
    
    // Test get map points (public)
    await testEndpoint(
        'Get Map Points',
        'GET',
        '/api/map/points'
    );

    // Test get map points with clustering
    await testEndpoint(
        'Get Map Points (Clustered)',
        'GET',
        '/api/map/points?cluster=true'
    );

    // Test get raw map points (requires auth)
    if (authToken) {
        await testEndpoint(
            'Get Raw Map Points',
            'GET',
            '/api/map/points/raw',
            { headers: { 'Authorization': `Bearer ${authToken}` } }
        );
    }
}

/**
 * Test notifications endpoints
 */
async function testNotificationsEndpoints() {
    if (!authToken) {
        console.log('⚠️  Skipping notifications endpoints - no auth token');
        return;
    }

    console.log('\n🔔 Testing Notifications Endpoints...');
    
    const headers = { 'Authorization': `Bearer ${authToken}` };

    // Test get notifications
    await testEndpoint(
        'Get Notifications',
        'GET',
        '/api/notifications',
        { headers }
    );

    // Test create notification
    await testEndpoint(
        'Create Notification',
        'POST',
        '/api/notifications',
        {
            headers,
            body: {
                userId: testUserId || 'u_1',
                type: 'TEST',
                title: 'Test Notification',
                body: 'This is a test notification'
            }
        }
    );

    // Test mark notification as read
    await testEndpoint(
        'Mark Notification as Read',
        'POST',
        '/api/notifications/n_1/read',
        { headers }
    );
}

/**
 * Test image upload endpoints
 */
async function testImageUploadEndpoints() {
    console.log('\n📸 Testing Image Upload Endpoints...');
    
    // Test get image (will likely fail as no images exist)
    await testEndpoint(
        'Get Image',
        'GET',
        '/api/imageupload/test-image.jpg'
    );
}

/**
 * Test health and status endpoints
 */
async function testHealthEndpoints() {
    console.log('\n🏥 Testing Health Endpoints...');
    
    // Test root endpoint
    await testEndpoint(
        'Root Endpoint',
        'GET',
        '/'
    );

    // Test API root
    await testEndpoint(
        'API Root',
        'GET',
        '/api'
    );
}

/**
 * Save test results to file
 */
function saveTestResults() {
    try {
        fs.writeFileSync(TEST_RESULTS_FILE, JSON.stringify(testResults, null, 2));
        console.log(`\n📊 Test results saved to: ${TEST_RESULTS_FILE}`);
    } catch (error) {
        console.error('Error saving test results:', error.message);
    }
}

/**
 * Print summary
 */
function printSummary() {
    console.log('\n' + '='.repeat(60));
    console.log('📊 API ENDPOINT TEST SUMMARY');
    console.log('='.repeat(60));
    console.log(`Total Tests: ${testResults.summary.total}`);
    console.log(`✅ Passed: ${testResults.summary.passed}`);
    console.log(`❌ Failed: ${testResults.summary.failed}`);
    console.log(`⏭️  Skipped: ${testResults.summary.skipped}`);
    console.log(`Success Rate: ${((testResults.summary.passed / testResults.summary.total) * 100).toFixed(1)}%`);
    console.log('='.repeat(60));

    if (testResults.summary.failed > 0) {
        console.log('\n❌ FAILED TESTS:');
        testResults.results
            .filter(r => !r.success)
            .forEach(r => {
                console.log(`  • ${r.name}: ${r.statusCode} - ${r.error || 'HTTP Error'}`);
            });
    }

    console.log('\n🎯 RECOMMENDATIONS:');
    if (testResults.summary.failed === 0) {
        console.log('  • All endpoints are working correctly! 🎉');
    } else {
        console.log('  • Review failed endpoints and check server logs');
        console.log('  • Ensure database connections are working');
        console.log('  • Verify authentication tokens are valid');
    }
}

/**
 * Main test runner
 */
async function runAllTests() {
    console.log('🚀 Starting 241 Runners Awareness API Endpoint Tests');
    console.log(`🌐 Testing API at: ${API_BASE_URL}`);
    console.log(`⏰ Started at: ${new Date().toISOString()}`);
    
    try {
        // Run all test suites
        await testHealthEndpoints();
        await testAuthEndpoints();
        await testAdminEndpoints();
        await testCasesEndpoints();
        await testIndividualsEndpoints();
        await testRunnerEndpoints();
        await testUsersEndpoints();
        await testMapEndpoints();
        await testNotificationsEndpoints();
        await testImageUploadEndpoints();
        
        // Save results and print summary
        saveTestResults();
        printSummary();
        
    } catch (error) {
        console.error('💥 Test runner error:', error.message);
        process.exit(1);
    }
}

// Run tests if this script is executed directly
if (require.main === module) {
    runAllTests().catch(console.error);
}

module.exports = {
    runAllTests,
    testEndpoint,
    makeRequest
};
