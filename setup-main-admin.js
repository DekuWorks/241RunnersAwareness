// 241 Runners Awareness - Main Admin Setup Script
// This script creates the main admin user in the backend database

const https = require('https');

const BACKEND_URL = 'https://241runnersawareness-api.azurewebsites.net';

// Main admin user data
const mainAdmin = {
    username: "main_admin",
    email: "contact@241runnersawareness.org",
    firstName: "Main",
    lastName: "Admin",
    password: "runners241@",
    role: "superadmin",
    organization: "241 Runners Awareness",
    credentials: "System Administrator",
    specialization: "System Management",
    yearsOfExperience: "5+"
};

// Function to make HTTPS requests
function makeRequest(url, method, data) {
    return new Promise((resolve, reject) => {
        const postData = data ? JSON.stringify(data) : '';
        
        const options = {
            hostname: '241runnersawareness-api.azurewebsites.net',
            port: 443,
            path: url.replace('https://241runnersawareness-api.azurewebsites.net', ''),
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'Content-Length': Buffer.byteLength(postData)
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
                } catch (error) {
                    resolve({ status: res.statusCode, data: body });
                }
            });
        });

        req.on('error', (error) => {
            reject(error);
        });

        if (postData) {
            req.write(postData);
        }
        req.end();
    });
}

// Function to create main admin user
async function createMainAdmin() {
    try {
        console.log('🔐 Setting up Main Admin for 241 Runners Awareness');
        console.log('==================================================');
        console.log('');

        console.log('Creating main admin user...');
        const response = await makeRequest(`${BACKEND_URL}/api/admin/create-admin`, 'POST', mainAdmin);

        if (response.status === 200) {
            console.log('✅ Successfully created main admin user');
            console.log(`   Email: ${mainAdmin.email}`);
            console.log(`   Role: ${mainAdmin.role}`);
            console.log('');
        } else if (response.status === 400 && response.data.message && response.data.message.includes('already exists')) {
            console.log('ℹ️  Main admin user already exists');
            console.log('');
        } else {
            console.log('❌ Failed to create main admin user');
            console.log(`   Status: ${response.status}`);
            console.log(`   Error: ${response.data.message || 'Unknown error'}`);
            console.log('');
        }

        // Test login
        console.log('Testing main admin login...');
        const loginResponse = await makeRequest(`${BACKEND_URL}/api/auth/login`, 'POST', {
            email: mainAdmin.email,
            password: mainAdmin.password
        });

        if (loginResponse.status === 200) {
            console.log('✅ Main admin login successful');
            console.log(`   Token: ${loginResponse.data.token ? 'Received' : 'Not received'}`);
        } else {
            console.log('❌ Main admin login failed');
            console.log(`   Status: ${loginResponse.status}`);
            console.log(`   Error: ${loginResponse.data.message || 'Unknown error'}`);
        }

        console.log('');
        console.log('==================================================');
        console.log('📋 Main Admin Credentials:');
        console.log('==================================================');
        console.log(`Email: ${mainAdmin.email}`);
        console.log(`Password: ${mainAdmin.password}`);
        console.log(`Role: ${mainAdmin.role}`);
        console.log('');
        console.log('🌐 You can now log in at: https://241runnersawareness.org/admin-login.html');
        console.log('');

    } catch (error) {
        console.error('❌ Error setting up main admin:', error.message);
        console.log('');
        console.log('This might be because:');
        console.log('1. Backend is not deployed yet');
        console.log('2. Backend is not accessible');
        console.log('3. Network connectivity issues');
        console.log('');
        console.log('The main admin credentials are:');
        console.log(`Email: ${mainAdmin.email}`);
        console.log(`Password: ${mainAdmin.password}`);
    }
}

// Run the setup
createMainAdmin();
