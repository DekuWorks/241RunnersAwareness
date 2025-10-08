#!/usr/bin/env node

/**
 * Script to add Marcus's admin account back to the database
 */

const https = require('https');

const API_BASE_URL = 'https://241runners-api-v2.azurewebsites.net';

const marcusAdmin = {
    email: 'dekuworks1@gmail.com',
    password: 'marcus2025',
    firstName: 'Marcus',
    lastName: 'Brown'
};

async function createAdminUser(userData) {
    return new Promise((resolve, reject) => {
        const postData = JSON.stringify(userData);
        
        const options = {
            hostname: '241runners-api-v2.azurewebsites.net',
            port: 443,
            path: '/api/auth/create-admin',
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
    console.log('🚀 Adding Marcus admin account back to database...\n');
    
    console.log(`📝 Creating admin user: ${marcusAdmin.email}`);
    
    try {
        const result = await createAdminUser(marcusAdmin);
        
        if (result.status === 200 || result.status === 201) {
            console.log(`✅ Successfully created: ${marcusAdmin.email}`);
        } else if (result.status === 400 && result.data?.error?.code === 'USER_EXISTS') {
            console.log(`ℹ️  User already exists: ${marcusAdmin.email}`);
        } else {
            console.log(`❌ Failed to create ${marcusAdmin.email}:`, result.data);
            console.log(`Status: ${result.status}`);
        }
    } catch (error) {
        console.log(`❌ Error creating ${marcusAdmin.email}:`, error.message);
    }
    
    console.log('\n🔐 Testing Marcus login...\n');
    
    try {
        const result = await testLogin(marcusAdmin.email, marcusAdmin.password);
        
        if (result.status === 200) {
            console.log(`✅ Login successful: ${marcusAdmin.email}`);
            console.log(`🎉 Marcus admin account is working!`);
        } else {
            console.log(`❌ Login failed for ${marcusAdmin.email}:`, result.data?.error?.message || 'Unknown error');
        }
    } catch (error) {
        console.log(`❌ Error testing login for ${marcusAdmin.email}:`, error.message);
    }
    
    console.log('\n✨ Marcus admin account process completed!');
}

main().catch(console.error);
