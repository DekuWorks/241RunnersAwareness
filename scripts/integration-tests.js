/**
 * ============================================
 * 241 RUNNERS AWARENESS - INTEGRATION TESTS
 * ============================================
 * 
 * Frontend to API integration tests for comprehensive validation
 */

const API_BASE_URL = 'https://241runners-api.azurewebsites.net';
const TEST_TIMEOUT = 30000; // 30 seconds

class IntegrationTester {
    constructor() {
        this.results = {
            passed: 0,
            failed: 0,
            tests: []
        };
        this.startTime = Date.now();
    }

    /**
     * Log test results
     */
    log(message, type = 'info') {
        const timestamp = new Date().toISOString();
        const prefix = type === 'error' ? '❌' : type === 'success' ? '✅' : 'ℹ️';
        console.log(`${prefix} [${timestamp}] ${message}`);
    }

    /**
     * Run a single test
     */
    async runTest(name, testFunction) {
        this.log(`Running test: ${name}`);
        
        try {
            await testFunction();
            this.results.passed++;
            this.results.tests.push({ name, status: 'PASSED', error: null });
            this.log(`✅ ${name} - PASSED`, 'success');
        } catch (error) {
            this.results.failed++;
            this.results.tests.push({ name, status: 'FAILED', error: error.message });
            this.log(`❌ ${name} - FAILED: ${error.message}`, 'error');
        }
    }

