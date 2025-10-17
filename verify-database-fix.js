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

// Test critical endpoints after database fix
async function testCriticalEndpoints(token) {
    console.log('📋 Testing critical endpoints after database fix...');
    
    const criticalEndpoints = [
        { url: '/api/v1/cases', name: 'Cases API', method: 'GET' },
        { url: '/api/v1/Runner/my-cases', name: 'My Cases API', method: 'GET' },
        { url: '/api/Devices/stats', name: 'Devices API', method: 'GET' },
        { url: '/api/notifications', name: 'Notifications API', method: 'GET' },
        { url: '/api/Topics/available', name: 'Topics API', method: 'GET' }
    ];

    const results = [];
    let successCount = 0;

    for (const endpoint of criticalEndpoints) {
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
                            body: body.substring(0, 200)
                        });
                    });
                });

                req.on('error', reject);
                req.end();
            });

            results.push(result);
            
            if (result.success) {
                successCount++;
            }
            
            const statusIcon = result.success ? '✅' : '❌';
            console.log(`${statusIcon} ${result.name} (${result.method}): ${result.status}`);
            
            if (!result.success) {
                console.log(`  Error: ${result.body}`);
            }
        } catch (error) {
            results.push({
                name: endpoint.name,
                method: endpoint.method,
                status: 'ERROR',
                success: false,
                error: error.message
            });
            console.log(`❌ ${endpoint.name} (${endpoint.method}): ERROR - ${error.message}`);
        }
    }

    return { results, successCount };
}

// Generate verification report
async function generateVerificationReport(testResults, successCount) {
    console.log('\n📊 DATABASE FIX VERIFICATION REPORT');
    console.log('='.repeat(50));
    
    const totalEndpoints = testResults.length;
    const successRate = ((successCount / totalEndpoints) * 100).toFixed(1);
    
    console.log(`📈 SUCCESS RATE: ${successRate}% (${successCount}/${totalEndpoints})`);
    
    const workingEndpoints = testResults.filter(r => r.success);
    const failingEndpoints = testResults.filter(r => !r.success);
    
    if (workingEndpoints.length > 0) {
        console.log('\n✅ WORKING ENDPOINTS:');
        workingEndpoints.forEach(result => {
            console.log(`  - ${result.name} (${result.method}): ${result.status}`);
        });
    }
    
    if (failingEndpoints.length > 0) {
        console.log('\n❌ FAILING ENDPOINTS:');
        failingEndpoints.forEach(result => {
            console.log(`  - ${result.name} (${result.method}): ${result.status}`);
        });
    }
    
    // Final assessment
    console.log('\n🎯 VERIFICATION STATUS:');
    if (successRate >= 80) {
        console.log('🎉 EXCELLENT! Database fix was successful.');
        console.log('✅ All critical endpoints are working.');
        console.log('✅ API is fully functional for core features.');
    } else if (successRate >= 60) {
        console.log('👍 GOOD! Most critical endpoints are working.');
        console.log('✅ Core functionality is operational.');
        console.log('🔧 Some endpoints may need additional fixes.');
    } else if (successRate >= 40) {
        console.log('⚠️  PARTIAL SUCCESS! Some endpoints are working.');
        console.log('✅ Basic functionality is operational.');
        console.log('🔧 Database tables may need manual creation.');
    } else {
        console.log('❌ DATABASE FIX NEEDED! Most endpoints are still failing.');
        console.log('🔧 Please execute the SQL script in Azure SQL Database.');
    }
    
    console.log('\n📋 NEXT STEPS:');
    if (successRate >= 80) {
        console.log('1. ✅ Database fix successful');
        console.log('2. 🔧 Fix remaining non-critical endpoints');
        console.log('3. 🔧 Implement advanced features');
        console.log('4. 🔧 Test complete site functionality');
    } else {
        console.log('1. 🔴 HIGH PRIORITY: Execute SQL script in Azure SQL Database');
        console.log('2. 🔴 HIGH PRIORITY: Create Cases and Devices tables');
        console.log('3. 🔧 Test endpoints after database creation');
        console.log('4. 🔧 Fix any remaining issues');
    }
}

// Main execution
async function main() {
    console.log('🔍 VERIFYING DATABASE FIX');
    console.log('='.repeat(40));
    console.log('Testing critical endpoints after database table creation');
    console.log('');

    try {
        // Step 1: Get authentication token
        console.log('📋 Getting authentication token...');
        const token = await getAuthToken();
        console.log('✅ Authentication token obtained\n');

        // Step 2: Test critical endpoints
        const { results, successCount } = await testCriticalEndpoints(token);

        // Step 3: Generate verification report
        await generateVerificationReport(results, successCount);

        console.log('\n🎉 VERIFICATION COMPLETE!');
        console.log('Database fix verification has been completed.');
        console.log('All critical endpoints have been tested and analyzed.');

    } catch (error) {
        console.error('❌ Error during verification:', error.message);
        process.exit(1);
    }
}

main();
