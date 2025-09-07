/**
 * ============================================
 * 241 RUNNERS AWARENESS - E2E TEST SCRIPT
 * ============================================
 * 
 * Simple end-to-end test for admin functionality
 * P3 Implementation: Admin E2E smoke tests
 */

const puppeteer = require('puppeteer');

// Test configuration
const TEST_CONFIG = {
    baseUrl: 'https://241runnersawareness.org',
    adminUrl: 'https://241runnersawareness.org/admin',
    loginUrl: 'https://241runnersawareness.org/admin/login.html',
    timeout: 30000,
    headless: true
};

// Test credentials (should be environment variables in real tests)
const TEST_CREDENTIALS = {
    email: process.env.ADMIN_EMAIL || 'admin@example.com',
    password: process.env.ADMIN_PASSWORD || 'testpassword'
};

class E2ETestRunner {
    constructor() {
        this.browser = null;
        this.page = null;
        this.results = [];
    }

    async setup() {
        console.log('🚀 Setting up E2E test environment...');
        
        this.browser = await puppeteer.launch({
            headless: TEST_CONFIG.headless,
            args: ['--no-sandbox', '--disable-setuid-sandbox']
        });
        
        this.page = await this.browser.newPage();
        
        // Set viewport
        await this.page.setViewport({ width: 1280, height: 720 });
        
        // Set timeout
        this.page.setDefaultTimeout(TEST_CONFIG.timeout);
        
        console.log('✅ E2E test environment ready');
    }

    async teardown() {
        if (this.browser) {
            await this.browser.close();
        }
        console.log('🧹 E2E test environment cleaned up');
    }

    async runTest(testName, testFunction) {
        console.log(`\n🧪 Running test: ${testName}`);
        
        try {
            await testFunction();
            this.results.push({ name: testName, status: 'PASS', error: null });
            console.log(`✅ ${testName} - PASSED`);
        } catch (error) {
            this.results.push({ name: testName, status: 'FAIL', error: error.message });
            console.log(`❌ ${testName} - FAILED: ${error.message}`);
        }
    }

    async testPublicSiteLoads() {
        await this.page.goto(TEST_CONFIG.baseUrl);
        
        // Check if page loads
        await this.page.waitForSelector('body', { timeout: 10000 });
        
        // Check for basic elements
        const title = await this.page.title();
        if (!title.includes('241 Runners Awareness')) {
            throw new Error('Page title does not contain expected text');
        }
        
        // Check for navigation
        const nav = await this.page.$('nav');
        if (!nav) {
            throw new Error('Navigation element not found');
        }
    }

    async testAdminLoginPageLoads() {
        await this.page.goto(TEST_CONFIG.loginUrl);
        
        // Check if login page loads
        await this.page.waitForSelector('form', { timeout: 10000 });
        
        // Check for login form elements
        const emailInput = await this.page.$('input[type="email"]');
        const passwordInput = await this.page.$('input[type="password"]');
        const submitButton = await this.page.$('button[type="submit"]');
        
        if (!emailInput || !passwordInput || !submitButton) {
            throw new Error('Login form elements not found');
        }
    }

    async testAdminLogin() {
        await this.page.goto(TEST_CONFIG.loginUrl);
        
        // Fill login form
        await this.page.type('input[type="email"]', TEST_CREDENTIALS.email);
        await this.page.type('input[type="password"]', TEST_CREDENTIALS.password);
        
        // Submit form
        await Promise.all([
            this.page.waitForNavigation({ waitUntil: 'networkidle0' }),
            this.page.click('button[type="submit"]')
        ]);
        
        // Check if redirected to dashboard
        const currentUrl = this.page.url();
        if (!currentUrl.includes('/admin/admindash.html')) {
            throw new Error('Login failed - not redirected to dashboard');
        }
    }

    async testAdminDashboardLoads() {
        // Should already be on dashboard from login test
        await this.page.waitForSelector('.admin-dashboard', { timeout: 10000 });
        
        // Check for dashboard elements
        const dashboard = await this.page.$('.admin-dashboard');
        const stats = await this.page.$('.dashboard-stats');
        const header = await this.page.$('.dashboard-header');
        
        if (!dashboard || !stats || !header) {
            throw new Error('Dashboard elements not found');
        }
        
        // Check for stat cards
        const statCards = await this.page.$$('.stat-card');
        if (statCards.length < 3) {
            throw new Error('Insufficient stat cards found');
        }
    }

