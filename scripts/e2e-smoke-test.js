#!/usr/bin/env node

/**
 * ============================================
 * 241 RUNNERS AWARENESS - E2E SMOKE TEST
 * ============================================
 * 
 * P3 Implementation: Admin E2E smoke test
 * Simple smoke test for critical admin functionality
 */

const puppeteer = require('puppeteer');
const fs = require('fs');
const path = require('path');

// ===== CONFIGURATION =====
const CONFIG = {
    baseUrl: 'https://241runnersawareness.org',
    apiUrl: 'https://241runners-api.azurewebsites.net',
    timeout: 30000,
    headless: true,
    viewport: { width: 1280, height: 720 },
    adminCredentials: {
        email: process.env.ADMIN_EMAIL || 'admin@241runnersawareness.org',
        password: process.env.ADMIN_PASSWORD || 'TestPassword123!'
    }
};

// ===== TEST RESULTS =====
let testResults = {
    passed: 0,
    failed: 0,
    tests: []
};

// ===== UTILITY FUNCTIONS =====

/**
 * Log test result
 * @param {string} testName - Test name
 * @param {boolean} passed - Test result
 * @param {string} message - Test message
 */
function logTest(testName, passed, message) {
    const result = { testName, passed, message, timestamp: new Date().toISOString() };
    testResults.tests.push(result);
    
    if (passed) {
        testResults.passed++;
        console.log(`✅ ${testName}: ${message}`);
    } else {
        testResults.failed++;
        console.log(`❌ ${testName}: ${message}`);
    }
}

/**
 * Wait for element to be visible
 * @param {Object} page - Puppeteer page
 * @param {string} selector - CSS selector
 * @param {number} timeout - Timeout in milliseconds
 */
async function waitForElement(page, selector, timeout = 5000) {
    try {
        await page.waitForSelector(selector, { visible: true, timeout });
        return true;
    } catch (error) {
        return false;
    }
}

/**
 * Take screenshot
 * @param {Object} page - Puppeteer page
 * @param {string} name - Screenshot name
 */
async function takeScreenshot(page, name) {
    try {
        const screenshotPath = path.join(__dirname, 'screenshots', `${name}-${Date.now()}.png`);
        await page.screenshot({ path: screenshotPath, fullPage: true });
        console.log(`📸 Screenshot saved: ${screenshotPath}`);
    } catch (error) {
        console.warn('Failed to take screenshot:', error.message);
    }
}

// ===== TEST FUNCTIONS =====

/**
 * Test 1: Home page loads
 * @param {Object} page - Puppeteer page
 */
async function testHomePageLoads(page) {
    try {
        await page.goto(CONFIG.baseUrl, { waitUntil: 'networkidle2', timeout: CONFIG.timeout });
        
        const title = await page.title();
        const hasContent = await page.$('body') !== null;
        
        if (title.includes('241 Runners') && hasContent) {
            logTest('Home Page Loads', true, 'Home page loaded successfully');
            return true;
        } else {
            logTest('Home Page Loads', false, `Title: ${title}, Has content: ${hasContent}`);
            return false;
        }
    } catch (error) {
        logTest('Home Page Loads', false, error.message);
        return false;
    }
}

/**
 * Test 2: Admin login page loads
 * @param {Object} page - Puppeteer page
 */
async function testAdminLoginPageLoads(page) {
    try {
        await page.goto(`${CONFIG.baseUrl}/admin/login.html`, { waitUntil: 'networkidle2', timeout: CONFIG.timeout });
        
        const title = await page.title();
        const hasLoginForm = await page.$('form') !== null;
        const hasEmailField = await page.$('input[type="email"]') !== null;
        const hasPasswordField = await page.$('input[type="password"]') !== null;
        
        if (title.includes('Admin Login') && hasLoginForm && hasEmailField && hasPasswordField) {
            logTest('Admin Login Page Loads', true, 'Admin login page loaded with form elements');
            return true;
        } else {
            logTest('Admin Login Page Loads', false, `Title: ${title}, Form: ${hasLoginForm}, Email: ${hasEmailField}, Password: ${hasPasswordField}`);
            return false;
        }
    } catch (error) {
        logTest('Admin Login Page Loads', false, error.message);
        return false;
    }
}

/**
 * Test 3: Admin login functionality
 * @param {Object} page - Puppeteer page
 */
async function testAdminLogin(page) {
    try {
        await page.goto(`${CONFIG.baseUrl}/admin/login.html`, { waitUntil: 'networkidle2', timeout: CONFIG.timeout });
        
        // Fill login form
        await page.type('input[type="email"]', CONFIG.adminCredentials.email);
        await page.type('input[type="password"]', CONFIG.adminCredentials.password);
        
        // Submit form
        await page.click('button[type="submit"]');
        
        // Wait for redirect or error
        await page.waitForTimeout(3000);
        
        const currentUrl = page.url();
        const hasError = await page.$('.error, .alert-danger') !== null;
        
        if (currentUrl.includes('admindash') && !hasError) {
            logTest('Admin Login', true, 'Successfully logged in and redirected to dashboard');
            return true;
        } else if (hasError) {
            const errorText = await page.$eval('.error, .alert-danger', el => el.textContent).catch(() => 'Unknown error');
            logTest('Admin Login', false, `Login failed with error: ${errorText}`);
            return false;
        } else {
            logTest('Admin Login', false, `Unexpected redirect to: ${currentUrl}`);
            return false;
        }
    } catch (error) {
        logTest('Admin Login', false, error.message);
        return false;
    }
}

