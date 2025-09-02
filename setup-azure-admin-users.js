#!/usr/bin/env node

/**
 * 241 Runners Awareness - Azure Admin User Setup Script
 * 
 * This script creates the initial admin users for the Azure backend deployment.
 * Run this script after the backend is deployed to Azure.
 * 
 * Usage: node setup-azure-admin-users.js
 */

const https = require('https');

// Configuration
const API_BASE_URL = 'https://241runners-api.azurewebsites.net';
const ADMIN_USERS = [
    {
        email: 'dekuworks1@gmail.com',
        password: 'marcus2025',
        firstName: 'Marcus',
        lastName: 'Brown',
        role: 'admin'
    },
    {
        email: 'danielcarey9770@gmail.com',
        password: 'daniel2025',
        firstName: 'Daniel',
        lastName: 'Carey',
        role: 'admin'
    },
    {
        email: 'danielcarey9770@yahoo.com',
        password: 'daniel2025',
        firstName: 'Daniel',
        lastName: 'Carey',
        role: 'admin'
    }
];

/**
 * Make HTTPS request to the API
 */
function makeRequest(path, method = 'GET', data = null) {
    return new Promise((resolve, reject) => {
        const options = {
            hostname: '241runners-api.azurewebsites.net',
            port: 443,
            path: path,
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'User-Agent': '241RunnersAwareness-Setup/1.0'
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
                    const parsed = JSON.parse(responseData);
                    resolve({ status: res.statusCode, data: parsed });
                } catch (e) {
                    resolve({ status: res.statusCode, data: responseData });
                }
            });
        });

        req.on('error', (error) => {
            reject(error);
        });

        if (data) {
            req.write(JSON.stringify(data));
        }

        req.end();
    });
}

/**
 * Test API health
 */
async function testAPIHealth() {
    console.log('🔍 Testing API health...');
    
    try {
        const response = await makeRequest('/api/auth/health');
        if (response.status === 200) {
            console.log('✅ API is healthy and responding');
            console.log(`   Database: ${response.data.database}`);
            console.log(`   Users: ${response.data.users}`);
            console.log(`   Runners: ${response.data.runners}`);
            return true;
        } else {
            console.log(`❌ API health check failed: ${response.status}`);
            return false;
        }
    } catch (error) {
        console.log(`❌ API health check error: ${error.message}`);
        return false;
    }
}

/**
 * Create admin user
 */
async function createAdminUser(userData) {
    console.log(`👤 Creating admin user: ${userData.email}`);
    
    try {
        const response = await makeRequest('/api/auth/create-first-admin', 'POST', userData);
        
        if (response.status === 200) {
            console.log(`✅ Admin user created successfully: ${userData.email}`);
            return true;
        } else if (response.status === 400 && response.data.message && response.data.message.includes('already exist')) {
            console.log(`ℹ️  Admin users already exist, using regular admin creation endpoint`);
            
            // Try regular admin creation (requires existing admin auth)
            const adminResponse = await makeRequest('/api/auth/create-admin', 'POST', userData);
            if (adminResponse.status === 200) {
                console.log(`✅ Admin user created via regular endpoint: ${userData.email}`);
                return true;
            } else {
                console.log(`⚠️  Regular admin creation failed: ${adminResponse.data.message || 'Unknown error'}`);
                return false;
            }
        } else {
            console.log(`❌ Failed to create admin user: ${response.data.message || 'Unknown error'}`);
            return false;
        }
    } catch (error) {
        console.log(`❌ Error creating admin user: ${error.message}`);
        return false;
    }
}

/**
 * Main setup function
 */
async function setupAdminUsers() {
    console.log('🚀 241 Runners Awareness - Azure Admin User Setup');
    console.log('================================================\n');
    
    // Test API health first
    const isHealthy = await testAPIHealth();
    if (!isHealthy) {
        console.log('\n❌ Cannot proceed - API is not healthy');
        console.log('Please ensure the Azure backend is deployed and running.');
        process.exit(1);
    }
    
    console.log('\n👥 Setting up admin users...\n');
    
    let successCount = 0;
    let totalCount = ADMIN_USERS.length;
    
    for (const user of ADMIN_USERS) {
        const success = await createAdminUser(user);
        if (success) successCount++;
        
        // Add delay between requests
        await new Promise(resolve => setTimeout(resolve, 1000));
    }
    
    console.log('\n📊 Setup Summary:');
    console.log(`   Total users: ${totalCount}`);
    console.log(`   Successful: ${successCount}`);
    console.log(`   Failed: ${totalCount - successCount}`);
    
    if (successCount === totalCount) {
        console.log('\n🎉 All admin users created successfully!');
        console.log('\n📋 Admin User Credentials:');
        ADMIN_USERS.forEach(user => {
            console.log(`   Email: ${user.email}`);
            console.log(`   Password: ${user.password}`);
            console.log(`   Role: ${user.role}\n`);
        });
        
        console.log('🔗 You can now login at: https://241runnersawareness.org/admin');
    } else {
        console.log('\n⚠️  Some admin users failed to create. Check the logs above.');
        process.exit(1);
    }
}

/**
 * Handle script execution
 */
if (require.main === module) {
    setupAdminUsers().catch(error => {
        console.error('\n💥 Setup failed with error:', error.message);
        process.exit(1);
    });
}

module.exports = { setupAdminUsers, makeRequest }; 