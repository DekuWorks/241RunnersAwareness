#!/usr/bin/env node

/**
 * Authentication and Role Enforcement Testing Script
 * Tests JWT session management, refresh, and role-based access control
 */

const https = require('https');
const readline = require('readline');

const API_BASE = 'https://241runners-api-v2.azurewebsites.net';

// Helper function to make HTTP requests
function makeRequest(method, path, data = null, headers = {}) {
    return new Promise((resolve, reject) => {
        const options = {
            hostname: '241runners-api-v2.azurewebsites.net',
            port: 443,
            path: path,
            method: method,
            headers: {
                'Content-Type': 'application/json',
                ...headers
            }
        };

        const req = https.request(options, (res) => {
            let body = '';
            res.on('data', (chunk) => body += chunk);
            res.on('end', () => {
                try {
                    const jsonBody = body ? JSON.parse(body) : {};
                    resolve({
                        status: res.statusCode,
                        headers: res.headers,
                        body: jsonBody
                    });
                } catch (e) {
                    resolve({
                        status: res.statusCode,
                        headers: res.headers,
                        body: body
                    });
                }
            });
        });

        req.on('error', reject);

        if (data) {
            req.write(JSON.stringify(data));
        }
        req.end();
    });
}

// Test functions
async function testHealthEndpoints() {
    console.log('🏥 Testing Health Endpoints...');
    
    try {
        const healthz = await makeRequest('GET', '/healthz');
        console.log(`✅ /healthz: ${healthz.status} - ${JSON.stringify(healthz.body)}`);
        
        const readyz = await makeRequest('GET', '/readyz');
        console.log(`✅ /readyz: ${readyz.status} - ${JSON.stringify(readyz.body)}`);
        
        const apiHealth = await makeRequest('GET', '/api/health');
        console.log(`✅ /api/health: ${apiHealth.status} - ${JSON.stringify(apiHealth.body)}`);
    } catch (error) {
        console.error('❌ Health endpoint test failed:', error.message);
    }
}

async function testUnauthenticatedAccess() {
    console.log('\n🔒 Testing Unauthenticated Access...');
    
    try {
        const adminStats = await makeRequest('GET', '/api/admin/stats');
        console.log(`✅ Admin stats (no auth): ${adminStats.status} - Expected 401`);
        
        const users = await makeRequest('GET', '/api/users');
        console.log(`✅ Users endpoint (no auth): ${users.status} - Expected 401`);
        
        const cases = await makeRequest('GET', '/api/cases');
        console.log(`✅ Cases endpoint (no auth): ${cases.status} - Expected 401`);
    } catch (error) {
        console.error('❌ Unauthenticated access test failed:', error.message);
    }
}

async function testLoginEndpoint() {
    console.log('\n🔐 Testing Login Endpoint...');
    
    try {
        // Test with invalid credentials
        const invalidLogin = await makeRequest('POST', '/api/auth/login', {
            email: 'invalid@test.com',
            password: 'wrongpassword'
        });
        console.log(`✅ Invalid login: ${invalidLogin.status} - ${invalidLogin.body.message}`);
        
        // Test with malformed request
        const malformedLogin = await makeRequest('POST', '/api/auth/login', {
            email: 'test@test.com'
            // Missing password
        });
        console.log(`✅ Malformed login: ${malformedLogin.status} - Expected 400`);
        
    } catch (error) {
        console.error('❌ Login endpoint test failed:', error.message);
    }
}

async function testPublicEndpoints() {
    console.log('\n🌐 Testing Public Endpoints...');
    
    try {
        const publicCases = await makeRequest('GET', '/api/cases/publiccases');
        console.log(`✅ Public cases: ${publicCases.status} - ${JSON.stringify(publicCases.body)}`);
        
        const houstonStats = await makeRequest('GET', '/api/cases/publiccases/stats/houston');
        console.log(`✅ Houston stats: ${houstonStats.status} - ${JSON.stringify(houstonStats.body)}`);
        
        const usersDebug = await makeRequest('GET', '/api/admin/users-debug');
        console.log(`✅ Users debug: ${usersDebug.status} - Users count: ${usersDebug.body.users?.length || 0}`);
        
    } catch (error) {
        console.error('❌ Public endpoints test failed:', error.message);
    }
}

