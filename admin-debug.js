// Admin Dashboard Authentication Debug Script
// Run this in the browser console to check authentication status

console.log('🔍 ADMIN AUTHENTICATION DEBUG');
console.log('================================');

// Check localStorage keys
console.log('📋 All localStorage keys:');
Object.keys(localStorage).forEach(key => {
    console.log(`  - ${key}: ${localStorage.getItem(key) ? 'Present' : 'Missing'}`);
});

// Check specific admin tokens
console.log('\n🔑 Admin Authentication Tokens:');
console.log(`  - ra_admin_token: ${localStorage.getItem('ra_admin_token') ? 'Present' : 'Missing'}`);
console.log(`  - jwtToken: ${localStorage.getItem('jwtToken') ? 'Present' : 'Missing'}`);
console.log(`  - access_token: ${localStorage.getItem('access_token') ? 'Present' : 'Missing'}`);

// Check admin role
console.log(`  - ra_admin_role: ${localStorage.getItem('ra_admin_role') || 'Missing'}`);

// Check admin user data
const adminUser = localStorage.getItem('ra_admin_user');
console.log(`  - ra_admin_user: ${adminUser ? 'Present' : 'Missing'}`);
if (adminUser) {
    try {
        const userData = JSON.parse(adminUser);
        console.log(`    - Email: ${userData.email || 'Unknown'}`);
        console.log(`    - Role: ${userData.role || 'Unknown'}`);
        console.log(`    - Name: ${userData.name || 'Unknown'}`);
    } catch (e) {
        console.log('    - Error parsing user data:', e.message);
    }
}

// Test API connectivity
console.log('\n🌐 Testing API Connectivity:');
fetch('https://241runners-api-v2.azurewebsites.net/api/health')
    .then(response => {
        console.log(`  - Health endpoint: ${response.status} ${response.statusText}`);
        return response.json();
    })
    .then(data => {
        console.log('  - Health response:', data);
    })
    .catch(error => {
        console.log('  - Health endpoint error:', error.message);
    });

// Test admin endpoint with token
const token = localStorage.getItem('ra_admin_token') || localStorage.getItem('jwtToken') || localStorage.getItem('access_token');
if (token) {
    console.log('\n🔐 Testing Admin API with token:');
    fetch('https://241runners-api-v2.azurewebsites.net/api/v1.0/Admin/stats', {
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        console.log(`  - Admin stats endpoint: ${response.status} ${response.statusText}`);
        if (response.ok) {
            return response.json();
        } else {
            return response.text();
        }
    })
    .then(data => {
        console.log('  - Admin stats response:', data);
    })
    .catch(error => {
        console.log('  - Admin stats error:', error.message);
    });
} else {
    console.log('\n❌ No authentication token found - cannot test admin API');
}

console.log('\n✅ Debug script completed');
