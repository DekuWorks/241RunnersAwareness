#!/usr/bin/env node

/**
 * Google SSO Integration Test Script
 * This script tests the Google SSO integration across different scenarios
 */

const https = require('https');
const http = require('http');

// Configuration
const config = {
  development: {
    frontendUrl: 'http://localhost:5173',
    backendUrl: 'http://localhost:5000',
    apiUrl: 'http://localhost:5000/api'
  },
  production: {
    frontendUrl: 'https://241runnersawareness.org',
    backendUrl: 'https://api.241runnersawareness.org',
    apiUrl: 'https://api.241runnersawareness.org/api'
  }
};

const environment = process.env.NODE_ENV || 'development';
const currentConfig = config[environment];

console.log(`🧪 Testing Google SSO Integration (${environment.toUpperCase()})`);
console.log('=' .repeat(50));

// Test functions
async function testBackendHealth() {
  console.log('\n1. Testing Backend Health...');
  
  return new Promise((resolve) => {
    const url = new URL(currentConfig.backendUrl);
    const client = url.protocol === 'https:' ? https : http;
    
    const req = client.get(url, (res) => {
      console.log(`✅ Backend is running (Status: ${res.statusCode})`);
      resolve(true);
    });
    
    req.on('error', (err) => {
      console.log(`❌ Backend is not accessible: ${err.message}`);
      resolve(false);
    });
    
    req.setTimeout(5000, () => {
      console.log('❌ Backend request timed out');
      req.destroy();
      resolve(false);
    });
  });
}

async function testSwaggerUI() {
  console.log('\n2. Testing Swagger UI...');
  
  return new Promise((resolve) => {
    const url = new URL(`${currentConfig.backendUrl}/swagger`);
    const client = url.protocol === 'https:' ? https : http;
    
    const req = client.get(url, (res) => {
      if (res.statusCode === 200) {
        console.log('✅ Swagger UI is accessible');
        resolve(true);
      } else {
        console.log(`❌ Swagger UI returned status: ${res.statusCode}`);
        resolve(false);
      }
    });
    
    req.on('error', (err) => {
      console.log(`❌ Swagger UI error: ${err.message}`);
      resolve(false);
    });
    
    req.setTimeout(5000, () => {
      console.log('❌ Swagger UI request timed out');
      req.destroy();
      resolve(false);
    });
  });
}

async function testCORSConfiguration() {
  console.log('\n3. Testing CORS Configuration...');
  
  return new Promise((resolve) => {
    const url = new URL(`${currentConfig.apiUrl}/auth/login`);
    const client = url.protocol === 'https:' ? https : http;
    
    const options = {
      hostname: url.hostname,
      port: url.port || (url.protocol === 'https:' ? 443 : 80),
      path: url.pathname,
      method: 'OPTIONS',
      headers: {
        'Origin': currentConfig.frontendUrl,
        'Access-Control-Request-Method': 'POST',
        'Access-Control-Request-Headers': 'Content-Type'
      }
    };
    
    const req = client.request(options, (res) => {
      const corsHeaders = res.headers['access-control-allow-origin'];
      if (corsHeaders && corsHeaders.includes(currentConfig.frontendUrl)) {
        console.log('✅ CORS is properly configured');
        resolve(true);
      } else {
        console.log('❌ CORS configuration issue');
        console.log(`   Expected: ${currentConfig.frontendUrl}`);
        console.log(`   Received: ${corsHeaders}`);
        resolve(false);
      }
    });
    
    req.on('error', (err) => {
      console.log(`❌ CORS test error: ${err.message}`);
      resolve(false);
    });
    
    req.setTimeout(5000, () => {
      console.log('❌ CORS test timed out');
      req.destroy();
      resolve(false);
    });
    
    req.end();
  });
}

async function testGoogleOAuthEndpoint() {
  console.log('\n4. Testing Google OAuth Endpoint...');
  
  return new Promise((resolve) => {
    const url = new URL(`${currentConfig.apiUrl}/auth/google-login`);
    const client = url.protocol === 'https:' ? https : http;
    
    const postData = JSON.stringify({
      IdToken: 'test-token'
    });
    
    const options = {
      hostname: url.hostname,
      port: url.port || (url.protocol === 'https:' ? 443 : 80),
      path: url.pathname,
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Content-Length': Buffer.byteLength(postData)
      }
    };
    
    const req = client.request(options, (res) => {
      let data = '';
      res.on('data', (chunk) => {
        data += chunk;
      });
      
      res.on('end', () => {
        try {
          const response = JSON.parse(data);
          if (res.statusCode === 400 && response.message === 'Invalid Google token.') {
            console.log('✅ Google OAuth endpoint is working (correctly rejected invalid token)');
            resolve(true);
          } else {
            console.log(`❌ Unexpected response: ${res.statusCode}`);
            console.log(`   Response: ${data}`);
            resolve(false);
          }
        } catch (err) {
          console.log(`❌ Invalid JSON response: ${err.message}`);
          resolve(false);
        }
      });
    });
    
    req.on('error', (err) => {
      console.log(`❌ Google OAuth test error: ${err.message}`);
      resolve(false);
    });
    
    req.setTimeout(5000, () => {
      console.log('❌ Google OAuth test timed out');
      req.destroy();
      resolve(false);
    });
    
    req.write(postData);
    req.end();
  });
}

async function testFrontendAccessibility() {
  console.log('\n5. Testing Frontend Accessibility...');
  
  return new Promise((resolve) => {
    const url = new URL(currentConfig.frontendUrl);
    const client = url.protocol === 'https:' ? https : http;
    
    const req = client.get(url, (res) => {
      if (res.statusCode === 200) {
        console.log('✅ Frontend is accessible');
        resolve(true);
      } else {
        console.log(`❌ Frontend returned status: ${res.statusCode}`);
        resolve(false);
      }
    });
    
    req.on('error', (err) => {
      console.log(`❌ Frontend error: ${err.message}`);
      resolve(false);
    });
    
    req.setTimeout(5000, () => {
      console.log('❌ Frontend request timed out');
      req.destroy();
      resolve(false);
    });
  });
}

// Main test runner
async function runTests() {
  const tests = [
    testBackendHealth,
    testSwaggerUI,
    testCORSConfiguration,
    testGoogleOAuthEndpoint,
    testFrontendAccessibility
  ];
  
  const results = [];
  
  for (const test of tests) {
    try {
      const result = await test();
      results.push(result);
    } catch (error) {
      console.log(`❌ Test failed with error: ${error.message}`);
      results.push(false);
    }
  }
  
  // Summary
  console.log('\n' + '=' .repeat(50));
  console.log('📊 TEST SUMMARY');
  console.log('=' .repeat(50));
  
  const passed = results.filter(r => r).length;
  const total = results.length;
  
  console.log(`✅ Passed: ${passed}/${total}`);
  console.log(`❌ Failed: ${total - passed}/${total}`);
  
  if (passed === total) {
    console.log('\n🎉 All tests passed! Google SSO integration is working correctly.');
    process.exit(0);
  } else {
    console.log('\n⚠️  Some tests failed. Please check the configuration and try again.');
    process.exit(1);
  }
}

// Run tests
runTests().catch(error => {
  console.error('💥 Test runner failed:', error);
  process.exit(1);
}); 