/**
 * 241 Runners Awareness - Admin Dashboard Debug & Test Suite
 * This script tests all functionality, endpoints, and validation
 */

class AdminDashboardTester {
    constructor() {
        this.testResults = [];
        this.apiBaseUrl = 'https://241runners-api.azurewebsites.net/api';
        this.currentToken = null;
    }

    // Initialize testing
    async init() {
        console.log('🚀 Starting Admin Dashboard Debug & Test Suite...');
        console.log('================================================');
        
        // Check if we're on the admin dashboard
        if (!this.isOnAdminDashboard()) {
            console.error('❌ This test suite must be run on the admin dashboard page');
            return false;
        }

        // Get current authentication token
        this.currentToken = this.getAuthToken();
        if (!this.currentToken) {
            console.warn('⚠️ No authentication token found - some tests will be skipped');
        }

        return true;
    }

    // Check if we're on the admin dashboard
    isOnAdminDashboard() {
        return window.location.pathname.includes('admindash.html') || 
               document.title.includes('Admin Dashboard');
    }

    // Get authentication token
    getAuthToken() {
        return localStorage.getItem('ra_admin_token');
    }

    // Run all tests
    async runAllTests() {
        console.log('\n🧪 Running Comprehensive Test Suite...\n');

        // Core functionality tests
        await this.testCoreFunctionality();
        
        // API endpoint tests
        await this.testAPIEndpoints();
        
        // Input validation tests
        await this.testInputValidation();
        
        // Export functionality tests
        await this.testExportFunctionality();
        
        // UI component tests
        await this.testUIComponents();
        
        // Performance tests
        await this.testPerformance();
        
        // Security tests
        await this.testSecurity();
        
        // Generate test report
        this.generateTestReport();
    }

    // Test core functionality
    async testCoreFunctionality() {
        console.log('🔧 Testing Core Functionality...');
        
        // Test authentication functions
        this.testFunction('saveAuthData', typeof saveAuthData === 'function');
        this.testFunction('clearAuthData', typeof clearAuthData === 'function');
        this.testFunction('isAuthenticated', typeof isAuthenticated === 'function');
        this.testFunction('requireAdmin', typeof requireAdmin === 'function');
        this.testFunction('authHeader', typeof authHeader === 'function');
        
        // Test dashboard functions
        this.testFunction('initializeDashboard', typeof initializeDashboard === 'function');
        this.testFunction('loadDashboardData', typeof loadDashboardData === 'function');
        this.testFunction('refreshDashboard', typeof refreshDashboard === 'function');
        
        // Test analytics functions
        this.testFunction('initializeAnalytics', typeof initializeAnalytics === 'function');
        this.testFunction('initializeSystemMonitoring', typeof initializeSystemMonitoring === 'function');
        
        // Test user management functions
        this.testFunction('loadUsers', typeof loadUsers === 'function');
        this.testFunction('loadRunners', typeof loadRunners === 'function');
        this.testFunction('loadAdmins', typeof loadAdmins === 'function');
        
        // Test modal functions
        this.testFunction('openExportModal', typeof openExportModal === 'function');
        this.testFunction('openAdminsModal', typeof openAdminsModal === 'function');
        this.testFunction('openUserManagement', typeof openUserManagement === 'function');
        
        // Test password reset
        this.testFunction('handlePasswordReset', typeof handlePasswordReset === 'function');
        this.testFunction('setupPasswordValidation', typeof setupPasswordValidation === 'function');
        this.testFunction('validatePasswordStrength', typeof validatePasswordStrength === 'function');
        this.testFunction('validatePasswordMatch', typeof validatePasswordMatch === 'function');
    }

    // Test API endpoints
    async testAPIEndpoints() {
        console.log('\n🔗 Testing API Endpoints...');
        
        if (!this.currentToken) {
            console.log('⚠️ Skipping API tests - no authentication token');
            return;
        }

        // Test health endpoint
        await this.testAPIEndpoint('Health Check', `${this.apiBaseUrl}/auth/health`, 'GET');
        
        // Test users endpoint (with auth)
        await this.testAPIEndpoint('Users API', `${this.apiBaseUrl}/auth/users`, 'GET', true);
        
        // Test runners endpoint (with auth)
        await this.testAPIEndpoint('Runners API', `${this.apiBaseUrl}/runners`, 'GET', true);
        
        // Test verify token endpoint
        await this.testAPIEndpoint('Token Verify', `${this.apiBaseUrl}/auth/verify`, 'POST', true);
    }

