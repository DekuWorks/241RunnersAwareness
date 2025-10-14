#!/usr/bin/env node

/**
 * Script to set up Lisa Thomas with dual admin/user roles
 * This allows her to function as both an admin and a regular user
 * so she can create runners and manage cases for her family
 */

const https = require('https');
const readline = require('readline');

// Configuration
const API_BASE_URL = 'https://241runners-api-v2.azurewebsites.net/api/v1';
const LISA_EMAIL = 'lthomas3350@gmail.com';
const LISA_PASSWORD = 'Lisa2025!';

// Helper function to make API requests
function makeRequest(method, endpoint, data = null, token = null) {
    return new Promise((resolve, reject) => {
        const url = new URL(`${API_BASE_URL}${endpoint}`);
        
        const options = {
            hostname: url.hostname,
            port: 443,
            path: url.pathname + url.search,
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'X-Client': 'DualRoleSetup/1.0'
            }
        };

        if (token) {
            options.headers['Authorization'] = `Bearer ${token}`;
        }

        const req = https.request(options, (res) => {
            let body = '';
            res.on('data', (chunk) => body += chunk);
            res.on('end', () => {
                try {
                    const response = JSON.parse(body);
                    resolve({ status: res.statusCode, data: response });
                } catch (e) {
                    resolve({ status: res.statusCode, data: body });
                }
            });
        });

        req.on('error', reject);

        if (data) {
            req.write(JSON.stringify(data));
        }

        req.end();
    });
}

// Login function
async function login(email, password) {
    console.log(`🔐 Logging in as ${email}...`);
    
    const response = await makeRequest('POST', '/auth/login', {
        email: email,
        password: password
    });

    if (response.status === 200 && response.data.success) {
        console.log('✅ Login successful');
        return response.data.token || response.data.accessToken;
    } else {
        console.error('❌ Login failed:', response.data);
        throw new Error('Login failed');
    }
}

// Update user with dual roles
async function updateUserWithDualRoles(token, userId, additionalRoles) {
    console.log(`🔄 Updating user ${userId} with additional roles: ${additionalRoles.join(', ')}...`);
    
    // First, let's try to update the user directly in the database
    // We'll use the admin update endpoint
    const response = await makeRequest('PUT', `/admin/users/${userId}`, {
        additionalRoles: JSON.stringify(additionalRoles)
    }, token);

    if (response.status === 200 && response.data.success) {
        console.log('✅ User updated successfully');
        return response.data;
    } else {
        console.error('❌ Update failed:', response.data);
        console.log('📝 Note: You may need to manually update the database or use a different approach');
        throw new Error('Update failed');
    }
}

// Get user info
async function getUserInfo(token) {
    console.log('👤 Getting current user info...');
    
    const response = await makeRequest('GET', '/auth/me', null, token);

    if (response.status === 200 && response.data.success) {
        console.log('✅ User info retrieved');
        return response.data.user;
    } else {
        console.error('❌ Failed to get user info:', response.data);
        throw new Error('Failed to get user info');
    }
}

// Main function
async function main() {
    try {
        console.log('🚀 Setting up Lisa Thomas with dual admin/user roles...\n');

        // Step 1: Login as Lisa
        const token = await login(LISA_EMAIL, LISA_PASSWORD);

        // Step 2: Get user info
        const userInfo = await getUserInfo(token);
        console.log(`📋 Current user info:`);
        console.log(`   - Name: ${userInfo.fullName}`);
        console.log(`   - Email: ${userInfo.email}`);
        console.log(`   - Current Role: ${userInfo.role}`);
        console.log(`   - User ID: ${userInfo.id}\n`);

        // Step 3: Update with additional roles
        // Lisa will have admin as primary role, but also user role for family management
        const additionalRoles = ['user', 'parent']; // She can be a user and parent for her family
        await updateUserWithDualRoles(token, userInfo.id, additionalRoles);

        // Step 4: Verify the update
        console.log('\n🔍 Verifying dual role setup...');
        const updatedUserInfo = await getUserInfo(token);
        
        console.log(`📋 Updated user info:`);
        console.log(`   - Name: ${updatedUserInfo.fullName}`);
        console.log(`   - Email: ${updatedUserInfo.email}`);
        console.log(`   - Primary Role: ${updatedUserInfo.role}`);
        console.log(`   - All Roles: ${updatedUserInfo.allRoles ? updatedUserInfo.allRoles.join(', ') : 'Not available'}`);
        console.log(`   - Primary User Role: ${updatedUserInfo.primaryUserRole || 'Not available'}`);
        console.log(`   - Is Admin User: ${updatedUserInfo.isAdminUser || 'Not available'}`);

        console.log('\n🎉 Dual role setup completed successfully!');
        console.log('\n📝 What this means:');
        console.log('   - Lisa can still access the admin dashboard with full admin privileges');
        console.log('   - Lisa can also function as a regular user to create and manage runners');
        console.log('   - Lisa can act as a parent for her family members with special needs');
        console.log('   - She can view and manage cases like any other user');
        console.log('\n🔗 Next steps:');
        console.log('   1. Test admin login at: https://241runnersawareness.org/admin/login.html');
        console.log('   2. Test user features at: https://241runnersawareness.org/login.html');
        console.log('   3. Lisa should be able to access both admin and user functionality');

    } catch (error) {
        console.error('\n❌ Error setting up dual roles:', error.message);
        process.exit(1);
    }
}

// Run the script
if (require.main === module) {
    main().catch(console.error);
}

module.exports = { main };
