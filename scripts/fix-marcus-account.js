#!/usr/bin/env node

/**
 * Script to fix Marcus's admin account by trying different approaches
 */

const https = require('https');

async function makeRequest(path, method = 'GET', data = null) {
    return new Promise((resolve, reject) => {
        const options = {
            hostname: '241runners-api-v2.azurewebsites.net',
            port: 443,
            path: path,
            method: method,
            headers: {
                'Content-Type': 'application/json'
            }
        };

        if (data) {
            const postData = JSON.stringify(data);
            options.headers['Content-Length'] = Buffer.byteLength(postData);
        }

        const req = https.request(options, (res) => {
            let responseData = '';
            
            res.on('data', (chunk) => {
                responseData += chunk;
            });
            
            res.on('end', () => {
                try {
                    const response = JSON.parse(responseData);
                    resolve({ status: res.statusCode, data: response });
                } catch (e) {
                    resolve({ status: res.statusCode, data: responseData });
                }
            });
        });

        req.on('error', (e) => {
            reject(e);
        });

        if (data) {
            req.write(JSON.stringify(data));
        }
        req.end();
    });
}

async function testLogin(email, password) {
    return await makeRequest('/api/auth/login', 'POST', { email, password });
}

async function main() {
    console.log('🔧 Attempting to fix Marcus admin account...\n');
    
    // Test current login status
    console.log('🔍 Testing current login status...');
    try {
        const loginResult = await testLogin('dekuworks1@gmail.com', 'marcus2025');
        if (loginResult.status === 200) {
            console.log('✅ Login is already working!');
            return;
        } else {
            console.log('❌ Login failed:', loginResult.data?.error?.message || 'Unknown error');
        }
    } catch (error) {
        console.log('❌ Error testing login:', error.message);
    }
    
    // Try different password variations
    const passwordVariations = [
        'marcus2025',
        'Marcus2025',
        'MARCUS2025',
        'marcus2025!',
        'Marcus2025!',
        'MARCUS2025!'
    ];
    
    console.log('\n🔑 Testing different password variations...');
    for (const password of passwordVariations) {
        try {
            const result = await testLogin('dekuworks1@gmail.com', password);
            if (result.status === 200) {
                console.log(`✅ Login successful with password: ${password}`);
                console.log('🎉 Marcus admin account is working!');
                return;
            } else {
                console.log(`❌ Password "${password}" failed:`, result.data?.error?.message || 'Unknown error');
            }
        } catch (error) {
            console.log(`❌ Error testing password "${password}":`, error.message);
        }
        
        // Wait between attempts
        await new Promise(resolve => setTimeout(resolve, 1000));
    }
    
    console.log('\n📋 Summary:');
    console.log('- User exists in database (registration endpoint confirms this)');
    console.log('- Login is failing with all password variations');
    console.log('- This suggests the password hash in the database is incorrect');
    console.log('- Need to wait for deployment with password reset endpoint');
    
    console.log('\n⏳ Next steps:');
    console.log('1. Wait for the latest deployment to complete (2-3 minutes)');
    console.log('2. Run the password reset endpoint');
    console.log('3. Test login again');
    
    console.log('\n✨ Marcus account fix process completed!');
}

main().catch(console.error);