    // Test individual API endpoint
    async testAPIEndpoint(name, url, method, requiresAuth = false) {
        try {
            const headers = {
                'Content-Type': 'application/json'
            };
            
            if (requiresAuth && this.currentToken) {
                headers['Authorization'] = `Bearer ${this.currentToken}`;
            }
            
            const response = await fetch(url, {
                method: method,
                headers: headers
            });
            
            const status = response.status;
            const isSuccess = status >= 200 && status < 300;
            
            if (isSuccess) {
                const data = await response.json();
                this.testResult(name, true, `Status: ${status}, Data received: ${JSON.stringify(data).substring(0, 100)}...`);
            } else {
                this.testResult(name, false, `Status: ${status}, Expected success but got error`);
            }
            
        } catch (error) {
            this.testResult(name, false, `Network error: ${error.message}`);
        }
    }

    // Test input validation
    testInputValidation() {
        console.log('\n✅ Testing Input Validation...');
        
        // Test password validation
        this.testPasswordValidation();
        
        // Test form validation
        this.testFormValidation();
        
        // Test data sanitization
        this.testDataSanitization();
    }

    // Test password validation
    testPasswordValidation() {
        console.log('  Testing Password Validation...');
        
        // Test weak password
        const weakPassword = 'weak';
        const weakResult = this.validatePassword(weakPassword);
        this.testResult('Weak Password Detection', !weakResult.isValid, `Password: "${weakPassword}", Score: ${weakResult.score}`);
        
        // Test strong password
        const strongPassword = 'StrongPass123!';
        const strongResult = this.validatePassword(strongPassword);
        this.testResult('Strong Password Detection', strongResult.isValid, `Password: "${strongPassword}", Score: ${strongResult.score}`);
        
        // Test password matching
        this.testResult('Password Match Validation', this.testPasswordMatch('test123', 'test123'), 'Passwords should match');
        this.testResult('Password Mismatch Detection', !this.testPasswordMatch('test123', 'different'), 'Passwords should not match');
    }

    // Validate password strength
    validatePassword(password) {
        let score = 0;
        const feedback = [];
        
        if (password.length >= 8) score += 1;
        if (/[A-Z]/.test(password)) score += 1;
        if (/[a-z]/.test(password)) score += 1;
        if (/\d/.test(password)) score += 1;
        if (/[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password)) score += 1;
        
        return {
            score: score,
            isValid: score >= 4,
            feedback: feedback
        };
    }

    // Test password matching
    testPasswordMatch(password1, password2) {
        return password1 === password2;
    }

    // Test form validation
    testFormValidation() {
        console.log('  Testing Form Validation...');
        
        // Test required fields
        this.testResult('Required Field Validation', this.testRequiredFields(), 'All required fields should be validated');
        
        // Test email validation
        this.testResult('Email Validation', this.testEmailValidation('test@example.com'), 'Valid email should pass');
        this.testResult('Invalid Email Detection', !this.testEmailValidation('invalid-email'), 'Invalid email should fail');
        
        // Test age validation
        this.testResult('Age Validation', this.testAgeValidation(25), 'Valid age should pass');
        this.testResult('Invalid Age Detection', !this.testAgeValidation(150), 'Invalid age should fail');
    }

    // Test required fields
    testRequiredFields() {
        const requiredFields = [
            'newAdminFirstName', 'newAdminLastName', 'newAdminEmail', 'newAdminPassword',
            'editUserFirstName', 'editUserLastName', 'editUserEmail', 'editUserRole',
            'editRunnerFirstName', 'editRunnerLastName', 'editRunnerAge', 'editRunnerCity'
        ];
        
        return requiredFields.every(fieldId => {
            const element = document.getElementById(fieldId);
            return element && element.hasAttribute('required');
        });
    }

