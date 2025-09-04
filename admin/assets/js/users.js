/**
 * ============================================
 * 241 RUNNERS AWARENESS - ADMIN USERS MANAGEMENT
 * ============================================
 * 
 * This file provides user management functionality for admin users
 * including CRUD operations, search, and filtering.
 */

// API Configuration
let API_BASE_URL = 'https://241runners-api.azurewebsites.net/api';

// Load API configuration
async function loadConfig() {
    try {
        const response = await fetch('/config.json');
        const config = await response.json();
        API_BASE_URL = config.API_BASE_URL;
    } catch (error) {
        console.warn('Failed to load config.json, using default API URL');
    }
}

// Initialize config on load
loadConfig();

// Global variables
let currentUsers = [];
let currentPage = 1;
const pageSize = 20;

/**
 * Initialize the users page
 */
export function initUsersPage() {
    loadConfig();
    loadUsers();
    setupEventListeners();
}

/**
 * Setup event listeners
 */
function setupEventListeners() {
    // Search functionality
    const searchInput = document.getElementById('searchContainer');
    if (searchInput) {
        searchInput.innerHTML = `
            <input type="text" id="userSearchInput" 
                   placeholder="Search users by name, email, or role..." 
                   class="search-input" 
                   onkeyup="filterUsers()">
        `;
    }
}

/**
 * Load users from the API
 */
