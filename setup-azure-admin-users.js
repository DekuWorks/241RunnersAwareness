// 241 Runners Awareness - Azure Admin User Setup Script
// This script creates admin users in the Azure backend database

const https = require('https');

const BACKEND_URL = 'https://241runnersawareness-api.azurewebsites.net';

// Admin users to create
const adminUsers = [
    {
        username: "marcus_brown",
        email: "dekuworks1@gmail.com",
        firstName: "Marcus",
        lastName: "Brown",
        password: "marcus2025",
        role: "admin",
        organization: "241 Runners Awareness",
        credentials: "Co-Founder",
        specialization: "Operations",
        yearsOfExperience: "3+"
    },
    {
        username: "daniel_carey",
        email: "danielcarey9770@gmail.com",
        firstName: "Daniel",
        lastName: "Carey",
        password: "daniel2025",
        role: "admin",
        organization: "241 Runners Awareness",
        credentials: "Co-Founder",
        specialization: "Technology",
        yearsOfExperience: "4+"
    },
    {
        username: "daniel_carey_yahoo",
        email: "danielcarey9770@yahoo.com",
        firstName: "Daniel",
        lastName: "Carey",
        password: "daniel2025",
        role: "admin",
        organization: "241 Runners Awareness",
        credentials: "Co-Founder",
        specialization: "Technology",
        yearsOfExperience: "4+"
    }
];

// Helper function to make HTTPS requests
function makeRequest(url, method = 'GET', data = null) {
    return new Promise((resolve, reject) => {
        const options = {
            hostname: new URL(url).hostname,
            port: 443,
            path: new URL(url).pathname,
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'User-Agent': '241RunnersAwareness-AdminSetup/1.0'
            }
        };

        const req = https.request(options, (res) => {
            let body = '';
            res.on('data', (chunk) => {
                body += chunk;
            });
            res.on('end', () => {
                try {
                    const response = JSON.parse(body);
                    resolve({ status: res.statusCode, data: response });
                } catch (e) {
                    resolve({ status: res.statusCode, data: body });
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

// Function to check if backend is ready
async function checkBackendHealth() {
    console.log('🔍 Checking backend health...');
    try {
        const response = await makeRequest(`${BACKEND_URL}/health`);
        if (response.status === 200) {
            console.log('✅ Backend is healthy and ready');
            return true;
        } else {
            console.log(`❌ Backend health check failed: ${response.status}`);
            return false;
        }
    } catch (error) {
        console.log(`❌ Backend health check error: ${error.message}`);
        return false;
    }
}

// Function to check existing admin users
async function getExistingAdminUsers() {
    console.log('🔍 Checking existing admin users...');
    try {
        const response = await makeRequest(`${BACKEND_URL}/api/admin/admins`);
        if (response.status === 200) {
            console.log(`✅ Found ${response.data.length} existing admin users`);
            return response.data.map(user => user.email);
        } else {
            console.log(`❌ Failed to get admin users: ${response.status}`);
            return [];
        }
    } catch (error) {
        console.log(`❌ Error getting admin users: ${error.message}`);
        return [];
    }
}

// Function to create admin user
async function createAdminUser(userData) {
    console.log(`👤 Creating admin user: ${userData.email}`);
    try {
        const response = await makeRequest(`${BACKEND_URL}/api/admin/create-admin`, 'POST', userData);
        if (response.status === 200) {
            console.log(`✅ Successfully created admin user: ${userData.email}`);
            return true;
        } else {
            console.log(`❌ Failed to create admin user ${userData.email}: ${response.status} - ${response.data.message || 'Unknown error'}`);
            return false;
        }
    } catch (error) {
        console.log(`❌ Error creating admin user ${userData.email}: ${error.message}`);
        return false;
    }
}

// Function to test admin login
async function testAdminLogin(email, password) {
    console.log(`🔐 Testing login for: ${email}`);
    try {
        const response = await makeRequest(`${BACKEND_URL}/api/auth/login`, 'POST', { email, password });
        if (response.status === 200 && response.data.success) {
            console.log(`✅ Login successful for: ${email}`);
            return true;
        } else {
            console.log(`❌ Login failed for ${email}: ${response.data.message || 'Unknown error'}`);
            return false;
        }
    } catch (error) {
        console.log(`❌ Error testing login for ${email}: ${error.message}`);
        return false;
    }
}

// Main execution
async function main() {
    console.log('🚀 241 Runners Awareness - Azure Admin Setup');
    console.log('=============================================');
    console.log(`Backend URL: ${BACKEND_URL}`);
    console.log('');

    // Check if backend is ready
    const isHealthy = await checkBackendHealth();
    if (!isHealthy) {
        console.log('❌ Backend is not ready. Please ensure the backend is deployed and running.');
        console.log('💡 You may need to wait a few minutes after deployment for the backend to be fully ready.');
        return;
    }

    // Get existing admin users
    const existingEmails = await getExistingAdminUsers();
    console.log('');

    // Create admin users
    let successCount = 0;
    let totalUsers = adminUsers.length;

    for (const userData of adminUsers) {
        if (existingEmails.includes(userData.email)) {
            console.log(`⏭️  Admin user ${userData.email} already exists, skipping...`);
            successCount++;
        } else {
            const success = await createAdminUser(userData);
            if (success) {
                successCount++;
            }
        }
        console.log('');
    }

    // Test logins
    console.log('🔐 Testing admin logins...');
    console.log('');
    
    for (const userData of adminUsers) {
        await testAdminLogin(userData.email, userData.password);
        console.log('');
    }

    // Summary
    console.log('📊 Setup Summary:');
    console.log('=================');
    console.log(`Total admin users: ${totalUsers}`);
    console.log(`Successfully processed: ${successCount}`);
    console.log(`Failed: ${totalUsers - successCount}`);
    console.log('');

    if (successCount === totalUsers) {
        console.log('🎉 All admin users are ready!');
        console.log('');
        console.log('🔑 Admin Login Credentials:');
        console.log('==========================');
        adminUsers.forEach(user => {
            console.log(`Email: ${user.email}`);
            console.log(`Password: ${user.password}`);
            console.log(`Role: ${user.role}`);
            console.log('---');
        });
        console.log('');
        console.log('🌐 Admin Login URL: https://241runnersawareness.org/admin-login.html');
    } else {
        console.log('⚠️  Some admin users failed to be created. Please check the errors above.');
    }
}

// Run the script
main().catch(console.error);
