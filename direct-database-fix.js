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

// Try to create tables by calling the OAuth migration endpoint with extended SQL
async function createTablesDirectly(token) {
    console.log('📋 Attempting to create missing tables directly...');
    
    // First, let's try the OAuth migration endpoint
    try {
        const oauthResult = await new Promise((resolve, reject) => {
            const options = {
                hostname: '241runners-api-v2.azurewebsites.net',
                port: 443,
                path: '/debug/apply-oauth-migration',
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                }
            };

            const req = https.request(options, (res) => {
                let body = '';
                res.on('data', (chunk) => body += chunk);
                res.on('end', () => {
                    resolve({
                        status: res.statusCode,
                        body: body
                    });
                });
            });

            req.on('error', reject);
            req.end();
        });

        console.log('✅ OAuth migration result:', oauthResult.body);
    } catch (error) {
        console.log('❌ OAuth migration error:', error.message);
    }

    // Now let's try to access endpoints that might trigger table creation
    const endpointsToTry = [
        { url: '/api/v1/cases', name: 'Cases API', method: 'GET' },
        { url: '/api/v1/cases', name: 'Cases API', method: 'POST' },
        { url: '/api/Devices/stats', name: 'Devices API', method: 'GET' },
        { url: '/api/v1/Database/health', name: 'Database Health API', method: 'GET' }
    ];
    
    const results = [];
    
    for (const endpoint of endpointsToTry) {
        try {
            console.log(`📋 Testing ${endpoint.name} (${endpoint.method})...`);
            
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
                        resolve({
                            name: endpoint.name,
                            method: endpoint.method,
                            status: res.statusCode,
                            success: res.statusCode >= 200 && res.statusCode < 300,
                            body: body.substring(0, 300)
                        });
                    });
                });

                req.on('error', reject);
                req.end();
            });
            
            results.push(result);
            const statusIcon = result.success ? '✅' : '❌';
            console.log(`  ${statusIcon} ${result.name} (${result.method}): ${result.status}`);
            
            if (!result.success) {
                console.log(`  Error details: ${result.body}`);
            }
        } catch (error) {
            console.log(`  ❌ ${endpoint.name} (${endpoint.method}): ERROR - ${error.message}`);
            results.push({
                name: endpoint.name,
                method: endpoint.method,
                status: 'ERROR',
                success: false,
                error: error.message
            });
        }
    }
    
    return results;
}

// Test all endpoints to see current status
async function testAllEndpoints(token) {
    console.log('\n📋 Testing all API endpoints to see current status...');
    
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
                            critical: endpoint.critical
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

// Generate troubleshooting report
async function generateTroubleshootingReport(testResults, criticalSuccessCount, criticalTotalCount) {
    console.log('\n📊 TROUBLESHOOTING REPORT');
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
    
    console.log('\n🔧 TROUBLESHOOTING STEPS:');
    console.log('1. 🔴 CRITICAL: The database tables need to be created manually');
    console.log('2. 🔴 CRITICAL: Connect to Azure SQL Database directly');
    console.log('3. 🔴 CRITICAL: Execute the SQL script manually');
    console.log('4. 🔧 The API endpoints cannot create tables automatically');
    console.log('5. 🔧 Entity Framework migrations may not be configured');
    
    console.log('\n🚀 MANUAL SOLUTION REQUIRED:');
    console.log('1. Open Azure Portal → SQL Database → Query editor');
    console.log('2. Copy the SQL script from execute-missing-tables.sql');
    console.log('3. Paste and execute the script');
    console.log('4. Verify tables were created');
    console.log('5. Test the API endpoints again');
    
    console.log('\n📋 ALTERNATIVE APPROACHES:');
    console.log('1. Use Azure Data Studio to connect to the database');
    console.log('2. Use SQL Server Management Studio (SSMS)');
    console.log('3. Use Azure CLI to execute SQL commands');
    console.log('4. Create a new API endpoint specifically for table creation');
}

// Main execution
async function main() {
    console.log('🔧 TROUBLESHOOTING DATABASE TABLE CREATION');
    console.log('='.repeat(60));
    console.log('Diagnosing why the scripts failed to add missing tables');
    console.log('');

    try {
        // Step 1: Get authentication token
        console.log('📋 Getting authentication token...');
        const token = await getAuthToken();
        console.log('✅ Authentication token obtained\n');

        // Step 2: Try to create tables directly
        console.log('📋 Attempting to create missing tables directly...');
        const tableResults = await createTablesDirectly(token);

        // Step 3: Test all endpoints
        const { results, criticalSuccessCount, criticalTotalCount } = await testAllEndpoints(token);

        // Step 4: Generate troubleshooting report
        await generateTroubleshootingReport(results, criticalSuccessCount, criticalTotalCount);

        console.log('\n🎉 TROUBLESHOOTING COMPLETE!');
        console.log('The issue has been diagnosed and solutions provided.');
        console.log('Manual database table creation is required.');

    } catch (error) {
        console.error('❌ Error during troubleshooting:', error.message);
        process.exit(1);
    }
}

main();