export async function loadUsers() {
    try {
        showLoading();
        
        const response = await fetch(`${API_BASE_URL}/admin/users`, {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('ra_admin_token')}`,
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const data = await response.json();
        currentUsers = data.users || [];
        
        renderUsersTable();
        hideLoading();
        
    } catch (error) {
        console.error('Error loading users:', error);
        showError('Failed to load users. Please try again.');
        hideLoading();
    }
}

/**
 * Filter users based on search input
 */
export function filterUsers() {
    const searchTerm = document.getElementById('userSearchInput')?.value.toLowerCase() || '';
    
    if (!searchTerm) {
        renderUsersTable();
        return;
    }

    const filteredUsers = currentUsers.filter(user => 
        user.firstName?.toLowerCase().includes(searchTerm) ||
        user.lastName?.toLowerCase().includes(searchTerm) ||
        user.email?.toLowerCase().includes(searchTerm) ||
        user.role?.toLowerCase().includes(searchTerm)
    );

    renderFilteredUsers(filteredUsers);
}

/**
 * Render filtered users
 */
function renderFilteredUsers(users) {
    const tableBody = document.getElementById('usersTableBody');
    if (!tableBody) return;

    tableBody.innerHTML = '';
    
    if (users.length === 0) {
        tableBody.innerHTML = `
            <tr>
                <td colspan="8" class="text-center py-4 text-gray-500">
                    No users found matching your search.
                </td>
            </tr>
        `;
        return;
    }

    users.forEach(user => {
        const row = createUserRow(user);
        tableBody.appendChild(row);
    });
}

/**
 * Create a user table row
 */
function createUserRow(user) {
    const row = document.createElement('tr');
    row.className = 'hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors';
    
    row.innerHTML = `
        <td class="px-6 py-4 whitespace-nowrap">
            <div class="flex items-center">
                <div class="flex-shrink-0 h-10 w-10">
                    <div class="h-10 w-10 rounded-full bg-gradient-to-r from-blue-500 to-purple-600 flex items-center justify-center text-white font-semibold text-sm">
                        ${user.firstName ? user.firstName.charAt(0).toUpperCase() : 'U'}
                    </div>
                </div>
                <div class="ml-4">
                    <div class="text-sm font-medium text-gray-900 dark:text-white">
                        ${user.firstName || 'N/A'} ${user.lastName || 'N/A'}
                    </div>
                    <div class="text-sm text-gray-500 dark:text-gray-400">
                        ${user.email || 'No email'}
                    </div>
                </div>
            </div>
        </td>
        <td class="px-6 py-4 whitespace-nowrap">
            <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getRoleBadgeClass(user.role)}">
                ${user.role || 'user'}
            </span>
        </td>
        <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
            ${user.phoneNumber || 'N/A'}
        </td>
        <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
            ${user.organization || 'N/A'}
        </td>
        <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
            ${user.title || 'N/A'}
        </td>
        <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
            ${formatDate(user.createdAt)}
        </td>
        <td class="px-6 py-4 whitespace-nowrap">
            <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${user.isActive ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200' : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'}">
                ${user.isActive ? 'Active' : 'Inactive'}
            </span>
        </td>
        <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
            <button onclick="editUser(${user.id})" class="text-indigo-600 hover:text-indigo-900 dark:text-indigo-400 dark:hover:text-indigo-300 mr-3">
                Edit
            </button>
            <button onclick="deleteUser(${user.id})" class="text-red-600 hover:text-red-900 dark:text-red-400 dark:hover:text-red-300">
                Delete
            </button>
        </td>
    `;
    
    return row;
}

/**
 * Render the users table
 */
function renderUsersTable() {
    const table = document.getElementById('usersTable');
    if (!table) return;

    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const usersToShow = currentUsers.slice(startIndex, endIndex);

    table.innerHTML = `
        <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
            <thead class="bg-gray-50 dark:bg-gray-800">
                <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                        User
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                        Role
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                        Phone
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                        Organization
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                        Title
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                        Created
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                        Status
                    </th>
                    <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                        Actions
                    </th>
                </tr>
            </thead>
            <tbody id="usersTableBody" class="bg-white dark:bg-gray-900 divide-y divide-gray-200 dark:divide-gray-700">
                ${usersToShow.map(user => createUserRow(user).outerHTML).join('')}
            </tbody>
        </table>
    `;

    updatePagination();
}

/**
 * Update pagination controls
 */
function updatePagination() {
    const paginationContainer = document.getElementById('pagination');
    if (!paginationContainer) return;

    const totalPages = Math.ceil(currentUsers.length / pageSize);
    
    paginationContainer.innerHTML = `
        <div class="flex items-center justify-between">
            <div class="flex items-center space-x-2">
                <button class="btn btn-sm" onclick="changePage(${currentPage - 1})" ${currentPage <= 1 ? 'disabled' : ''}>
                    Previous
                </button>
                <span class="text-sm text-gray-700 dark:text-gray-300">
                    Page ${currentPage} of ${totalPages} (${currentUsers.length} total users)
                </span>
                <button class="btn btn-sm" onclick="changePage(${currentPage + 1})" ${currentPage >= totalPages ? 'disabled' : ''}>
                    Next
                </button>
            </div>
        </div>
    `;
}

/**
 * Change page
 */
export function changePage(page) {
    const totalPages = Math.ceil(currentUsers.length / pageSize);
    
    if (page < 1 || page > totalPages) {
        return;
    }
    
    currentPage = page;
    renderUsersTable();
}

/**
 * Get role badge class
 */
function getRoleBadgeClass(role) {
    switch (role?.toLowerCase()) {
        case 'admin':
            return 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200';
        case 'moderator':
            return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200';
        case 'user':
        default:
            return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200';
    }
}

/**
 * Format date
 */
function formatDate(dateString) {
    if (!dateString) return 'N/A';
    
    try {
        const date = new Date(dateString);
        return date.toLocaleDateString();
    } catch (error) {
        return 'Invalid Date';
    }
}

/**
 * Show loading state
 */
function showLoading() {
    const table = document.getElementById('usersTable');
    if (table) {
        table.innerHTML = `
            <div class="flex items-center justify-center py-8">
                <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                <span class="ml-2 text-gray-600">Loading users...</span>
            </div>
        `;
    }
}

/**
 * Hide loading state
 */
function hideLoading() {
    // Loading will be hidden when table is rendered
}

/**
 * Show error message
 */
function showError(message) {
    const table = document.getElementById('usersTable');
    if (table) {
        table.innerHTML = `
            <div class="flex items-center justify-center py-8">
                <div class="text-red-600 text-center">
                    <i class="fas fa-exclamation-triangle text-2xl mb-2"></i>
                    <p>${message}</p>
                </div>
            </div>
        `;
    }
}

// Make functions global for HTML onclick handlers
window.filterUsers = filterUsers;
window.changePage = changePage; 