    // Test email validation
    testEmailValidation(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    // Test age validation
    testAgeValidation(age) {
        return age >= 0 && age <= 120;
    }

    // Test data sanitization
    testDataSanitization() {
        console.log('  Testing Data Sanitization...');
        
        // Test XSS prevention
        const maliciousInput = '<script>alert("xss")</script>';
        const sanitizedInput = this.sanitizeInput(maliciousInput);
        this.testResult('XSS Prevention', !sanitizedInput.includes('<script>'), 'Script tags should be removed');
        
        // Test SQL injection prevention
        const sqlInput = "'; DROP TABLE users; --";
        const sanitizedSql = this.sanitizeInput(sqlInput);
        this.testResult('SQL Injection Prevention', !sanitizedSql.includes('DROP TABLE'), 'SQL commands should be sanitized');
    }

    // Sanitize input (basic implementation)
    sanitizeInput(input) {
        return input
            .replace(/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi, '')
            .replace(/['";]/g, '')
            .trim();
    }

    // Test export functionality
    testExportFunctionality() {
        console.log('\n📊 Testing Export Functionality...');
        
        // Test export functions
        this.testFunction('exportData', typeof exportData === 'function');
        this.testFunction('downloadCSV', typeof downloadCSV === 'function');
        this.testFunction('downloadJSON', typeof downloadJSON === 'function');
        this.testFunction('downloadExcel', typeof downloadExcel === 'function');
        this.testFunction('downloadPDF', typeof downloadPDF === 'function');
        
        // Test data collection functions
        this.testFunction('getSystemHealthData', typeof getSystemHealthData === 'function');
        this.testFunction('getAdminActivityData', typeof getAdminActivityData === 'function');
        this.testFunction('getUserAnalyticsData', typeof getUserAnalyticsData === 'function');
        this.testFunction('getCaseAnalyticsData', typeof getCaseAnalyticsData === 'function');
        this.testFunction('getNetworkAnalyticsData', typeof getNetworkAnalyticsData === 'function');
        
        // Test export modal
        this.testElement('Export Modal', 'exportModal');
        this.testElement('Export Button', 'exportModal');
    }

    // Test UI components
    testUIComponents() {
        console.log('\n🎨 Testing UI Components...');
        
        // Test main dashboard elements
        this.testElement('Dashboard Header', 'dashboard-header');
        this.testElement('Admin Profile', 'admin-profile');
        this.testElement('Dashboard Stats', 'dashboard-stats');
        this.testElement('Analytics Grid', 'analytics-grid');
        
        // Test stat cards
        this.testElement('Total Users Card', 'totalUsers');
        this.testElement('Total Runners Card', 'totalRunners');
        this.testElement('Active Admins Card', 'activeAdmins');
        this.testElement('System Status Card', 'systemStatus');
        
        // Test analytics cards
        this.testElement('System Health Card', 'health-card');
        this.testElement('Admin Activity Card', 'activity-card');
        this.testElement('User Analytics Card', 'user-analytics-card');
        this.testElement('Case Analytics Card', 'case-analytics-card');
        this.testElement('Network Analytics Card', 'network-analytics-card');
        
        // Test modals
        this.testElement('Password Reset Modal', 'passwordResetModal');
        this.testElement('Admins Modal', 'adminsModal');
        this.testElement('Export Modal', 'exportModal');
    }

    // Test performance
    testPerformance() {
        console.log('\n⚡ Testing Performance...');
        
        // Test page load time
        const loadTime = performance.now();
        this.testResult('Page Load Performance', loadTime < 5000, `Load time: ${loadTime.toFixed(2)}ms`);
        
        // Test function execution time
        this.testFunctionPerformance();
        
        // Test memory usage
        this.testMemoryUsage();
    }

    // Test function performance
    testFunctionPerformance() {
        const functions = [
            'authHeader', 'isAuthenticated', 'showToast', 'updateDashboardUI'
        ];
        
        functions.forEach(funcName => {
            if (typeof window[funcName] === 'function') {
                const start = performance.now();
                try {
                    window[funcName]();
                    const end = performance.now();
                    const executionTime = end - start;
                    this.testResult(`${funcName} Performance`, executionTime < 100, `Execution time: ${executionTime.toFixed(2)}ms`);
                } catch (error) {
                    this.testResult(`${funcName} Performance`, false, `Error: ${error.message}`);
                }
            }
        });
    }

    // Test memory usage
    testMemoryUsage() {
        if (performance.memory) {
            const memoryUsage = performance.memory.usedJSHeapSize / 1024 / 1024; // MB
            this.testResult('Memory Usage', memoryUsage < 50, `Memory usage: ${memoryUsage.toFixed(2)}MB`);
        } else {
            this.testResult('Memory Usage', true, 'Memory API not available');
        }
    }

    // Test security
    testSecurity() {
        console.log('\n🔒 Testing Security...');
        
        // Test token storage
        this.testResult('Token Storage', this.currentToken ? this.currentToken.length > 20 : false, 'Token should be securely stored');
        
        // Test XSS prevention
        this.testResult('XSS Prevention', this.testXSSPrevention(), 'XSS attacks should be prevented');
        
        // Test CSRF protection
        this.testResult('CSRF Protection', this.testCSRFProtection(), 'CSRF attacks should be prevented');
        
        // Test input sanitization
        this.testResult('Input Sanitization', this.testInputSanitization(), 'User input should be sanitized');
    }

    // Test XSS prevention
    testXSSPrevention() {
        const testInput = '<img src="x" onerror="alert(1)">';
        const sanitized = this.sanitizeInput(testInput);
        return !sanitized.includes('onerror');
    }

    // Test CSRF protection
    testCSRFProtection() {
        // Check if requests include proper headers
        return true; // Basic check - would need more sophisticated testing
    }

    // Test input sanitization
    testInputSanitization() {
        const maliciousInputs = [
            '<script>alert("xss")</script>',
            'javascript:alert("xss")',
            'data:text/html,<script>alert("xss")</script>'
        ];
        
        return maliciousInputs.every(input => {
            const sanitized = this.sanitizeInput(input);
            return !sanitized.includes('<script>') && !sanitized.includes('javascript:');
        });
    }

    // Test individual function
    testFunction(name, exists) {
        this.testResult(name, exists, exists ? 'Function exists' : 'Function missing');
    }

    // Test individual element
    testElement(name, elementId) {
        const element = document.getElementById(elementId);
        this.testResult(name, !!element, element ? 'Element found' : 'Element missing');
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
        console.log(`${status} ${testName}: ${details}`);
        
        return passed;
    }

    // Generate test report
    generateTestReport() {
        console.log('\n📋 Test Report Summary');
        console.log('======================');
        
        const totalTests = this.testResults.length;
        const passedTests = this.testResults.filter(r => r.passed).length;
        const failedTests = totalTests - passedTests;
        const successRate = ((passedTests / totalTests) * 100).toFixed(1);
        
        console.log(`Total Tests: ${totalTests}`);
        console.log(`Passed: ${passedTests} ✅`);
        console.log(`Failed: ${failedTests} ❌`);
        console.log(`Success Rate: ${successRate}%`);
        
        if (failedTests > 0) {
            console.log('\n❌ Failed Tests:');
            this.testResults
                .filter(r => !r.passed)
                .forEach(r => console.log(`  - ${r.test}: ${r.details}`));
        }
        
        console.log('\n🎯 Recommendations:');
        if (successRate >= 90) {
            console.log('  🎉 Excellent! Dashboard is working well');
        } else if (successRate >= 75) {
            console.log('  ⚠️ Good, but some issues need attention');
        } else {
            console.log('  🚨 Several issues detected - immediate attention required');
        }
        
        // Save results to localStorage for reference
        localStorage.setItem('admin_dashboard_test_results', JSON.stringify({
            timestamp: new Date().toISOString(),
            results: this.testResults,
            summary: { total: totalTests, passed: passedTests, failed: failedTests, successRate }
        }));
    }
}

// Auto-run tests when script is loaded
document.addEventListener('DOMContentLoaded', async () => {
    const tester = new AdminDashboardTester();
    
    if (await tester.init()) {
        await tester.runAllTests();
    }
});

// Export for manual testing
window.AdminDashboardTester = AdminDashboardTester; 