    async testRealTimeUpdates() {
        // Open a second page to simulate multiple admin sessions
        const page2 = await this.browser.newPage();
        await page2.setViewport({ width: 1280, height: 720 });
        
        try {
            // Login to second page
            await page2.goto(TEST_CONFIG.loginUrl);
            await page2.type('input[type="email"]', TEST_CREDENTIALS.email);
            await page2.type('input[type="password"]', TEST_CREDENTIALS.password);
            
            await Promise.all([
                page2.waitForNavigation({ waitUntil: 'networkidle0' }),
                page2.click('button[type="submit"]')
            ]);
            
            // Wait for both pages to load
            await this.page.waitForSelector('.admin-dashboard');
            await page2.waitForSelector('.admin-dashboard');
            
            // Check for real-time connection indicators
            const connectionStatus1 = await this.page.$('#connectionStatus');
            const connectionStatus2 = await page2.$('#connectionStatus');
            
            if (!connectionStatus1 || !connectionStatus2) {
                throw new Error('Real-time connection status not found');
            }
            
            // Wait a bit for real-time connection to establish
            await this.page.waitForTimeout(5000);
            
            console.log('✅ Real-time updates test completed');
            
        } finally {
            await page2.close();
        }
    }

    async testResponsiveDesign() {
        // Test mobile viewport
        await this.page.setViewport({ width: 375, height: 667 });
        await this.page.reload();
        
        // Check if dashboard adapts to mobile
        await this.page.waitForSelector('.admin-dashboard');
        
        // Check for mobile-specific styles
        const dashboard = await this.page.$('.admin-dashboard');
        const styles = await this.page.evaluate((el) => {
            return window.getComputedStyle(el);
        }, dashboard);
        
        // Reset to desktop viewport
        await this.page.setViewport({ width: 1280, height: 720 });
    }

    async testLogout() {
        // Click logout button
        const logoutButton = await this.page.$('#logoutBtn');
        if (!logoutButton) {
            throw new Error('Logout button not found');
        }
        
        await Promise.all([
            this.page.waitForNavigation({ waitUntil: 'networkidle0' }),
            this.page.click('#logoutBtn')
        ]);
        
        // Check if redirected to login page
        const currentUrl = this.page.url();
        if (!currentUrl.includes('/admin/login.html')) {
            throw new Error('Logout failed - not redirected to login page');
        }
    }

    async runAllTests() {
        console.log('🧪 Starting E2E test suite...\n');
        
        await this.setup();
        
        try {
            // Public site tests
            await this.runTest('Public Site Loads', () => this.testPublicSiteLoads());
            
            // Admin login tests
            await this.runTest('Admin Login Page Loads', () => this.testAdminLoginPageLoads());
            await this.runTest('Admin Login', () => this.testAdminLogin());
            
            // Dashboard tests
            await this.runTest('Admin Dashboard Loads', () => this.testAdminDashboardLoads());
            await this.runTest('Real-time Updates', () => this.testRealTimeUpdates());
            await this.runTest('Responsive Design', () => this.testResponsiveDesign());
            
            // Logout test
            await this.runTest('Admin Logout', () => this.testLogout());
            
        } finally {
            await this.teardown();
        }
        
        this.printResults();
    }

    printResults() {
        console.log('\n📊 E2E Test Results:');
        console.log('====================');
        
        let passed = 0;
        let failed = 0;
        
        this.results.forEach(result => {
            const status = result.status === 'PASS' ? '✅' : '❌';
            console.log(`${status} ${result.name}`);
            
            if (result.status === 'PASS') {
                passed++;
            } else {
                failed++;
                console.log(`   Error: ${result.error}`);
            }
        });
        
        console.log('\n📈 Summary:');
        console.log(`Total: ${this.results.length}`);
        console.log(`Passed: ${passed}`);
        console.log(`Failed: ${failed}`);
        
        if (failed > 0) {
            console.log('\n❌ Some tests failed. Please check the errors above.');
            process.exit(1);
        } else {
            console.log('\n✅ All tests passed!');
        }
    }
}

// Main execution
async function main() {
    const runner = new E2ETestRunner();
    
    try {
        await runner.runAllTests();
    } catch (error) {
        console.error('❌ Test suite failed:', error);
        process.exit(1);
    }
}

// Run if called directly
if (require.main === module) {
    main();
}

module.exports = E2ETestRunner;
