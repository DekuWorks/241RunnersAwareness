#!/usr/bin/env node

const https = require('https');

const API_BASE = 'https://241runners-api-v2.azurewebsites.net';

// Get authentication token
async function getAuthToken() {
    return new Promise((resolve, reject) => {
        const options = {
            hostname: '241runners-api-v2.azurewebsites.net',
            port: 443,
            path: '/api/v1/auth/login',
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        };

        const req = https.request(options, (res) => {
            let body = '';
            res.on('data', (chunk) => body += chunk);
            res.on('end', () => {
                try {
                    const data = JSON.parse(body);
                    if (data.accessToken) {
                        resolve(data.accessToken);
                    } else {
                        reject(new Error('No token received'));
                    }
                } catch (e) {
                    reject(e);
                }
            });
        });

        req.on('error', reject);
        req.write(JSON.stringify({
            email: 'dekuworks1@gmail.com',
            password: 'marcus2025'
        }));
        req.end();
    });
}

// Check current status of all endpoints
async function checkCurrentStatus(token) {
    console.log('📋 Checking current status of all API endpoints...');
    
    const endpoints = [
        { url: '/api/v1/cases', name: 'Cases API', method: 'GET', critical: true },
        { url: '/api/v1/Runner/my-cases', name: 'My Cases API', method: 'GET', critical: true },
        { url: '/api/v1/enhanced-runner', name: 'Enhanced Runner API', method: 'GET', critical: false },
        { url: '/api/notifications', name: 'Notifications API', method: 'GET', critical: true },
        { url: '/api/Topics/available', name: 'Topics API', method: 'GET', critical: true },
        { url: '/api/Devices/stats', name: 'Devices Stats API', method: 'GET', critical: false },
        { url: '/api/v1/Database/health', name: 'Database Health API', method: 'GET', critical: false },
        { url: '/api/v1/Security/health', name: 'Security Health API', method: 'GET', critical: false },
        { url: '/api/v1/Monitoring/health', name: 'Monitoring Health API', method: 'GET', critical: false }
    ];

    const results = [];
    let criticalSuccessCount = 0;
    let criticalTotalCount = 0;

    for (const endpoint of endpoints) {
        try {
            const result = await new Promise((resolve, reject) => {
                const options = {
                    hostname: '241runners-api-v2.azurewebsites.net',
                    port: 443,
                    path: endpoint.url,
                    method: endpoint.method,
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    }
                };

                const req = https.request(options, (res) => {
                    let body = '';
                    res.on('data', (chunk) => body += chunk);
                    res.on('end', () => {
                        const isSuccess = res.statusCode >= 200 && res.statusCode < 300;
                        resolve({
                            name: endpoint.name,
                            method: endpoint.method,
                            status: res.statusCode,
                            success: isSuccess,
                            critical: endpoint.critical,
                            body: body.substring(0, 200)
                        });
                    });
                });

                req.on('error', reject);
                req.end();
            });

            results.push(result);
            
            if (endpoint.critical) {
                criticalTotalCount++;
                if (result.success) criticalSuccessCount++;
            }
            
            const statusIcon = result.success ? '✅' : '❌';
            const criticalIcon = endpoint.critical ? '🔴' : '🟡';
            console.log(`${statusIcon} ${criticalIcon} ${result.name} (${result.method}): ${result.status}`);
            
            if (!result.success) {
                console.log(`  Error: ${result.body}`);
            }
        } catch (error) {
            results.push({
                name: endpoint.name,
                method: endpoint.method,
                status: 'ERROR',
                success: false,
                critical: endpoint.critical,
                error: error.message
            });
            console.log(`❌ ${endpoint.critical ? '🔴' : '🟡'} ${endpoint.name} (${endpoint.method}): ERROR - ${error.message}`);
        }
    }

    return { results, criticalSuccessCount, criticalTotalCount };
}

// Generate status report
async function generateStatusReport(testResults, criticalSuccessCount, criticalTotalCount) {
    console.log('\n📊 CURRENT STATUS REPORT');
    console.log('='.repeat(60));
    
    const workingEndpoints = testResults.filter(r => r.success);
    const failingEndpoints = testResults.filter(r => !r.success);
    const criticalEndpoints = testResults.filter(r => r.critical);
    const criticalWorking = criticalEndpoints.filter(r => r.success);
    
    const overallSuccessRate = ((workingEndpoints.length / testResults.length) * 100).toFixed(1);
    const criticalSuccessRate = criticalTotalCount > 0 ? ((criticalSuccessCount / criticalTotalCount) * 100).toFixed(1) : 0;
    
    console.log(`📈 OVERALL SUCCESS RATE: ${overallSuccessRate}% (${workingEndpoints.length}/${testResults.length})`);
    console.log(`🔴 CRITICAL SUCCESS RATE: ${criticalSuccessRate}% (${criticalSuccessCount}/${criticalTotalCount})`);
    
    if (criticalWorking.length > 0) {
        console.log('\n✅ WORKING CRITICAL ENDPOINTS:');
        criticalWorking.forEach(result => {
            console.log(`  - ${result.name} (${result.method}): ${result.status}`);
        });
    }
    
    const criticalFailing = criticalEndpoints.filter(r => !r.success);
    if (criticalFailing.length > 0) {
        console.log('\n❌ FAILING CRITICAL ENDPOINTS:');
        criticalFailing.forEach(result => {
            console.log(`  - ${result.name} (${result.method}): ${result.status}`);
        });
    }
    
    console.log('\n🔧 NEXT STEPS:');
    if (criticalSuccessRate < 80) {
        console.log('1. 🔴 HIGH PRIORITY: Create missing database tables manually');
        console.log('2. 🔴 HIGH PRIORITY: Use step-by-step approach in step-by-step-database-fix.md');
        console.log('3. 🔧 Create tables one by one to identify errors');
        console.log('4. 🔧 Test after each table creation');
    } else {
        console.log('1. ✅ Core functionality is working');
        console.log('2. 🔧 Fix remaining non-critical endpoints');
        console.log('3. 🔧 Test complete site functionality');
    }
}

// Main execution
async function main() {
    console.log('🔍 CHECKING CURRENT API STATUS');
    console.log('='.repeat(50));
    console.log('Testing all endpoints to see current status');
    console.log('');

    try {
        // Step 1: Get authentication token
        console.log('📋 Getting authentication token...');
        const token = await getAuthToken();
        console.log('✅ Authentication token obtained\n');

        // Step 2: Check current status
        const { results, criticalSuccessCount, criticalTotalCount } = await checkCurrentStatus(token);

        // Step 3: Generate status report
        await generateStatusReport(results, criticalSuccessCount, criticalTotalCount);

        console.log('\n🎉 STATUS CHECK COMPLETE!');
        console.log('Current API status has been analyzed.');
        console.log('Use the step-by-step approach to fix any issues.');

    } catch (error) {
        console.error('❌ Error during status check:', error.message);
        process.exit(1);
    }
}

main();
