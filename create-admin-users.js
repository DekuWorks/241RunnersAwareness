#!/usr/bin/env node

/**
 * Script to create the 6 admin users for 241 Runners Awareness
 * Based on the commit history, these are the admin users that should exist
 */

const https = require('https');

// Configuration
const API_BASE_URL = 'https://241runners-api-v2.azurewebsites.net/api/v1';

// The 6 admin users from the commit history
const adminUsers = [
    {
        email: 'dekuworks1@gmail.com',
        password: 'marcus2025',
        firstName: 'Marcus',
        lastName: 'Brown'
    },
    {
        email: 'lthomas3350@gmail.com',
        password: 'Lisa2025!',
        firstName: 'Lisa',
        lastName: 'Thomas'
    },
    {
        email: 'markmelasky@gmail.com',
        password: 'Mark2025!',
        firstName: 'Mark',
        lastName: 'Melasky'
    },
    {
        email: 'danielcarey9770@yahoo.com',
        password: 'Daniel2025!',
        firstName: 'Daniel',
        lastName: 'Carey'
    },
    {
        email: 'tinaleggins@yahoo.com',
        password: 'Tina2025!',
        firstName: 'Tina',
        lastName: 'Matthews'
    },
    {
        email: 'ralphfrank900@gmail.com',
        password: 'Ralph2025!',
        firstName: 'Ralph',
        lastName: 'Frank'
    }
];

// Helper function to make API requests
function makeRequest(method, endpoint, data = null) {
    return new Promise((resolve, reject) => {
        const url = new URL(`${API_BASE_URL}${endpoint}`);
        
        const options = {
            hostname: url.hostname,
            port: 443,
            path: url.pathname + url.search,
            method: method,
            headers: {
                'Content-Type': 'application/json'
            }
        };

        if (data) {
            const jsonData = JSON.stringify(data);
            options.headers['Content-Length'] = Buffer.byteLength(jsonData);
        }

        const req = https.request(options, (res) => {
            let responseData = '';
            res.on('data', (chunk) => responseData += chunk);
            res.on('end', () => {
                try {
                    const parsedData = JSON.parse(responseData);
                    resolve({ status: res.statusCode, data: parsedData });
                } catch (e) {
                    resolve({ status: res.statusCode, data: responseData });
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

// Create admin user
async function createAdminUser(userData) {
    console.log(`🔄 Creating admin user: ${userData.firstName} ${userData.lastName} (${userData.email})...`);
    
    try {
        const response = await makeRequest('POST', '/auth/create-admin', userData);
        
        if (response.status === 200 && response.data.success) {
            console.log(`✅ Successfully created admin user: ${userData.email}`);
            return true;
        } else if (response.status === 400 && response.data.error && response.data.error.code === 'USER_EXISTS') {
            console.log(`⚠️  Admin user already exists: ${userData.email}`);
            return true;
        } else {
            console.error(`❌ Failed to create admin user: ${userData.email}`);
            console.error(`   Status: ${response.status}`);
            console.error(`   Error: ${JSON.stringify(response.data)}`);
            return false;
        }
    } catch (error) {
        console.error(`❌ Error creating admin user ${userData.email}:`, error.message);
        return false;
    }
}

// Test login for an admin user
async function testLogin(email, password) {
    console.log(`🔐 Testing login for: ${email}...`);
    
    try {
        const response = await makeRequest('POST', '/auth/login', { email, password });
        
        if (response.status === 200 && response.data.success) {
            console.log(`✅ Login successful for: ${email}`);
            return true;
        } else {
            console.error(`❌ Login failed for: ${email}`);
            console.error(`   Status: ${response.status}`);
            console.error(`   Error: ${JSON.stringify(response.data)}`);
            return false;
        }
    } catch (error) {
        console.error(`❌ Error testing login for ${email}:`, error.message);
        return false;
    }
}

// Main function
async function main() {
    console.log('🚀 Creating 6 admin users for 241 Runners Awareness...\n');
    
    let successCount = 0;
    let totalCount = adminUsers.length;
    
    // Create all admin users
    for (const user of adminUsers) {
        const success = await createAdminUser(user);
        if (success) successCount++;
        
        // Add a small delay between requests
        await new Promise(resolve => setTimeout(resolve, 1000));
    }
    
    console.log(`\n📊 Results: ${successCount}/${totalCount} admin users created successfully`);
    
    if (successCount > 0) {
        console.log('\n🔐 Testing admin login functionality...');
        
        // Test login with Marcus (first admin)
        const testUser = adminUsers[0];
        const loginSuccess = await testLogin(testUser.email, testUser.password);
        
        if (loginSuccess) {
            console.log('\n🎉 Admin login is working! You can now access the admin dashboard.');
            console.log('\n📝 Admin Credentials:');
            adminUsers.forEach(user => {
                console.log(`   ${user.firstName} ${user.lastName}: ${user.email} / ${user.password}`);
            });
            console.log('\n🔗 Admin Dashboard: https://241runnersawareness.org/admin/login.html');
        } else {
            console.log('\n⚠️  Admin users were created but login is not working. Check database connectivity.');
        }
    } else {
        console.log('\n❌ No admin users were created. Check the API and database status.');
    }
}

// Run the script
if (require.main === module) {
    main().catch(console.error);
}

module.exports = { main, adminUsers };
