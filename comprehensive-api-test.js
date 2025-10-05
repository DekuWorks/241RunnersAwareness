#!/usr/bin/env node

const https = require('https');
const http = require('http');

// Configuration
const API_BASE_URL = 'https://241runners-api-v2.azurewebsites.net';
const TEST_CREDENTIALS = {
  user: { email: 'test@example.com', password: 'TestPassword123!' },
  admin: { email: 'dekuworks1@gmail.com', password: 'marcus2025' }
};

// Colors for console output
const colors = {
  reset: '\x1b[0m',
  bright: '\x1b[1m',
  red: '\x1b[31m',
  green: '\x1b[32m',
  yellow: '\x1b[33m',
  blue: '\x1b[34m',
  magenta: '\x1b[35m',
  cyan: '\x1b[36m'
};

// Helper function to make HTTP requests
function makeRequest(url, options = {}) {
  return new Promise((resolve, reject) => {
    const urlObj = new URL(url);
    const requestOptions = {
      hostname: urlObj.hostname,
      port: urlObj.port || (urlObj.protocol === 'https:' ? 443 : 80),
      path: urlObj.pathname + urlObj.search,
      method: options.method || 'GET',
      headers: {
        'Content-Type': 'application/json',
        ...options.headers
      }
    };

    const req = (urlObj.protocol === 'https:' ? https : http).request(requestOptions, (res) => {
      let data = '';
      res.on('data', chunk => data += chunk);
      res.on('end', () => {
        try {
          const jsonData = data ? JSON.parse(data) : null;
          resolve({
            status: res.statusCode,
            statusText: res.statusMessage,
            headers: res.headers,
            data: jsonData,
            rawData: data
          });
        } catch (e) {
          resolve({
            status: res.statusCode,
            statusText: res.statusMessage,
            headers: res.headers,
            data: null,
            rawData: data
          });
        }
      });
    });

    req.on('error', reject);
    
    if (options.body) {
      req.write(typeof options.body === 'string' ? options.body : JSON.stringify(options.body));
    }
    
    req.end();
  });
}

// Test result tracking
const testResults = {
  passed: 0,
  failed: 0,
  total: 0,
  details: []
};

function logTest(name, status, message, details = null) {
  testResults.total++;
  if (status === 'PASS') {
    testResults.passed++;
    console.log(`${colors.green}✓${colors.reset} ${colors.bright}${name}${colors.reset} - ${message}`);
  } else {
    testResults.failed++;
    console.log(`${colors.red}✗${colors.reset} ${colors.bright}${name}${colors.reset} - ${message}`);
  }
  
  testResults.details.push({ name, status, message, details });
}

async function testEndpoint(name, url, options = {}, expectedStatus = 200) {
  try {
    const response = await makeRequest(url, options);
    const success = response.status === expectedStatus;
    
    if (success) {
      logTest(name, 'PASS', `Status: ${response.status} ${response.statusText}`);
    } else {
      logTest(name, 'FAIL', `Expected ${expectedStatus}, got ${response.status} ${response.statusText}`, response.data);
    }
    
    return { success, response };
  } catch (error) {
    logTest(name, 'FAIL', `Error: ${error.message}`);
    return { success: false, error };
  }
}

