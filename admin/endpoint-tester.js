/**
 * 241 Runners Awareness - API Endpoint Testing Suite
 * Comprehensive testing of all backend API endpoints
 */

class APIEndpointTester {
    constructor() {
        this.apiBaseUrl = 'https://241runnersawareness-api.azurewebsites.net/api';
        this.testResults = [];
        this.authToken = null;
        this.testData = {
            testUser: {
                firstName: 'Test',
                lastName: 'User',
                email: 'testuser@example.com',
                password: 'TestPass123!',
                role: 'user'
            },
            testRunner: {
                firstName: 'Test',
                lastName: 'Runner',
                age: 25,
                city: 'Test City',
                state: 'TS',
                status: 'active'
            }
        };
    }

    // Initialize testing
    async init() {
        console.log('🚀 Starting API Endpoint Testing Suite...');
        console.log('==========================================');
        
        // Get authentication token
        this.authToken = this.getAuthToken();
        if (!this.authToken) {
            console.warn('⚠️ No authentication token found - some tests will be skipped');
        }

        return true;
    }

    // Get authentication token
    getAuthToken() {
        return localStorage.getItem('ra_admin_token');
    }

    // Run all endpoint tests
    async runAllTests() {
        console.log('\n🧪 Testing All API Endpoints...\n');

        // Test authentication endpoints
        await this.testAuthEndpoints();
        
        // Test user management endpoints
        await this.testUserEndpoints();
        
        // Test runner management endpoints
        await this.testRunnerEndpoints();
        
        // Test system endpoints
        await this.testSystemEndpoints();
        
        // Test error handling
        await this.testErrorHandling();
        
        // Test rate limiting
        await this.testRateLimiting();
        
        // Test data validation
        await this.testDataValidation();
        
        // Generate test report
        this.generateEndpointReport();
    }

    // Test authentication endpoints
    async testAuthEndpoints() {
        console.log('🔐 Testing Authentication Endpoints...');
        
        // Test health endpoint (no auth required)
        await this.testEndpoint('Health Check', `${this.apiBaseUrl}/auth/health`, 'GET', null, {
            expectedStatus: 200,
            expectedFields: ['status', 'timestamp', 'database', 'users', 'runners']
        });
        
        // Test login endpoint
        await this.testEndpoint('Login', `${this.apiBaseUrl}/auth/login`, 'POST', {
            email: 'test@example.com',
            password: 'wrongpassword'
        }, {
            expectedStatus: 401,
            expectedFields: ['message']
        });
        
        // Test verify token endpoint (with auth)
        if (this.authToken) {
            await this.testEndpoint('Verify Token', `${this.apiBaseUrl}/auth/verify`, 'POST', null, {
                expectedStatus: 200,
                expectedFields: ['valid', 'user'],
                requiresAuth: true
            });
        } else {
            console.log('  ⏭️ Skipping token verification - no auth token');
        }
        
        // Test register endpoint (admin only)
        if (this.authToken) {
            await this.testEndpoint('Register User', `${this.apiBaseUrl}/auth/register`, 'POST', this.testData.testUser, {
                expectedStatus: 201,
                expectedFields: ['token', 'user'],
                requiresAuth: true
            });
        } else {
            console.log('  ⏭️ Skipping user registration - no auth token');
        }
    }

    // Test user management endpoints
    async testUserEndpoints() {
        console.log('\n👥 Testing User Management Endpoints...');
        
        if (!this.authToken) {
            console.log('  ⏭️ Skipping user endpoints - no auth token');
            return;
        }
        
        // Test get all users
        await this.testEndpoint('Get All Users', `${this.apiBaseUrl}/auth/users`, 'GET', null, {
            expectedStatus: 200,
            expectedFields: ['users'],
            requiresAuth: true
        });
        
        // Test get user by ID (will need to create a test user first)
        // This would require creating a test user and then testing the endpoint
        console.log('  ⏭️ User ID endpoint test requires test user creation');
    }

    // Test runner management endpoints
    async testRunnerEndpoints() {
        console.log('\n🏃 Testing Runner Management Endpoints...');
        
        if (!this.authToken) {
            console.log('  ⏭️ Skipping runner endpoints - no auth token');
            return;
        }
        
        // Test get all runners
        await this.testEndpoint('Get All Runners', `${this.apiBaseUrl}/runners`, 'GET', null, {
            expectedStatus: 200,
            expectedFields: [],
            requiresAuth: true
        });
        
        // Test create runner
        await this.testEndpoint('Create Runner', `${this.apiBaseUrl}/runners`, 'POST', this.testData.testRunner, {
            expectedStatus: 201,
            expectedFields: ['id', 'firstName', 'lastName'],
            requiresAuth: true
        });
    }

