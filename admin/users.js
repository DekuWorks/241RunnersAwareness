/**
 * ============================================
 * 241 RUNNERS AWARENESS - ADMIN USERS MANAGEMENT
 * ============================================
 * 
 * Users CRUD operations for admin dashboard
 */

import { fetchWithAuth, showToast } from './admin-auth.js';
import { renderTable, createSearchInput, createFilterDropdown, showModal, showConfirm, validateForm } from './admin-ui.js';

// State management
let currentUsers = [];
let currentPage = 1;
let pageSize = 10;
let currentSearch = '';
let currentFilter = '';

// Column definitions for users table
const userColumns = [
    {
        key: 'fullName',
        title: 'Name',
        render: (value, user) => `${user.firstName} ${user.lastName}`
    },
    {
        key: 'email',
        title: 'Email'
    },
    {
        key: 'role',
        title: 'Role',
        render: (value) => `<span class="status-badge ${value.toLowerCase()}">${value}</span>`
    },
    {
        key: 'status',
        title: 'Status',
        render: (value) => `<span class="status-badge ${value.toLowerCase()}">${value}</span>`
    },
    {
        key: 'createdAt',
        title: 'Created',
        render: (value) => new Date(value).toLocaleDateString()
    },
    {
        key: 'lastLoginAt',
        title: 'Last Login',
        render: (value) => value ? new Date(value).toLocaleDateString() : 'Never'
    }
];

// Action buttons for users table
const userActions = [
    {
        text: 'Edit',
        type: 'primary',
        onClick: 'editUser',
        visible: (user) => true
    },
    {
        text: 'Disable',
        type: 'warning',
        onClick: 'disableUser',
        visible: (user) => user.status === 'Active'
    },
    {
        text: 'Enable',
        type: 'success',
        onClick: 'enableUser',
        visible: (user) => user.status === 'Disabled'
    },
    {
        text: 'Reset 2FA',
        type: 'secondary',
        onClick: 'resetUser2FA',
        visible: (user) => true
    }
];

/**
 * Load users from API
 */
export async function loadUsers() {
    try {
        const params = new URLSearchParams({
            page: currentPage,
            pageSize: pageSize
        });
        
        if (currentSearch) {
            params.append('query', currentSearch);
        }
        
        if (currentFilter) {
            params.append('role', currentFilter);
        }
        
        const response = await fetchWithAuth(`/admin/users?${params}`);
        
        currentUsers = response.items || [];
        
        renderUsersTable();
        updatePagination(response.total || 0);
        
    } catch (error) {
        console.error('Error loading users:', error);
        showToast('Failed to load users', 'error');
        
        // Show users data
        function showUsers(users) {
            if (!users || users.length === 0) {
                document.getElementById('usersTableBody').innerHTML = `
                    <tr>
                        <td colspan="8" class="text-center text-muted py-4">
                            <div class="flex flex-col items-center space-y-2">
                                <svg class="w-12 h-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z"></path>
                                </svg>
                                <span class="text-lg font-medium">No users found</span>
                                <span class="text-sm">Users will appear here once they register</span>
                            </div>
                        </td>
                    </tr>
                `;
                return;
            }

            const tbody = document.getElementById('usersTableBody');
            tbody.innerHTML = '';

            users.forEach(user => {
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
                
                tbody.appendChild(row);
            });
        }
    }
}

/**
 * Render users table
 */
function renderUsersTable() {
    renderTable('usersTable', currentUsers, userColumns, {
        showPagination: false, // We handle pagination separately
        actions: userActions
    });
}

/**
 * Update pagination controls
 */
function updatePagination(total) {
    const totalPages = Math.ceil(total / pageSize);
    const paginationContainer = document.getElementById('pagination');
    
    if (!paginationContainer) return;
    
    paginationContainer.innerHTML = `
        <button class="btn btn-sm" onclick="changePage(${currentPage - 1})" ${currentPage <= 1 ? 'disabled' : ''}>
            Previous
        </button>
        <span class="page-info">Page ${currentPage} of ${totalPages} (${total} total users)</span>
        <button class="btn btn-sm" onclick="changePage(${currentPage + 1})" ${currentPage >= totalPages ? 'disabled' : ''}>
            Next
        </button>
    `;
}

