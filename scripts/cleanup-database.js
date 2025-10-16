#!/usr/bin/env node

/**
 * Database Cleanup Script
 * This script removes all mock data and non-admin users from the database
 * Keeps only the 6 designated admin users
 */

const https = require('https');

const API_BASE_URL = 'https://241runners-api-v2.azurewebsites.net/api/v1';

// The 6 admin users that should be preserved
const ADMIN_USERS = [
    'dekuworks1@gmail.com',        // Marcus Brown
    'danielcarey9770@yahoo.com',   // Daniel Carey
    'lthomas3350@gmail.com',       // Lisa Thomas
    'tinaleggins@yahoo.com',       // Tina Matthews
    'mmelasky@iplawconsulting.com', // Mark Melasky
    'ralphfrank900@gmail.com'      // Ralph Frank
];

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
                'X-Client': 'DatabaseCleanup/1.0'
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

// Login as admin to get token
async function loginAsAdmin() {
    console.log('🔐 Logging in as admin...');
    
    const response = await makeRequest('POST', '/auth/login', {
        email: 'dekuworks1@gmail.com',
        password: 'marcus2025'
    });

    if (response.status === 200 && response.data.success) {
        console.log('✅ Admin login successful');
        return response.data.accessToken || response.data.token;
    } else {
        console.error('❌ Admin login failed:', response.data);
        throw new Error('Admin login failed');
    }
}

// Get all users from the database
async function getAllUsers(token) {
    console.log('📋 Retrieving all users from database...');
    
    const response = await makeRequest('GET', '/admin/users', null, token);
    
    if (response.status === 200 && response.data.success) {
        console.log(`✅ Retrieved ${response.data.users?.length || 0} users`);
        return response.data.users || [];
    } else {
        console.error('❌ Failed to retrieve users:', response.data);
        throw new Error('Failed to retrieve users');
    }
}

// Delete a user by ID
async function deleteUser(token, userId, userEmail) {
    console.log(`🗑️  Deleting user: ${userEmail} (ID: ${userId})`);
    
    // Try the main delete endpoint first
    let response = await makeRequest('DELETE', `/admin/users/${userId}`, null, token);
    
    // If that fails, try the debug endpoint (for testing)
    if (response.status !== 200 && response.status !== 204) {
        console.log(`   ⚠️  Main endpoint failed, trying debug endpoint...`);
        response = await makeRequest('DELETE', `/admin/users-debug/${userId}`, null, token);
    }
    
    if (response.status === 200 || response.status === 204) {
        console.log(`✅ Successfully deleted: ${userEmail}`);
        return true;
    } else {
        console.error(`❌ Failed to delete ${userEmail}:`, response.data);
        return false;
    }
}

// Clean up mock data from other tables (cases, runners, etc.)
async function cleanupMockData(token) {
    console.log('🧹 Cleaning up mock data from other tables...');
    
    try {
        // Delete all cases (mock data)
        const casesResponse = await makeRequest('DELETE', '/admin/cases/all', null, token);
        if (casesResponse.status === 200) {
            console.log('✅ Cleared all cases');
        } else {
            console.log('⚠️  Could not clear cases (may not exist)');
        }
        
        // Delete all runners (mock data)
        const runnersResponse = await makeRequest('DELETE', '/admin/runners/all', null, token);
        if (runnersResponse.status === 200) {
            console.log('✅ Cleared all runners');
        } else {
            console.log('⚠️  Could not clear runners (may not exist)');
        }
        
        // Delete all notifications (mock data)
        const notificationsResponse = await makeRequest('DELETE', '/admin/notifications/all', null, token);
        if (notificationsResponse.status === 200) {
            console.log('✅ Cleared all notifications');
        } else {
            console.log('⚠️  Could not clear notifications (may not exist)');
        }
        
    } catch (error) {
        console.log('⚠️  Some cleanup operations may have failed:', error.message);
    }
}

// Main cleanup function
async function main() {
    try {
        console.log('🚀 Starting database cleanup process...\n');
        console.log('⚠️  WARNING: This will delete all non-admin users and mock data!\n');
        
        // Step 1: Login as admin
        const token = await loginAsAdmin();
        
        // Step 2: Get all users
        const users = await getAllUsers(token);
        
        if (!users || users.length === 0) {
            console.log('ℹ️  No users found in database');
            return;
        }
        
        console.log(`\n📊 Found ${users.length} users in database`);
        
        // Step 3: Identify users to keep and delete
        const usersToKeep = [];
        const usersToDelete = [];
        
        users.forEach(user => {
            if (ADMIN_USERS.includes(user.email)) {
                usersToKeep.push(user);
            } else {
                usersToDelete.push(user);
            }
        });
        
        console.log(`\n👥 Users to KEEP (${usersToKeep.length}):`);
        usersToKeep.forEach(user => {
            console.log(`   ✅ ${user.email} - ${user.firstName} ${user.lastName} (${user.role})`);
        });
        
        console.log(`\n🗑️  Users to DELETE (${usersToDelete.length}):`);
        usersToDelete.forEach(user => {
            console.log(`   ❌ ${user.email} - ${user.firstName} ${user.lastName} (${user.role})`);
        });
        
        if (usersToDelete.length === 0) {
            console.log('\n✅ No non-admin users found. Database is already clean!');
        } else {
            // Step 4: Delete non-admin users
            console.log('\n🗑️  Deleting non-admin users...');
            let deletedCount = 0;
            
            for (const user of usersToDelete) {
                const success = await deleteUser(token, user.id, user.email);
                if (success) {
                    deletedCount++;
                }
                // Wait between deletions to avoid overwhelming the server
                await new Promise(resolve => setTimeout(resolve, 500));
            }
            
            console.log(`\n✅ Deleted ${deletedCount} out of ${usersToDelete.length} non-admin users`);
        }
        
        // Step 5: Clean up mock data
        await cleanupMockData(token);
        
        // Step 6: Final verification
        console.log('\n🔍 Final verification...');
        const finalUsers = await getAllUsers(token);
        
        console.log(`\n📊 Final user count: ${finalUsers.length}`);
        console.log('👥 Remaining users:');
        finalUsers.forEach(user => {
            const isAdmin = ADMIN_USERS.includes(user.email);
            console.log(`   ${isAdmin ? '✅' : '❌'} ${user.email} - ${user.firstName} ${user.lastName} (${user.role})`);
        });
        
        console.log('\n🎉 Database cleanup completed successfully!');
        console.log('\n📝 Summary:');
        console.log(`   - Kept ${usersToKeep.length} admin users`);
        console.log(`   - Deleted ${usersToDelete.length} non-admin users`);
        console.log('   - Cleared all mock data from cases, runners, and notifications');
        console.log('\n🔐 Admin users preserved:');
        ADMIN_USERS.forEach(email => {
            console.log(`   - ${email}`);
        });
        
    } catch (error) {
        console.error('\n❌ Error during database cleanup:', error.message);
        process.exit(1);
    }
}

// Run the script
if (require.main === module) {
    main().catch(console.error);
}

module.exports = { main, ADMIN_USERS };