async function runComprehensiveTests() {
  console.log(`${colors.cyan}${colors.bright}🔍 241 Runners API - Comprehensive Test Suite${colors.reset}\n`);
  
  // Test 1: Health Check
  console.log(`${colors.blue}📊 Health & System Endpoints${colors.reset}`);
  await testEndpoint('Health Check', `${API_BASE_URL}/health`);
  await testEndpoint('Swagger UI', `${API_BASE_URL}/swagger`, {}, 200);
  await testEndpoint('API Info', `${API_BASE_URL}/api/info`);
  
  // Test 2: Authentication
  console.log(`\n${colors.blue}🔐 Authentication Endpoints${colors.reset}`);
  
  // User Login
  const userLoginResult = await testEndpoint(
    'User Login', 
    `${API_BASE_URL}/api/v1.0/auth/login`,
    {
      method: 'POST',
      body: TEST_CREDENTIALS.user
    }
  );
  
  let userToken = null;
  if (userLoginResult.success && userLoginResult.response.data) {
    userToken = userLoginResult.response.data.accessToken;
  }
  
  // Admin Login
  const adminLoginResult = await testEndpoint(
    'Admin Login', 
    `${API_BASE_URL}/api/v1.0/auth/login`,
    {
      method: 'POST',
      body: TEST_CREDENTIALS.admin
    }
  );
  
  let adminToken = null;
  if (adminLoginResult.success && adminLoginResult.response.data) {
    adminToken = adminLoginResult.response.data.accessToken;
  }
  
  // Test 3: User Endpoints (with authentication)
  console.log(`\n${colors.blue}👤 User Endpoints${colors.reset}`);
  
  if (userToken) {
    await testEndpoint(
      'Get Current User',
      `${API_BASE_URL}/api/v1.0/users/me`,
      { headers: { 'Authorization': `Bearer ${userToken}` } }
    );
    
    await testEndpoint(
      'Update User Profile',
      `${API_BASE_URL}/api/v1.0/users/me`,
      {
        method: 'PUT',
        headers: { 'Authorization': `Bearer ${userToken}` },
        body: { firstName: 'Test', lastName: 'User' }
      }
    );
  } else {
    logTest('User Endpoints', 'SKIP', 'No user token available');
  }
  
  // Test 4: Runner Endpoints
  console.log(`\n${colors.blue}🏃 Runner Endpoints${colors.reset}`);
  
  if (userToken) {
    await testEndpoint(
      'Get Runners',
      `${API_BASE_URL}/api/v1.0/Runner`,
      { headers: { 'Authorization': `Bearer ${userToken}` } }
    );
    
    await testEndpoint(
      'Get My Runner Profile',
      `${API_BASE_URL}/api/v1.0/Runner/my-profile`,
      { headers: { 'Authorization': `Bearer ${userToken}` } }
    );
    
    await testEndpoint(
      'Create Runner Profile',
      `${API_BASE_URL}/api/v1.0/Runner`,
      {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${userToken}` },
        body: {
          name: 'Test Runner',
          dateOfBirth: '1990-01-01',
          gender: 'Male',
          status: 'Missing',
          physicalDescription: 'Test description',
          userId: 33
        }
      }
    );
  } else {
    logTest('Runner Endpoints', 'SKIP', 'No user token available');
  }
  
  // Test 5: Admin Endpoints
  console.log(`\n${colors.blue}👑 Admin Endpoints${colors.reset}`);
  
  if (adminToken) {
    await testEndpoint(
      'Get All Users',
      `${API_BASE_URL}/api/v1.0/admin/users`,
      { headers: { 'Authorization': `Bearer ${adminToken}` } }
    );
    
    await testEndpoint(
      'Get System Status',
      `${API_BASE_URL}/api/v1.0/admin/system-status`,
      { headers: { 'Authorization': `Bearer ${adminToken}` } }
    );
    
    await testEndpoint(
      'Get Monitoring Data',
      `${API_BASE_URL}/api/v1.0/admin/monitoring`,
      { headers: { 'Authorization': `Bearer ${adminToken}` } }
    );
    
    await testEndpoint(
      'Get Active Sessions',
      `${API_BASE_URL}/api/v1.0/admin/sessions`,
      { headers: { 'Authorization': `Bearer ${adminToken}` } }
    );
  } else {
    logTest('Admin Endpoints', 'SKIP', 'No admin token available');
  }
  
  // Test 6: Case Endpoints
  console.log(`\n${colors.blue}📋 Case Endpoints${colors.reset}`);
  
  if (userToken) {
    await testEndpoint(
      'Get Public Cases',
      `${API_BASE_URL}/api/v1.0/cases/public`
    );
    
    await testEndpoint(
      'Get My Cases',
      `${API_BASE_URL}/api/v1.0/cases/my-cases`,
      { headers: { 'Authorization': `Bearer ${userToken}` } }
    );
  } else {
    logTest('Case Endpoints', 'SKIP', 'No user token available');
  }
  
  // Test 7: SignalR Hub
  console.log(`\n${colors.blue}🔌 SignalR Hub${colors.reset}`);
  
  try {
    const hubResponse = await makeRequest(`${API_BASE_URL}/hubs/user/negotiate`);
    if (hubResponse.status === 200 || hubResponse.status === 401) {
      logTest('SignalR Hub Negotiate', 'PASS', `Status: ${hubResponse.status} (Hub is accessible)`);
    } else {
      logTest('SignalR Hub Negotiate', 'FAIL', `Status: ${hubResponse.status}`);
    }
  } catch (error) {
    logTest('SignalR Hub Negotiate', 'FAIL', `Error: ${error.message}`);
  }
  
  // Test 8: Error Handling
  console.log(`\n${colors.blue}⚠️ Error Handling${colors.reset}`);
  
  await testEndpoint(
    'Invalid Endpoint',
    `${API_BASE_URL}/api/invalid-endpoint`,
    {},
    404
  );
  
  await testEndpoint(
    'Unauthorized Access',
    `${API_BASE_URL}/api/v1.0/users/me`,
    {},
    401
  );
  
  // Summary
  console.log(`\n${colors.cyan}${colors.bright}📊 Test Summary${colors.reset}`);
  console.log(`${colors.green}✓ Passed: ${testResults.passed}${colors.reset}`);
  console.log(`${colors.red}✗ Failed: ${testResults.failed}${colors.reset}`);
  console.log(`${colors.blue}📊 Total: ${testResults.total}${colors.reset}`);
  
  const successRate = ((testResults.passed / testResults.total) * 100).toFixed(1);
  console.log(`${colors.magenta}📈 Success Rate: ${successRate}%${colors.reset}`);
  
  if (testResults.failed > 0) {
    console.log(`\n${colors.yellow}⚠️ Failed Tests:${colors.reset}`);
    testResults.details
      .filter(test => test.status === 'FAIL')
      .forEach(test => {
        console.log(`  • ${test.name}: ${test.message}`);
        if (test.details) {
          console.log(`    Details: ${JSON.stringify(test.details, null, 2)}`);
        }
      });
  }
  
  console.log(`\n${colors.cyan}🎯 API Testing Complete!${colors.reset}`);
}

// Run the tests
runComprehensiveTests().catch(console.error);