/**
 * Change page
 */
export function changePage(page) {
    if (page < 1) return;
    currentPage = page;
    loadUsers();
}

/**
 * Search users
 */
export function searchUsers(query) {
    currentSearch = query;
    currentPage = 1; // Reset to first page
    loadUsers();
}

/**
 * Filter users by role
 */
export function filterUsers(role) {
    currentFilter = role;
    currentPage = 1; // Reset to first page
    loadUsers();
}

/**
 * Edit user
 */
export async function editUser(userIndex) {
    const user = currentUsers[userIndex];
    if (!user) return;
    
    showEditUserModal(user);
}

/**
 * Show edit user modal
 */
function showEditUserModal(user) {
    const modalContent = `
        <form id="editUserForm">
            <div class="form-grid">
                <div class="form-group">
                    <label for="editFirstName">First Name *</label>
                    <input type="text" id="editFirstName" name="firstName" value="${user.firstName || ''}" required>
                </div>
                <div class="form-group">
                    <label for="editLastName">Last Name *</label>
                    <input type="text" id="editLastName" name="lastName" value="${user.lastName || ''}" required>
                </div>
                <div class="form-group">
                    <label for="editEmail">Email *</label>
                    <input type="email" id="editEmail" name="email" value="${user.email || ''}" required>
                </div>
                <div class="form-group">
                    <label for="editRole">Role *</label>
                    <select id="editRole" name="role" required>
                        <option value="user" ${user.role === 'user' ? 'selected' : ''}>User</option>
                        <option value="parent" ${user.role === 'parent' ? 'selected' : ''}>Parent</option>
                        <option value="caregiver" ${user.role === 'caregiver' ? 'selected' : ''}>Caregiver</option>
                        <option value="aba_therapist" ${user.role === 'aba_therapist' ? 'selected' : ''}>ABA Therapist</option>
                        <option value="adoptive_parent" ${user.role === 'adoptive_parent' ? 'selected' : ''}>Adoptive Parent</option>
                        <option value="admin" ${user.role === 'admin' ? 'selected' : ''}>Admin</option>
                    </select>
                </div>
                <div class="form-group">
                    <label for="editStatus">Status *</label>
                    <select id="editStatus" name="status" required>
                        <option value="Active" ${user.status === 'Active' ? 'selected' : ''}>Active</option>
                        <option value="Disabled" ${user.status === 'Disabled' ? 'selected' : ''}>Disabled</option>
                    </select>
                </div>
                <div class="form-group">
                    <label for="editPhoneNumber">Phone Number</label>
                    <input type="tel" id="editPhoneNumber" name="phoneNumber" value="${user.phoneNumber || ''}">
                </div>
            </div>
        </form>
    `;
    
    const modal = showModal({
        title: 'Edit User',
        content: modalContent,
        buttons: [
            {
                text: 'Cancel',
                type: 'secondary',
                onClick: 'this.closest(\'.modal\').remove()'
            },
            {
                text: 'Save Changes',
                type: 'primary',
                onClick: 'saveUserChanges()'
            }
        ]
    });
    
    // Make save function global
    window.saveUserChanges = async () => {
        await saveUser(user.id);
    };
}

/**
 * Save user changes
 */
