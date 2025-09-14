#!/usr/bin/env node

/**
 * Script to reset Marcus's admin password
 */

const https = require('https');

async function resetMarcusPassword() {
    return new Promise((resolve, reject) => {
        const options = {
            hostname: '241runners-api-v2.azurewebsites.net',
            port: 443,
            path: '/api/auth/reset-marcus-password',
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        };

        const req = https.request(options, (res) => {
            let data = '';
            
            res.on('data', (chunk) => {
                data += chunk;
            });
            
            res.on('end', () => {
                try {
                    const response = JSON.parse(data);
                    resolve({ status: res.statusCode, data: response });
                } catch (e) {
                    resolve({ status: res.statusCode, data: data });
                }
            });
        });

        req.on('error', (e) => {
            reject(e);
        });

        req.end();
    });
}

async function testLogin(email, password) {
    return new Promise((resolve, reject) => {
        const postData = JSON.stringify({ email, password });
        
        const options = {
            hostname: '241runners-api-v2.azurewebsites.net',
            port: 443,
            path: '/api/auth/login',
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Content-Length': Buffer.byteLength(postData)
            }
        };

        const req = https.request(options, (res) => {
            let data = '';
            
            res.on('data', (chunk) => {
                data += chunk;
            });
            
            res.on('end', () => {
                try {
                    const response = JSON.parse(data);
                    resolve({ status: res.statusCode, data: response });
                } catch (e) {
                    resolve({ status: res.statusCode, data: data });
                }
            });
        });

        req.on('error', (e) => {
            reject(e);
        });

        req.write(postData);
        req.end();
    });
}

async function main() {
    console.log('🔧 Resetting Marcus admin password...\n');
    
    try {
        const result = await resetMarcusPassword();
        
        if (result.status === 200) {
            console.log(`✅ Password reset successful:`, result.data);
        } else {
            console.log(`❌ Password reset failed:`, result.data);
            console.log(`Status: ${result.status}`);
        }
    } catch (error) {
        console.log(`❌ Error resetting password:`, error.message);
    }
    
    console.log('\n🔐 Testing Marcus login after password reset...\n');
    
    try {
        const result = await testLogin('dekuworks1@gmail.com', 'marcus2025');
        
        if (result.status === 200) {
            console.log(`✅ Login successful: dekuworks1@gmail.com`);
            console.log(`🎉 Marcus admin account is working!`);
        } else {
            console.log(`❌ Login failed:`, result.data?.error?.message || 'Unknown error');
        }
    } catch (error) {
        console.log(`❌ Error testing login:`, error.message);
    }
    
    console.log('\n✨ Marcus password reset process completed!');
}

main().catch(console.error);