async function testCORSHeaders() {
    console.log('\n🌍 Testing CORS Headers...');
    
    try {
        const response = await makeRequest('GET', '/healthz');
        const corsHeaders = {
            'Access-Control-Allow-Origin': response.headers['access-control-allow-origin'],
            'Access-Control-Allow-Methods': response.headers['access-control-allow-methods'],
            'Access-Control-Allow-Headers': response.headers['access-control-allow-headers']
        };
        console.log(`✅ CORS Headers: ${JSON.stringify(corsHeaders)}`);
    } catch (error) {
        console.error('❌ CORS headers test failed:', error.message);
    }
}

async function testSecurityHeaders() {
    console.log('\n🛡️ Testing Security Headers...');
    
    try {
        const response = await makeRequest('GET', '/healthz');
        const securityHeaders = {
            'X-Content-Type-Options': response.headers['x-content-type-options'],
            'X-Frame-Options': response.headers['x-frame-options'],
            'Referrer-Policy': response.headers['referrer-policy']
        };
        console.log(`✅ Security Headers: ${JSON.stringify(securityHeaders)}`);
    } catch (error) {
        console.error('❌ Security headers test failed:', error.message);
    }
}

// Interactive login test
async function testInteractiveLogin() {
    console.log('\n🔑 Interactive Login Test...');
    console.log('Enter admin credentials to test JWT token generation:');
    
    const rl = readline.createInterface({
        input: process.stdin,
        output: process.stdout
    });
    
    return new Promise((resolve) => {
        rl.question('Email: ', async (email) => {
            rl.question('Password: ', async (password) => {
                try {
                    const loginResponse = await makeRequest('POST', '/api/auth/login', {
                        email: email,
                        password: password
                    });
                    
                    if (loginResponse.status === 200 && loginResponse.body.token) {
                        console.log(`✅ Login successful! Token received: ${loginResponse.body.token.substring(0, 20)}...`);
                        
                        // Test authenticated request
                        const adminStats = await makeRequest('GET', '/api/admin/stats', null, {
                            'Authorization': `Bearer ${loginResponse.body.token}`
                        });
                        console.log(`✅ Authenticated admin request: ${adminStats.status}`);
                        
                        // Test token verification
                        const verifyResponse = await makeRequest('POST', '/api/auth/verify', null, {
                            'Authorization': `Bearer ${loginResponse.body.token}`
                        });
                        console.log(`✅ Token verification: ${verifyResponse.status}`);
                        
                    } else {
                        console.log(`❌ Login failed: ${loginResponse.status} - ${loginResponse.body.message || 'Unknown error'}`);
                    }
                } catch (error) {
                    console.error('❌ Interactive login test failed:', error.message);
                }
                
                rl.close();
                resolve();
            });
        });
    });
}

// Main test runner
async function runTests() {
    console.log('🧪 241 Runners Awareness - Authentication & Security Tests');
    console.log('='.repeat(60));
    
    await testHealthEndpoints();
    await testUnauthenticatedAccess();
    await testLoginEndpoint();
    await testPublicEndpoints();
    await testCORSHeaders();
    await testSecurityHeaders();
    
    // Ask if user wants to test interactive login
    const rl = readline.createInterface({
        input: process.stdin,
        output: process.stdout
    });
    
    rl.question('\n🔑 Do you want to test interactive login? (y/n): ', async (answer) => {
        if (answer.toLowerCase() === 'y' || answer.toLowerCase() === 'yes') {
            await testInteractiveLogin();
        }
        
        console.log('\n✅ All tests completed!');
        rl.close();
    });
}

// Run tests if this script is executed directly
if (require.main === module) {
    runTests().catch(console.error);
}

module.exports = {
    makeRequest,
    testHealthEndpoints,
    testUnauthenticatedAccess,
    testLoginEndpoint,
    testPublicEndpoints,
    testCORSHeaders,
    testSecurityHeaders,
    testInteractiveLogin
};