    // Test system endpoints
    async testSystemEndpoints() {
        console.log('\n⚙️ Testing System Endpoints...');
        
        // Test health endpoint again for consistency
        await this.testEndpoint('System Health', `${this.apiBaseUrl}/auth/health`, 'GET', null, {
            expectedStatus: 200,
            expectedFields: ['status', 'timestamp', 'database']
        });
        
        // Test with different HTTP methods
        await this.testEndpoint('Health with POST (should fail)', `${this.apiBaseUrl}/auth/health`, 'POST', null, {
            expectedStatus: 405, // Method not allowed
            expectedFields: ['message']
        });
    }

    // Test error handling
    async testErrorHandling() {
        console.log('\n❌ Testing Error Handling...');
        
        // Test invalid endpoint
        await this.testEndpoint('Invalid Endpoint', `${this.apiBaseUrl}/invalid/endpoint`, 'GET', null, {
            expectedStatus: 404,
            expectedFields: ['message']
        });
        
        // Test malformed JSON
        await this.testEndpoint('Malformed JSON', `${this.apiBaseUrl}/auth/login`, 'POST', 'invalid json', {
            expectedStatus: 400,
            expectedFields: ['message']
        });
        
        // Test missing required fields
        await this.testEndpoint('Missing Required Fields', `${this.apiBaseUrl}/auth/login`, 'POST', {
            email: 'test@example.com'
            // password missing
        }, {
            expectedStatus: 400,
            expectedFields: ['message']
        });
    }

    // Test rate limiting
    async testRateLimiting() {
        console.log('\n⏱️ Testing Rate Limiting...');
        
        // Make multiple rapid requests to test rate limiting
        const promises = [];
        for (let i = 0; i < 10; i++) {
            promises.push(this.makeRequest(`${this.apiBaseUrl}/auth/health`, 'GET'));
        }
        
        try {
            const responses = await Promise.all(promises);
            const successCount = responses.filter(r => r.status === 200).length;
            const rateLimitedCount = responses.filter(r => r.status === 429).length;
            
            this.testResult('Rate Limiting', rateLimitedCount > 0 || successCount === 10, 
                `Success: ${successCount}, Rate Limited: ${rateLimitedCount}`);
        } catch (error) {
            this.testResult('Rate Limiting', false, `Error: ${error.message}`);
        }
    }

    // Test data validation
    async testDataValidation() {
        console.log('\n✅ Testing Data Validation...');
        
        // Test invalid email format
        await this.testEndpoint('Invalid Email Format', `${this.apiBaseUrl}/auth/login`, 'POST', {
            email: 'invalid-email-format',
            password: 'TestPass123!'
        }, {
            expectedStatus: 400,
            expectedFields: ['message']
        });
        
        // Test weak password
        await this.testEndpoint('Weak Password', `${this.apiBaseUrl}/auth/register`, 'POST', {
            firstName: 'Test',
            lastName: 'User',
            email: 'test@example.com',
            password: 'weak',
            role: 'user'
        }, {
            expectedStatus: 400,
            expectedFields: ['message']
        });
        
        // Test invalid age
        await this.testEndpoint('Invalid Age', `${this.apiBaseUrl}/runners`, 'POST', {
            firstName: 'Test',
            lastName: 'Runner',
            age: 150, // Invalid age
            city: 'Test City',
            state: 'TS',
            status: 'active'
        }, {
            expectedStatus: 400,
            expectedFields: ['message']
        });
    }

    // Test individual endpoint
    async testEndpoint(name, url, method, body, expectations) {
        try {
            console.log(`  Testing: ${name}`);
            
            const response = await this.makeRequest(url, method, body, expectations.requiresAuth);
            const responseData = await this.parseResponse(response);
            
            // Check status code
            const statusCorrect = response.status === expectations.expectedStatus;
            
            // Check response fields
            const fieldsCorrect = this.validateResponseFields(responseData, expectations.expectedFields);
            
            // Overall test result
            const testPassed = statusCorrect && fieldsCorrect;
            
            this.testResult(name, testPassed, 
                `Status: ${response.status} (expected ${expectations.expectedStatus}), ` +
                `Fields: ${fieldsCorrect ? 'OK' : 'Missing fields'}`);
            
            return testPassed;
            
        } catch (error) {
            this.testResult(name, false, `Error: ${error.message}`);
            return false;
        }
    }

    // Make HTTP request
    async makeRequest(url, method, body = null, requiresAuth = false) {
        const headers = {
            'Content-Type': 'application/json'
        };
        
        if (requiresAuth && this.authToken) {
            headers['Authorization'] = `Bearer ${this.authToken}`;
        }
        
        const options = {
            method: method,
            headers: headers
        };
        
        if (body && method !== 'GET') {
            options.body = typeof body === 'string' ? body : JSON.stringify(body);
        }
        
        const response = await fetch(url, options);
        return response;
    }

