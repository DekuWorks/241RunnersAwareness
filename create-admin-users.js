// 241 Runners Awareness - Admin User Setup Script
// This script creates admin users in the backend database using the new API endpoints

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

// Helper function to make HTTP requests
function makeRequest(url, method, data) {
    return new Promise((resolve, reject) => {
        const urlObj = new URL(url);
        const postData = data ? JSON.stringify(data) : null;
        
        const options = {
            hostname: urlObj.hostname,
            port: 443,
            path: urlObj.pathname,
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'Content-Length': postData ? Buffer.byteLength(postData) : 0
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

        req.on('error', (err) => {
            reject(err);
        });

        if (postData) {
            req.write(postData);
        }
        req.end();
    });
}

// Function to create admin user
async function createAdminUser(userData) {
    try {
        console.log(`Creating admin user: ${userData.email}`);
        
        const response = await makeRequest(
            `${BACKEND_URL}/api/admin/create-admin`,
            'POST',
            userData
        );

        if (response.status === 200) {
            console.log(`✅ Successfully created admin user: ${userData.email}`);
            return true;
        } else {
            console.log(`❌ Failed to create admin user ${userData.email}: ${response.data.message || 'Unknown error'}`);
            return false;
        }
    } catch (error) {
        console.log(`❌ Error creating admin user ${userData.email}: ${error.message}`);
        return false;
    }
}

// Function to test admin login
async function testAdminLogin(email, password) {
    try {
        console.log(`Testing login for: ${email}`);
        
        const response = await makeRequest(
            `${BACKEND_URL}/api/auth/login`,
            'POST',
            { email, password }
        );

        if (response.status === 200 && (response.data.user.role === 'admin' || response.data.user.role === 'superadmin')) {
            console.log(`✅ Login successful for: ${email}`);
            return true;
        } else {
            console.log(`❌ Login failed for ${email}: User is not an admin or invalid credentials`);
            return false;
        }
    } catch (error) {
        console.log(`❌ Login failed for ${email}: ${error.message}`);
        return false;
    }
}

// Function to get existing admin users
async function getExistingAdminUsers() {
    try {
        console.log('Checking existing admin users...');
        
        const response = await makeRequest(
            `${BACKEND_URL}/api/admin/admins`,
            'GET'
        );

        if (response.status === 200) {
            console.log(`Found ${response.data.length} existing admin users`);
            return response.data;
        } else {
            console.log('Failed to get existing admin users');
            return [];
        }
    } catch (error) {
        console.log(`Failed to get existing admin users: ${error.message}`);
        return [];
    }
}

// Main execution
async function main() {
    console.log('🔐 Setting up Admin Users for 241 Runners Awareness');
    console.log('==================================================');
    console.log('');

    // Check existing admin users
    const existingUsers = await getExistingAdminUsers();
    const existingEmails = existingUsers.map(user => user.email);

    let successCount = 0;
    const totalUsers = adminUsers.length;

    // Create admin users
    console.log('🚀 Starting admin user setup...');
    for (const user of adminUsers) {
        if (existingEmails.includes(user.email)) {
            console.log(`⚠️  Admin user already exists: ${user.email}`);
            successCount++;
        } else {
            if (await createAdminUser(user)) {
                successCount++;
            }
        }
        console.log('');
    }

    console.log('==================================================');
    console.log('📊 Setup Summary:');
    console.log(`Total users: ${totalUsers}`);
    console.log(`Successfully created/verified: ${successCount}`);
    console.log(`Failed: ${totalUsers - successCount}`);
    console.log('');

    // Test logins
    console.log('🔍 Testing admin logins...');
    let loginSuccessCount = 0;

    for (const user of adminUsers) {
        if (await testAdminLogin(user.email, user.password)) {
            loginSuccessCount++;
        }
        console.log('');
    }

    console.log('==================================================');
    console.log('📊 Login Test Summary:');
    console.log(`Total logins tested: ${totalUsers}`);
    console.log(`Successful logins: ${loginSuccessCount}`);
    console.log(`Failed logins: ${totalUsers - loginSuccessCount}`);
    console.log('');

    // Display admin credentials
    console.log('🔑 Admin User Credentials:');
    console.log('==================================================');
    for (const user of adminUsers) {
        console.log(`Email: ${user.email}`);
        console.log(`Password: ${user.password}`);
        console.log(`Role: ${user.role}`);
        console.log('---');
    }
    console.log('');

    console.log('✅ Admin user setup completed!');
    console.log('🌐 You can now log in at: https://241runnersawareness.org/admin-login.html');
}

// Run the script
main().catch(console.error);
