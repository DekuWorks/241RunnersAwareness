#!/usr/bin/env node

/**
 * Performance Testing Script for 241 Runners Awareness API
 * Tests various endpoints under load to measure performance
 */

const https = require('https');
const http = require('http');

const API_BASE_URL = 'https://241runners-api-v2.azurewebsites.net';
const CONCURRENT_REQUESTS = 10;
const TOTAL_REQUESTS = 100;
const REQUEST_TIMEOUT = 30000; // 30 seconds

// Test configuration
const tests = [
    {
        name: 'API Base Endpoint',
        url: `${API_BASE_URL}/api`,
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
    },
    {
        name: 'Database Health Check',
        url: `${API_BASE_URL}/readyz`,
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
    },
    {
        name: 'Database Test',
        url: `${API_BASE_URL}/api/test-db`,
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
    },
    {
        name: 'User Registration',
        url: `${API_BASE_URL}/api/auth/register`,
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            email: `perftest${Date.now()}@example.com`,
            password: 'PerfTest@2024!',
            firstName: 'Performance',
            lastName: 'Test',
            role: 'user'
        })
    },
    {
        name: 'Admin Users (with auth)',
        url: `${API_BASE_URL}/api/Admin/users-debug`,
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
    }
];

// Statistics tracking
const stats = {
    totalRequests: 0,
    successfulRequests: 0,
    failedRequests: 0,
    totalResponseTime: 0,
    minResponseTime: Infinity,
    maxResponseTime: 0,
    responseTimes: [],
    errors: []
};

/**
 * Make a single HTTP request
 */
function makeRequest(test) {
    return new Promise((resolve) => {
        const startTime = Date.now();
        const url = new URL(test.url);
        const options = {
            hostname: url.hostname,
            port: url.port || (url.protocol === 'https:' ? 443 : 80),
            path: url.pathname + url.search,
            method: test.method,
            headers: test.headers,
            timeout: REQUEST_TIMEOUT
        };

        const client = url.protocol === 'https:' ? https : http;
        
        const req = client.request(options, (res) => {
            let data = '';
            
            res.on('data', (chunk) => {
                data += chunk;
            });
            
            res.on('end', () => {
                const endTime = Date.now();
                const responseTime = endTime - startTime;
                
                stats.totalRequests++;
                stats.totalResponseTime += responseTime;
                stats.minResponseTime = Math.min(stats.minResponseTime, responseTime);
                stats.maxResponseTime = Math.max(stats.maxResponseTime, responseTime);
                stats.responseTimes.push(responseTime);
                
                if (res.statusCode >= 200 && res.statusCode < 300) {
                    stats.successfulRequests++;
                } else {
                    stats.failedRequests++;
                    stats.errors.push({
                        test: test.name,
                        statusCode: res.statusCode,
                        responseTime: responseTime,
                        error: `HTTP ${res.statusCode}`
                    });
                }
                
                resolve({
                    success: res.statusCode >= 200 && res.statusCode < 300,
                    statusCode: res.statusCode,
                    responseTime: responseTime,
                    data: data
                });
            });
        });
        
        req.on('error', (error) => {
            const endTime = Date.now();
            const responseTime = endTime - startTime;
            
            stats.totalRequests++;
            stats.failedRequests++;
            stats.errors.push({
                test: test.name,
                statusCode: 0,
                responseTime: responseTime,
                error: error.message
            });
            
            resolve({
                success: false,
                statusCode: 0,
                responseTime: responseTime,
                error: error.message
            });
        });
        
        req.on('timeout', () => {
            req.destroy();
            const endTime = Date.now();
            const responseTime = endTime - startTime;
            
            stats.totalRequests++;
            stats.failedRequests++;
            stats.errors.push({
                test: test.name,
                statusCode: 0,
                responseTime: responseTime,
                error: 'Request timeout'
            });
            
            resolve({
                success: false,
                statusCode: 0,
                responseTime: responseTime,
                error: 'Request timeout'
            });
        });
        
        if (test.body) {
            req.write(test.body);
        }
        
        req.end();
    });
}

/**
 * Run concurrent requests
 */
