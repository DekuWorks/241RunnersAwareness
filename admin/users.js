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
        
        // Show mock data for development
        showMockUsers();
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

/**
 * Show mock users data for development
 */
function showMockUsers() {
    const mockUsers = [
        {
            id: '1',
            firstName: 'John',
            lastName: 'Doe',
            email: 'john.doe@example.com',
            role: 'user',
            status: 'Active',
            createdAt: '2025-01-15T10:30:00Z',
            lastLoginAt: '2025-01-20T14:22:00Z',
            phoneNumber: '+1234567890'
        },
        {
            id: '2',
            firstName: 'Jane',
            lastName: 'Smith',
            email: 'jane.smith@example.com',
            role: 'parent',
            status: 'Active',
            createdAt: '2025-01-10T09:15:00Z',
            lastLoginAt: '2025-01-19T16:45:00Z',
            phoneNumber: '+1234567891'
        },
        {
            id: '3',
            firstName: 'Admin',
            lastName: 'User',
            email: 'admin@241runnersawareness.org',
            role: 'admin',
            status: 'Active',
            createdAt: '2025-01-01T00:00:00Z',
            lastLoginAt: '2025-01-20T18:30:00Z',
            phoneNumber: '+1234567892'
        },
        {
            id: '4',
            firstName: 'Bob',
            lastName: 'Johnson',
            email: 'bob.johnson@example.com',
            role: 'caregiver',
            status: 'Disabled',
            createdAt: '2025-01-12T11:20:00Z',
            lastLoginAt: '2025-01-18T13:10:00Z',
            phoneNumber: '+1234567893'
        }
    ];
    
    currentUsers = mockUsers;
    renderUsersTable();
    updatePagination(mockUsers.length);
}

// Make functions global for onclick handlers
window.editUser = editUser;
window.disableUser = disableUser;
window.enableUser = enableUser;
window.resetUser2FA = resetUser2FA;
window.changePage = changePage; 