    /**
     * Make HTTP request with timeout
     */
    async makeRequest(url, options = {}) {
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), TEST_TIMEOUT);

        try {
            const response = await fetch(url, {
                ...options,
                signal: controller.signal,
                headers: {
                    'Content-Type': 'application/json',
                    ...options.headers
                }
            });

            clearTimeout(timeoutId);
            return response;
        } catch (error) {
            clearTimeout(timeoutId);
            throw error;
        }
    }

    /**
     * Test API health endpoint
     */
    async testApiHealth() {
        const response = await this.makeRequest(`${API_BASE_URL}/api/auth/health`);
        
        if (!response.ok) {
            throw new Error(`Health endpoint returned ${response.status}: ${response.statusText}`);
        }

        const data = await response.json();
        
        if (data.Status !== 'Healthy') {
            throw new Error(`Health endpoint returned unhealthy status: ${data.Status}`);
        }

        this.log(`API Health: ${data.Status} (Version: ${data.Version})`);
    }

    /**
     * Test CORS configuration
     */
    async testCorsConfiguration() {
        const response = await this.makeRequest(`${API_BASE_URL}/cors-test`, {
            headers: {
                'Origin': 'https://241runnersawareness.org'
            }
        });

        if (!response.ok) {
            throw new Error(`CORS test failed with ${response.status}: ${response.statusText}`);
        }

        const data = await response.json();
        
        if (!data.Message || !data.Message.includes('CORS is working')) {
            throw new Error('CORS test response is invalid');
        }

        this.log('CORS configuration is working correctly');
    }

    /**
     * Test user registration flow
     */
    async testUserRegistration() {
        const testUser = {
            email: `test-${Date.now()}@example.com`,
            password: 'TestPassword123!',
            firstName: 'Test',
            lastName: 'User',
            phoneNumber: '555-0123'
        };

        const response = await this.makeRequest(`${API_BASE_URL}/api/auth/register`, {
            method: 'POST',
            body: JSON.stringify(testUser)
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(`Registration failed: ${errorData.message || response.statusText}`);
        }

        const data = await response.json();
        
        if (!data.token || !data.user) {
            throw new Error('Registration response missing token or user data');
        }

        this.log(`User registration successful: ${data.user.email}`);
        return { token: data.token, user: data.user };
    }

    /**
     * Test user login flow
     */
    async testUserLogin() {
        // First register a user
        const testUser = {
            email: `test-${Date.now()}@example.com`,
            password: 'TestPassword123!',
            firstName: 'Test',
            lastName: 'User',
            phoneNumber: '555-0123'
        };

        // Register
        await this.makeRequest(`${API_BASE_URL}/api/auth/register`, {
            method: 'POST',
            body: JSON.stringify(testUser)
        });

        // Login
        const loginResponse = await this.makeRequest(`${API_BASE_URL}/api/auth/login`, {
            method: 'POST',
            body: JSON.stringify({
                email: testUser.email,
                password: testUser.password
            })
        });

        if (!loginResponse.ok) {
            const errorData = await loginResponse.json();
            throw new Error(`Login failed: ${errorData.message || loginResponse.statusText}`);
        }

        const loginData = await loginResponse.json();
        
        if (!loginData.token || !loginData.user) {
            throw new Error('Login response missing token or user data');
        }

        this.log(`User login successful: ${loginData.user.email}`);
        return { token: loginData.token, user: loginData.user };
    }

    /**
     * Test authenticated endpoints
     */
    async testAuthenticatedEndpoints() {
        const { token } = await this.testUserLogin();

        // Test /api/auth/me endpoint
        const meResponse = await this.makeRequest(`${API_BASE_URL}/api/auth/me`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!meResponse.ok) {
            throw new Error(`/api/auth/me failed: ${meResponse.statusText}`);
        }

        const meData = await meResponse.json();
        
        if (!meData.email) {
            throw new Error('/api/auth/me response missing user data');
        }

        this.log('Authenticated endpoints working correctly');
    }

    /**
     * Test JWT token validation
     */
    async testJwtValidation() {
        const { token } = await this.testUserLogin();

        // Test with valid token
        const validResponse = await this.makeRequest(`${API_BASE_URL}/api/auth/me`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!validResponse.ok) {
            throw new Error('Valid JWT token was rejected');
        }

        // Test with invalid token
        const invalidResponse = await this.makeRequest(`${API_BASE_URL}/api/auth/me`, {
            headers: {
                'Authorization': 'Bearer invalid-token'
            }
        });

        if (invalidResponse.ok) {
            throw new Error('Invalid JWT token was accepted');
        }

        if (invalidResponse.status !== 401) {
            throw new Error(`Expected 401 for invalid token, got ${invalidResponse.status}`);
        }

        this.log('JWT token validation working correctly');
    }

    /**
     * Test error handling
     */
    async testErrorHandling() {
        // Test 404 endpoint
        const notFoundResponse = await this.makeRequest(`${API_BASE_URL}/api/nonexistent`);
        
        if (notFoundResponse.status !== 404) {
            throw new Error(`Expected 404 for nonexistent endpoint, got ${notFoundResponse.status}`);
        }

        // Test invalid registration data
        const invalidRegistrationResponse = await this.makeRequest(`${API_BASE_URL}/api/auth/register`, {
            method: 'POST',
            body: JSON.stringify({
                email: 'invalid-email',
                password: '123' // Too short
            })
        });

        if (invalidRegistrationResponse.ok) {
            throw new Error('Invalid registration data was accepted');
        }

        if (invalidRegistrationResponse.status !== 400) {
            throw new Error(`Expected 400 for invalid data, got ${invalidRegistrationResponse.status}`);
        }

        this.log('Error handling working correctly');
    }

    /**
     * Test API performance
     */
    async testApiPerformance() {
        const startTime = Date.now();
        
        // Make multiple concurrent requests
        const promises = Array(10).fill().map(() => 
            this.makeRequest(`${API_BASE_URL}/api/auth/health`)
        );

        await Promise.all(promises);
        
        const endTime = Date.now();
        const duration = endTime - startTime;
        const avgResponseTime = duration / 10;

        if (avgResponseTime > 5000) { // 5 seconds
            throw new Error(`API performance is too slow: ${avgResponseTime}ms average response time`);
        }

        this.log(`API performance test passed: ${avgResponseTime}ms average response time`);
    }

    /**
     * Test database connectivity
     */
    async testDatabaseConnectivity() {
        // This test assumes the API has a database-dependent endpoint
        // We'll test by trying to register a user (which requires database access)
        try {
            await this.testUserRegistration();
            this.log('Database connectivity working correctly');
        } catch (error) {
            throw new Error(`Database connectivity test failed: ${error.message}`);
        }
    }

    /**
     * Run all integration tests
     */
    async runAllTests() {
        this.log('Starting 241 Runners Awareness API Integration Tests');
        this.log(`API Base URL: ${API_BASE_URL}`);
        this.log(`Test Timeout: ${TEST_TIMEOUT}ms`);

        // Core functionality tests
        await this.runTest('API Health Check', () => this.testApiHealth());
        await this.runTest('CORS Configuration', () => this.testCorsConfiguration());
        await this.runTest('Database Connectivity', () => this.testDatabaseConnectivity());
        
        // Authentication tests
        await this.runTest('User Registration', () => this.testUserRegistration());
        await this.runTest('User Login', () => this.testUserLogin());
        await this.runTest('JWT Token Validation', () => this.testJwtValidation());
        await this.runTest('Authenticated Endpoints', () => this.testAuthenticatedEndpoints());
        
        // Error handling tests
        await this.runTest('Error Handling', () => this.testErrorHandling());
        
        // Performance tests
        await this.runTest('API Performance', () => this.testApiPerformance());

        // Generate test report
        this.generateReport();
    }

    /**
     * Generate test report
     */
    generateReport() {
        const endTime = Date.now();
        const totalDuration = endTime - this.startTime;
        const totalTests = this.results.passed + this.results.failed;
        const successRate = totalTests > 0 ? (this.results.passed / totalTests * 100).toFixed(1) : 0;

        console.log('\n' + '='.repeat(60));
        console.log('INTEGRATION TEST REPORT');
        console.log('='.repeat(60));
        console.log(`Total Tests: ${totalTests}`);
        console.log(`Passed: ${this.results.passed}`);
        console.log(`Failed: ${this.results.failed}`);
        console.log(`Success Rate: ${successRate}%`);
        console.log(`Total Duration: ${totalDuration}ms`);
        console.log('='.repeat(60));

        if (this.results.failed > 0) {
            console.log('\nFAILED TESTS:');
            this.results.tests
                .filter(test => test.status === 'FAILED')
                .forEach(test => {
                    console.log(`❌ ${test.name}: ${test.error}`);
                });
        }

        console.log('\nDETAILED RESULTS:');
        this.results.tests.forEach(test => {
            const status = test.status === 'PASSED' ? '✅' : '❌';
            console.log(`${status} ${test.name}`);
        });

        console.log('\n' + '='.repeat(60));

        if (this.results.failed === 0) {
            console.log('🎉 ALL TESTS PASSED! API is ready for production.');
        } else {
            console.log('⚠️  Some tests failed. Please review and fix issues before deployment.');
            process.exit(1);
        }
    }
}

// Run tests if this script is executed directly
if (typeof window === 'undefined') {
    // Node.js environment
    const tester = new IntegrationTester();
    tester.runAllTests().catch(error => {
        console.error('Integration tests failed:', error);
        process.exit(1);
    });
} else {
    // Browser environment
    window.IntegrationTester = IntegrationTester;
}