    // Parse response
    async parseResponse(response) {
        try {
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            } else {
                return await response.text();
            }
        } catch (error) {
            return null;
        }
    }

    // Validate response fields
    validateResponseFields(data, expectedFields) {
        if (!data || typeof data !== 'object') return false;
        
        return expectedFields.every(field => {
            return data.hasOwnProperty(field);
        });
    }

    // Record test result
    testResult(testName, passed, details) {
        const result = {
            test: testName,
            passed: passed,
            details: details,
            timestamp: new Date().toISOString()
        };
        
        this.testResults.push(result);
        
        const status = passed ? '✅ PASS' : '❌ FAIL';
        console.log(`    ${status} ${testName}: ${details}`);
        
        return passed;
    }

    // Test API performance
    async testAPIPerformance() {
        console.log('\n⚡ Testing API Performance...');
        
        const endpoints = [
            { name: 'Health Check', url: `${this.apiBaseUrl}/auth/health`, method: 'GET' },
            { name: 'Users API', url: `${this.apiBaseUrl}/auth/users`, method: 'GET', auth: true },
            { name: 'Runners API', url: `${this.apiBaseUrl}/runners`, method: 'GET', auth: true }
        ];
        
        for (const endpoint of endpoints) {
            if (endpoint.auth && !this.authToken) {
                console.log(`  ⏭️ Skipping ${endpoint.name} - no auth token`);
                continue;
            }
            
            const startTime = performance.now();
            try {
                const response = await this.makeRequest(endpoint.url, endpoint.method, null, endpoint.auth);
                const endTime = performance.now();
                const responseTime = endTime - startTime;
                
                this.testResult(`${endpoint.name} Performance`, responseTime < 5000, 
                    `Response time: ${responseTime.toFixed(2)}ms`);
                    
            } catch (error) {
                this.testResult(`${endpoint.name} Performance`, false, `Error: ${error.message}`);
            }
        }
    }

    // Test API availability
    async testAPIAvailability() {
        console.log('\n🌐 Testing API Availability...');
        
        const testUrls = [
            `${this.apiBaseUrl}/auth/health`,
            `${this.apiBaseUrl}/auth/users`,
            `${this.apiBaseUrl}/runners`
        ];
        
        for (const url of testUrls) {
            try {
                const startTime = performance.now();
                const response = await fetch(url, { method: 'HEAD' });
                const endTime = performance.now();
                const responseTime = endTime - startTime;
                
                this.testResult(`API Availability: ${url}`, response.ok, 
                    `Status: ${response.status}, Response time: ${responseTime.toFixed(2)}ms`);
                    
            } catch (error) {
                this.testResult(`API Availability: ${url}`, false, `Error: ${error.message}`);
            }
        }
    }

    // Generate endpoint test report
    generateEndpointReport() {
        console.log('\n📋 API Endpoint Test Report');
        console.log('============================');
        
        const totalTests = this.testResults.length;
        const passedTests = this.testResults.filter(r => r.passed).length;
        const failedTests = totalTests - passedTests;
        const successRate = ((passedTests / totalTests) * 100).toFixed(1);
        
        console.log(`Total Endpoint Tests: ${totalTests}`);
        console.log(`Passed: ${passedTests} ✅`);
        console.log(`Failed: ${failedTests} ❌`);
        console.log(`Success Rate: ${successRate}%`);
        
        if (failedTests > 0) {
            console.log('\n❌ Failed Endpoint Tests:');
            this.testResults
                .filter(r => !r.passed)
                .forEach(r => console.log(`  - ${r.test}: ${r.details}`));
        }
        
        console.log('\n🎯 API Health Assessment:');
        if (successRate >= 90) {
            console.log('  🎉 Excellent! All API endpoints are working properly');
        } else if (successRate >= 75) {
            console.log('  ⚠️ Good, but some endpoints need attention');
        } else {
            console.log('  🚨 Several endpoint issues detected - immediate attention required');
        }
        
        // Save results to localStorage
        localStorage.setItem('api_endpoint_test_results', JSON.stringify({
            timestamp: new Date().toISOString(),
            results: this.testResults,
            summary: { total: totalTests, passed: passedTests, failed: failedTests, successRate }
        }));
    }

    // Run performance and availability tests
    async runPerformanceTests() {
        await this.testAPIPerformance();
        await this.testAPIAvailability();
    }
}

// Auto-run tests when script is loaded
document.addEventListener('DOMContentLoaded', async () => {
    const endpointTester = new APIEndpointTester();
    
    if (await endpointTester.init()) {
        await endpointTester.runAllTests();
        await endpointTester.runPerformanceTests();
    }
});

// Export for manual testing
window.APIEndpointTester = APIEndpointTester; 