async function saveUser(userId) {
    try {
        const form = document.getElementById('editUserForm');
        const formData = new FormData(form);
        
        const userData = {
            firstName: formData.get('firstName'),
            lastName: formData.get('lastName'),
            email: formData.get('email'),
            role: formData.get('role'),
            status: formData.get('status'),
            phoneNumber: formData.get('phoneNumber')
        };
        
        // Validate form
        const validationRules = {
            firstName: { required: 'First name is required' },
            lastName: { required: 'Last name is required' },
            email: { 
                required: 'Email is required',
                email: 'Please enter a valid email address'
            },
            role: { required: 'Role is required' },
            status: { required: 'Status is required' }
        };
        
        const validation = validateForm(userData, validationRules);
        if (!validation.isValid) {
            showToast('Please fix the errors in the form', 'error');
            return;
        }
        
        const response = await fetchWithAuth(`/admin/users/${userId}`, {
            method: 'PATCH',
            body: JSON.stringify(userData)
        });
        
        showToast('User updated successfully', 'success');
        
        // Close modal and reload users
        document.querySelector('.modal').remove();
        loadUsers();
        
    } catch (error) {
        console.error('Error saving user:', error);
        showToast('Failed to update user: ' + error.message, 'error');
    }
}

/**
 * Disable user
 */
export async function disableUser(userIndex) {
    const user = currentUsers[userIndex];
    if (!user) return;
    
    const confirmed = await showConfirm(
        `Are you sure you want to disable ${user.firstName} ${user.lastName}?`,
        'Disable User'
    );
    
    if (confirmed) {
        try {
            await fetchWithAuth(`/admin/users/${user.id}/disable`, {
                method: 'PATCH'
            });
            
            showToast('User disabled successfully', 'success');
            loadUsers();
            
        } catch (error) {
            console.error('Error disabling user:', error);
            showToast('Failed to disable user: ' + error.message, 'error');
        }
    }
}

/**
 * Enable user
 */
export async function enableUser(userIndex) {
    const user = currentUsers[userIndex];
    if (!user) return;
    
    const confirmed = await showConfirm(
        `Are you sure you want to enable ${user.firstName} ${user.lastName}?`,
        'Enable User'
    );
    
    if (confirmed) {
        try {
            await fetchWithAuth(`/admin/users/${user.id}/enable`, {
                method: 'PATCH'
            });
            
            showToast('User enabled successfully', 'success');
            loadUsers();
            
        } catch (error) {
            console.error('Error enabling user:', error);
            showToast('Failed to enable user: ' + error.message, 'error');
        }
    }
}

/**
 * Reset user 2FA
 */
export async function resetUser2FA(userIndex) {
    const user = currentUsers[userIndex];
    if (!user) return;
    
    const confirmed = await showConfirm(
        `Are you sure you want to reset 2FA for ${user.firstName} ${user.lastName}? They will need to set up 2FA again.`,
        'Reset 2FA'
    );
    
    if (confirmed) {
        try {
            await fetchWithAuth(`/admin/users/${user.id}/reset-2fa`, {
                method: 'POST'
            });
            
            showToast('2FA reset successfully', 'success');
            
        } catch (error) {
            console.error('Error resetting 2FA:', error);
            showToast('Failed to reset 2FA: ' + error.message, 'error');
        }
    }
}

/**
 * Initialize users page
 */
export function initUsersPage() {
    // Create search input
    createSearchInput('searchContainer', searchUsers, {
        placeholder: 'Search users by name or email...',
        debounceMs: 300
    });
    
    // Create role filter
    const roleOptions = [
        { value: '', label: 'All Roles' },
        { value: 'user', label: 'User' },
        { value: 'parent', label: 'Parent' },
        { value: 'caregiver', label: 'Caregiver' },
        { value: 'aba_therapist', label: 'ABA Therapist' },
        { value: 'adoptive_parent', label: 'Adoptive Parent' },
        { value: 'admin', label: 'Admin' }
    ];
    
    createFilterDropdown('filterContainer', roleOptions, filterUsers, {
        placeholder: 'Filter by role...'
    });
    
    // Load initial data
    loadUsers();
}



// Make functions global for onclick handlers
window.editUser = editUser;
window.disableUser = disableUser;
window.enableUser = enableUser;
window.resetUser2FA = resetUser2FA;
window.changePage = changePage; 