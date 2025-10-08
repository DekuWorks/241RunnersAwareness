#!/usr/bin/env node

/**
 * Script to manually create admin users in the database
 * This is a temporary solution to ensure admin users exist
 */

const https = require('https');

const API_BASE_URL = 'https://241runners-api-v2.azurewebsites.net';

const adminUsers = [
    {
        email: 'dekuworks1@gmail.com',
        password: 'marcus2025',
        firstName: 'Marcus',
        lastName: 'Brown'
    },
    {
        email: 'danielcarey9770@yahoo.com',
        password: 'Daniel2025!',
        firstName: 'Daniel',
        lastName: 'Carey'
    },
    {
        email: 'lthomas3350@gmail.com',
        password: 'Lisa2025!',
        firstName: 'Lisa',
        lastName: 'Thomas'
    },
    {
        email: 'tinaleggins@yahoo.com',
        password: 'Tina2025!',
        firstName: 'Tina',
        lastName: 'Matthews'
    },
    {
        email: 'mmelasky@iplawconsulting.com',
        password: 'Mark2025!',
        firstName: 'Mark',
        lastName: 'Melasky'
    },
    {
        email: 'ralphfrank900@gmail.com',
        password: 'Ralph2025!',
        firstName: 'Ralph',
        lastName: 'Frank'
    }
];

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
    console.log('🚀 Starting admin user creation process...\n');
    
    for (const user of adminUsers) {
        console.log(`📝 Creating admin user: ${user.email}`);
        
        try {
            const result = await createAdminUser(user);
            
            if (result.status === 200 || result.status === 201) {
                console.log(`✅ Successfully created: ${user.email}`);
            } else if (result.status === 400 && result.data?.error?.code === 'USER_EXISTS') {
                console.log(`ℹ️  User already exists: ${user.email}`);
            } else {
                console.log(`❌ Failed to create ${user.email}:`, result.data);
            }
        } catch (error) {
            console.log(`❌ Error creating ${user.email}:`, error.message);
        }
        
        // Wait a bit between requests
        await new Promise(resolve => setTimeout(resolve, 1000));
    }
    
    console.log('\n🔐 Testing admin logins...\n');
    
    for (const user of adminUsers) {
        console.log(`🔑 Testing login for: ${user.email}`);
        
        try {
            const result = await testLogin(user.email, user.password);
            
            if (result.status === 200) {
                console.log(`✅ Login successful: ${user.email}`);
            } else {
                console.log(`❌ Login failed for ${user.email}:`, result.data?.error?.message || 'Unknown error');
            }
        } catch (error) {
            console.log(`❌ Error testing login for ${user.email}:`, error.message);
        }
        
        // Wait a bit between requests
        await new Promise(resolve => setTimeout(resolve, 1000));
    }
    
    console.log('\n✨ Admin user creation process completed!');
}

main().catch(console.error);