async function runConcurrentRequests(test, count) {
    const promises = [];
    for (let i = 0; i < count; i++) {
        promises.push(makeRequest(test));
    }
    return Promise.all(promises);
}

/**
 * Calculate percentiles
 */
function calculatePercentiles(responseTimes, percentiles = [50, 90, 95, 99]) {
    const sorted = [...responseTimes].sort((a, b) => a - b);
    const results = {};
    
    percentiles.forEach(p => {
        const index = Math.ceil((p / 100) * sorted.length) - 1;
        results[`p${p}`] = sorted[index] || 0;
    });
    
    return results;
}

/**
 * Print statistics
 */
function printStats() {
    console.log('\n' + '='.repeat(60));
    console.log('PERFORMANCE TEST RESULTS');
    console.log('='.repeat(60));
    
    console.log(`Total Requests: ${stats.totalRequests}`);
    console.log(`Successful Requests: ${stats.successfulRequests}`);
    console.log(`Failed Requests: ${stats.failedRequests}`);
    console.log(`Success Rate: ${((stats.successfulRequests / stats.totalRequests) * 100).toFixed(2)}%`);
    
    if (stats.responseTimes.length > 0) {
        const avgResponseTime = stats.totalResponseTime / stats.totalRequests;
        const percentiles = calculatePercentiles(stats.responseTimes);
        
        console.log('\nResponse Time Statistics:');
        console.log(`Average: ${avgResponseTime.toFixed(2)}ms`);
        console.log(`Minimum: ${stats.minResponseTime}ms`);
        console.log(`Maximum: ${stats.maxResponseTime}ms`);
        console.log(`P50 (Median): ${percentiles.p50}ms`);
        console.log(`P90: ${percentiles.p90}ms`);
        console.log(`P95: ${percentiles.p95}ms`);
        console.log(`P99: ${percentiles.p99}ms`);
    }
    
    if (stats.errors.length > 0) {
        console.log('\nErrors:');
        stats.errors.slice(0, 10).forEach(error => {
            console.log(`- ${error.test}: ${error.error} (${error.responseTime}ms)`);
        });
        if (stats.errors.length > 10) {
            console.log(`... and ${stats.errors.length - 10} more errors`);
        }
    }
    
    console.log('\n' + '='.repeat(60));
}

/**
 * Main test runner
 */
async function runPerformanceTests() {
    console.log('Starting Performance Tests for 241 Runners Awareness API');
    console.log(`API Base URL: ${API_BASE_URL}`);
    console.log(`Concurrent Requests: ${CONCURRENT_REQUESTS}`);
    console.log(`Total Requests per test: ${TOTAL_REQUESTS}`);
    console.log(`Request Timeout: ${REQUEST_TIMEOUT}ms`);
    console.log('\n' + '-'.repeat(60));
    
    const startTime = Date.now();
    
    for (const test of tests) {
        console.log(`\nTesting: ${test.name}`);
        console.log(`URL: ${test.url}`);
        
        const testStartTime = Date.now();
        
        // Run requests in batches
        const batches = Math.ceil(TOTAL_REQUESTS / CONCURRENT_REQUESTS);
        for (let batch = 0; batch < batches; batch++) {
            const remainingRequests = TOTAL_REQUESTS - (batch * CONCURRENT_REQUESTS);
            const batchSize = Math.min(CONCURRENT_REQUESTS, remainingRequests);
            
            await runConcurrentRequests(test, batchSize);
            
            // Progress indicator
            const completed = Math.min((batch + 1) * CONCURRENT_REQUESTS, TOTAL_REQUESTS);
            process.stdout.write(`\rProgress: ${completed}/${TOTAL_REQUESTS} requests completed`);
        }
        
        const testEndTime = Date.now();
        console.log(`\nTest completed in ${testEndTime - testStartTime}ms`);
    }
    
    const endTime = Date.now();
    console.log(`\n\nAll tests completed in ${endTime - startTime}ms`);
    
    printStats();
}

// Run the tests
if (require.main === module) {
    runPerformanceTests().catch(console.error);
}

module.exports = { runPerformanceTests, makeRequest, stats };