/**
 * Test 4: Admin dashboard loads
 * @param {Object} page - Puppeteer page
 */
async function testAdminDashboardLoads(page) {
    try {
        const dashboardLoaded = await waitForElement(page, '.admin-dashboard', 10000);
        const hasStats = await page.$('.stat-card') !== null;
        const hasHeader = await page.$('.dashboard-header') !== null;
        
        if (dashboardLoaded && hasStats && hasHeader) {
            logTest('Admin Dashboard Loads', true, 'Dashboard loaded with stats and header');
            return true;
        } else {
            logTest('Admin Dashboard Loads', false, `Dashboard: ${dashboardLoaded}, Stats: ${hasStats}, Header: ${hasHeader}`);
            return false;
        }
    } catch (error) {
        logTest('Admin Dashboard Loads', false, error.message);
        return false;
    }
}

/**
 * Test 5: API health check
 * @param {Object} page - Puppeteer page
 */
async function testApiHealthCheck(page) {
    try {
        const response = await page.goto(`${CONFIG.apiUrl}/healthz`, { waitUntil: 'networkidle2', timeout: 10000 });
        const status = response.status();
        const content = await page.content();
        
        if (status === 200 && content.includes('Healthy')) {
            logTest('API Health Check', true, 'API health endpoint returned 200 OK');
            return true;
        } else {
            logTest('API Health Check', false, `Status: ${status}, Content: ${content.substring(0, 100)}...`);
            return false;
        }
    } catch (error) {
        logTest('API Health Check', false, error.message);
        return false;
    }
}

/**
 * Test 6: Real-time connection
 * @param {Object} page - Puppeteer page
 */
async function testRealtimeConnection(page) {
    try {
        // Check if connection status element exists
        const hasConnectionStatus = await page.$('#connectionStatus') !== null;
        
        if (hasConnectionStatus) {
            // Wait a bit for connection to establish
            await page.waitForTimeout(5000);
            
            const connectionText = await page.$eval('#connectionStatus', el => el.textContent).catch(() => '');
            
            if (connectionText.includes('Connected') || connectionText.includes('Polling')) {
                logTest('Real-time Connection', true, `Connection status: ${connectionText}`);
                return true;
            } else {
                logTest('Real-time Connection', false, `Connection status: ${connectionText}`);
                return false;
            }
        } else {
            logTest('Real-time Connection', false, 'Connection status element not found');
            return false;
        }
    } catch (error) {
        logTest('Real-time Connection', false, error.message);
        return false;
    }
}

// ===== MAIN TEST RUNNER =====

/**
 * Run all smoke tests
 */
async function runSmokeTests() {
    console.log('🚀 Starting E2E Smoke Tests...');
    console.log(`📍 Base URL: ${CONFIG.baseUrl}`);
    console.log(`🔗 API URL: ${CONFIG.apiUrl}`);
    console.log('');

    let browser;
    let page;

    try {
        // Launch browser
        browser = await puppeteer.launch({
            headless: CONFIG.headless,
            args: ['--no-sandbox', '--disable-setuid-sandbox']
        });

        page = await browser.newPage();
        await page.setViewport(CONFIG.viewport);

        // Create screenshots directory
        const screenshotsDir = path.join(__dirname, 'screenshots');
        if (!fs.existsSync(screenshotsDir)) {
            fs.mkdirSync(screenshotsDir, { recursive: true });
        }

        // Run tests
        await testHomePageLoads(page);
        await takeScreenshot(page, 'home-page');

        await testAdminLoginPageLoads(page);
        await takeScreenshot(page, 'admin-login');

        await testAdminLogin(page);
        await takeScreenshot(page, 'admin-logged-in');

        await testAdminDashboardLoads(page);
        await takeScreenshot(page, 'admin-dashboard');

        await testApiHealthCheck(page);

        await testRealtimeConnection(page);
        await takeScreenshot(page, 'realtime-connection');

    } catch (error) {
        console.error('❌ Test runner error:', error);
        testResults.failed++;
    } finally {
        if (browser) {
            await browser.close();
        }
    }

    // Print results
    console.log('');
    console.log('📊 Test Results Summary:');
    console.log(`✅ Passed: ${testResults.passed}`);
    console.log(`❌ Failed: ${testResults.failed}`);
    console.log(`📈 Success Rate: ${((testResults.passed / (testResults.passed + testResults.failed)) * 100).toFixed(1)}%`);

    // Save results to file
    const resultsPath = path.join(__dirname, 'test-results.json');
    fs.writeFileSync(resultsPath, JSON.stringify(testResults, null, 2));
    console.log(`📄 Results saved to: ${resultsPath}`);

    // Exit with appropriate code
    process.exit(testResults.failed > 0 ? 1 : 0);
}

// ===== CLI INTERFACE =====

if (require.main === module) {
    runSmokeTests().catch(error => {
        console.error('💥 Smoke test failed:', error);
        process.exit(1);
    });
}

module.exports = { runSmokeTests, testResults };
