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

// Apply database migration
async function applyDatabaseMigration(token) {
    return new Promise((resolve, reject) => {
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
                try {
                    const data = JSON.parse(body);
                    resolve(data);
                } catch (e) {
                    resolve({ message: body });
                }
            });
        });

        req.on('error', reject);
        req.end();
    });
}

// Test endpoints after migration
async function testEndpoints(token) {
    const endpoints = [
        '/api/v1/cases',
        '/api/v1/Runner/my-cases',
        '/api/v1/enhanced-runner',
        '/api/notifications',
        '/api/Topics/available',
        '/api/Devices/stats',
        '/api/v1/Database/health',
        '/api/v1/Security/health',
        '/api/v1/Monitoring/health'
    ];

    const results = [];

    for (const endpoint of endpoints) {
        try {
            const result = await new Promise((resolve, reject) => {
                const options = {
                    hostname: '241runners-api-v2.azurewebsites.net',
                    port: 443,
                    path: endpoint,
                    method: 'GET',
                    headers: {
                        'Authorization': `Bearer ${token}`
                    }
                };

                const req = https.request(options, (res) => {
                    let body = '';
                    res.on('data', (chunk) => body += chunk);
                    res.on('end', () => {
                        resolve({
                            endpoint,
                            status: res.statusCode,
                            success: res.statusCode >= 200 && res.statusCode < 300
                        });
                    });
                });

                req.on('error', reject);
                req.end();
            });

            results.push(result);
            console.log(`${result.success ? '✅' : '❌'} ${endpoint}: ${result.status}`);
        } catch (error) {
            results.push({
                endpoint,
                status: 'ERROR',
                success: false,
                error: error.message
            });
            console.log(`❌ ${endpoint}: ERROR - ${error.message}`);
        }
    }

    return results;
}

// Main execution
async function main() {
    console.log('🚀 Starting comprehensive database fix...\n');

    try {
        // Step 1: Get authentication token
        console.log('📋 Step 1: Getting authentication token...');
        const token = await getAuthToken();
        console.log('✅ Authentication token obtained\n');

        // Step 2: Apply OAuth migration
        console.log('📋 Step 2: Applying OAuth migration...');
        const migrationResult = await applyDatabaseMigration(token);
        console.log('✅ OAuth migration applied:', migrationResult.message, '\n');

        // Step 3: Test endpoints
        console.log('📋 Step 3: Testing endpoints after migration...');
        const testResults = await testEndpoints(token);

        // Step 4: Summary
        console.log('\n📊 TEST RESULTS SUMMARY');
        console.log('='.repeat(50));
        
        const successCount = testResults.filter(r => r.success).length;
        const totalCount = testResults.length;
        const successRate = ((successCount / totalCount) * 100).toFixed(1);
        
        console.log(`✅ Successful: ${successCount}/${totalCount} (${successRate}%)`);
        console.log(`❌ Failed: ${totalCount - successCount}/${totalCount}`);
        
        if (successCount < totalCount) {
            console.log('\n❌ FAILED ENDPOINTS:');
            testResults.filter(r => !r.success).forEach(result => {
                console.log(`  - ${result.endpoint}: ${result.status}`);
            });
        }

        console.log('\n🎯 NEXT STEPS:');
        if (successRate >= 80) {
            console.log('✅ Database migration successful! Most endpoints are working.');
            console.log('🔧 Next: Implement missing CRUD operations and file upload.');
        } else {
            console.log('⚠️  Some endpoints still failing. May need additional database fixes.');
            console.log('🔧 Next: Check database schema and add missing tables/columns.');
        }

    } catch (error) {
        console.error('❌ Error during database fix:', error.message);
        process.exit(1);
    }
}

main();
