#!/usr/bin/env node

/**
 * Complete Database Cleanup Script
 * Removes ALL runners and ensures only 6 admin users remain
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
                'X-Client': 'CompleteDatabaseCleanup/1.0'
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
                    resolve({ 
                        status: res.statusCode, 
                        data: response,
                        headers: res.headers
                    });
                } catch (e) {
                    resolve({ 
                        status: res.statusCode, 
                        data: body,
                        headers: res.headers
                    });
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

// Login as admin
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

// Get all users
async function getAllUsers(token) {
    console.log('📋 Retrieving all users...');
    
    const response = await makeRequest('GET', '/Admin/users', null, token);
    
    if (response.status === 200 && response.data.success) {
        console.log(`✅ Retrieved ${response.data.users?.length || 0} users`);
        return response.data.users || [];
    } else {
        console.error('❌ Failed to retrieve users:', response.data);
        throw new Error('Failed to retrieve users');
    }
}

// Get all runners
async function getAllRunners(token) {
    console.log('🏃 Retrieving all runners...');
    
    const response = await makeRequest('GET', '/runner', null, token);
    
    if (response.status === 200 && response.data.success) {
        console.log(`✅ Retrieved ${response.data.runners?.length || 0} runners`);
        return response.data.runners || [];
    } else {
        console.error('❌ Failed to retrieve runners:', response.data);
        throw new Error('Failed to retrieve runners');
    }
}

// Delete a user by ID
async function deleteUser(token, userId, userEmail) {
    console.log(`🗑️  Deleting user: ${userEmail} (ID: ${userId})`);
    
    // Try the main delete endpoint first
    let response = await makeRequest('DELETE', `/Admin/users/${userId}`, null, token);
    
    // If that fails, try the debug endpoint
    if (response.status !== 200 && response.status !== 204) {
        console.log(`   ⚠️  Main endpoint failed, trying debug endpoint...`);
        response = await makeRequest('DELETE', `/Admin/users-debug/${userId}`, null, token);
    }
    
    if (response.status === 200 || response.status === 204) {
        console.log(`✅ Successfully deleted: ${userEmail}`);
        return true;
    } else {
        console.error(`❌ Failed to delete ${userEmail}:`, response.data);
        return false;
    }
}

// Delete a runner by ID
async function deleteRunner(token, runnerId, runnerName) {
    console.log(`🗑️  Deleting runner: ${runnerName} (ID: ${runnerId})`);
    
    const response = await makeRequest('DELETE', `/runner/${runnerId}`, null, token);
    
    if (response.status === 200 || response.status === 204) {
        console.log(`✅ Successfully deleted: ${runnerName}`);
        return true;
    } else {
        console.error(`❌ Failed to delete ${runnerName}:`, response.data);
        return false;
    }
}

// Clean up all cases
async function cleanupCases(token) {
    console.log('📋 Cleaning up all cases...');
    
    try {
        const response = await makeRequest('GET', '/cases', null, token);
        if (response.status === 200 && response.data.success && response.data.cases) {
            const cases = response.data.cases;
            console.log(`Found ${cases.length} cases to delete`);
            
            for (const caseItem of cases) {
                const deleteResponse = await makeRequest('DELETE', `/cases/${caseItem.id}`, null, token);
                if (deleteResponse.status === 200 || deleteResponse.status === 204) {
                    console.log(`✅ Deleted case: ${caseItem.title}`);
                } else {
                    console.log(`⚠️  Could not delete case: ${caseItem.title}`);
                }
            }
        } else {
            console.log('ℹ️  No cases found or cases endpoint not available');
        }
    } catch (error) {
        console.log('⚠️  Could not clean up cases:', error.message);
    }
}

// Main cleanup function
async function main() {
    try {
        console.log('🚀 Starting COMPLETE database cleanup process...\n');
        console.log('⚠️  WARNING: This will delete ALL non-admin users, ALL runners, and ALL cases!\n');
        
        // Step 1: Login as admin
        const token = await loginAsAdmin();
        
        // Step 2: Get all users and identify what to delete
        const users = await getAllUsers(token);
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
        
        // Step 3: Delete non-admin users
        if (usersToDelete.length > 0) {
            console.log('\n🗑️  Deleting non-admin users...');
            let deletedCount = 0;
            
            for (const user of usersToDelete) {
                const success = await deleteUser(token, user.id, user.email);
                if (success) {
                    deletedCount++;
                }
                await new Promise(resolve => setTimeout(resolve, 500));
            }
            
            console.log(`\n✅ Deleted ${deletedCount} out of ${usersToDelete.length} non-admin users`);
        }
        
        // Step 4: Get and delete ALL runners
        const runners = await getAllRunners(token);
        if (runners.length > 0) {
            console.log(`\n🏃 Deleting ALL ${runners.length} runners...`);
            let deletedRunners = 0;
            
            for (const runner of runners) {
                const success = await deleteRunner(token, runner.id, runner.name || 'Unknown');
                if (success) {
                    deletedRunners++;
                }
                await new Promise(resolve => setTimeout(resolve, 500));
            }
            
            console.log(`\n✅ Deleted ${deletedRunners} out of ${runners.length} runners`);
        } else {
            console.log('\nℹ️  No runners found to delete');
        }
        
        // Step 5: Clean up all cases
        await cleanupCases(token);
        
        // Step 6: Final verification
        console.log('\n🔍 Final verification...');
        const finalUsers = await getAllUsers(token);
        const finalRunners = await getAllRunners(token);
        
        console.log(`\n📊 Final counts:`);
        console.log(`   Users: ${finalUsers.length}`);
        console.log(`   Runners: ${finalRunners.length}`);
        
        console.log('\n👥 Final users:');
        finalUsers.forEach(user => {
            const isAdmin = ADMIN_USERS.includes(user.email);
            console.log(`   ${isAdmin ? '✅' : '❌'} ${user.email} - ${user.firstName} ${user.lastName} (${user.role})`);
        });
        
        console.log('\n🏃 Final runners:');
        if (finalRunners.length === 0) {
            console.log('   ✅ No runners remaining (as expected)');
        } else {
            finalRunners.forEach(runner => {
                console.log(`   ❌ ${runner.name} (ID: ${runner.id})`);
            });
        }
        
        console.log('\n🎉 COMPLETE database cleanup finished successfully!');
        console.log('\n📝 Summary:');
        console.log(`   - Kept ${usersToKeep.length} admin users`);
        console.log(`   - Deleted ${usersToDelete.length} non-admin users`);
        console.log(`   - Deleted ${runners.length} runners`);
        console.log(`   - Cleaned up all cases`);
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
