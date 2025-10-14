/**
 * Role Switcher for Dual-Role Users
 * 
 * This component allows admin users who also have user/parent roles
 * to switch between admin and user interfaces seamlessly.
 */

class RoleSwitcher {
    constructor() {
        this.currentRole = 'admin';
        this.userData = null;
        this.init();
    }

    init() {
        this.loadUserData();
        this.createRoleSwitcher();
        this.setupEventListeners();
    }

    loadUserData() {
        // Try to get user data from localStorage
        const userData = localStorage.getItem('user');
        if (userData) {
            this.userData = JSON.parse(userData);
        }

        // If not found, try to get from admin storage
        if (!this.userData) {
            const adminUser = localStorage.getItem('ra_admin_user');
            if (adminUser) {
                this.userData = JSON.parse(adminUser);
            }
        }
    }

    createRoleSwitcher() {
        if (!this.userData || !this.hasMultipleRoles()) {
            return; // No need for role switcher if user only has one role
        }

        const navbar = document.querySelector('.navbar-nav');
        if (!navbar) return;

        // Create role switcher dropdown
        const roleSwitcherHtml = `
            <li class="nav-item dropdown">
                <a class="nav-link dropdown-toggle" href="#" id="roleSwitcher" role="button" data-bs-toggle="dropdown" aria-expanded="false">
                    <i class="fas fa-user-cog me-2"></i>
                    <span id="currentRoleLabel">Admin Mode</span>
                </a>
                <ul class="dropdown-menu" aria-labelledby="roleSwitcher">
                    <li><h6 class="dropdown-header">Switch Role</h6></li>
                    <li><a class="dropdown-item" href="#" data-role="admin">
                        <i class="fas fa-shield-alt me-2"></i>Admin Dashboard
                    </a></li>
                    <li><a class="dropdown-item" href="#" data-role="user">
                        <i class="fas fa-user me-2"></i>User Dashboard
                    </a></li>
                    <li><a class="dropdown-item" href="#" data-role="parent">
                        <i class="fas fa-users me-2"></i>Parent Dashboard
                    </a></li>
                    <li><hr class="dropdown-divider"></li>
                    <li><h6 class="dropdown-header">Quick Actions</h6></li>
                    <li><a class="dropdown-item" href="/profile.html">
                        <i class="fas fa-user-edit me-2"></i>My Profile
                    </a></li>
                    <li><a class="dropdown-item" href="/my-cases.html">
                        <i class="fas fa-folder-open me-2"></i>My Cases
                    </a></li>
                    <li><a class="dropdown-item" href="/runners.html">
                        <i class="fas fa-running me-2"></i>My Runners
                    </a></li>
                </ul>
            </li>
        `;

        navbar.insertAdjacentHTML('beforeend', roleSwitcherHtml);
        this.updateRoleDisplay();
    }

    setupEventListeners() {
        document.addEventListener('click', (e) => {
            if (e.target.closest('[data-role]')) {
                e.preventDefault();
                const role = e.target.closest('[data-role]').dataset.role;
                this.switchRole(role);
            }
        });
    }

    hasMultipleRoles() {
        if (!this.userData) return false;
        
        // Check if user has multiple roles
        const hasAdmin = this.userData.role === 'admin' || 
                        (this.userData.allRoles && this.userData.allRoles.includes('admin'));
        const hasOtherRoles = this.userData.allRoles && this.userData.allRoles.length > 1;
        
        return hasAdmin && hasOtherRoles;
    }

    switchRole(role) {
        if (role === 'admin') {
            this.currentRole = 'admin';
            this.updateRoleDisplay();
            // Stay on admin dashboard
            this.showNotification('Switched to Admin Mode', 'info');
        } else if (role === 'user') {
            this.currentRole = 'user';
            this.updateRoleDisplay();
            // Redirect to user dashboard
            window.location.href = '/index.html';
        } else if (role === 'parent') {
            this.currentRole = 'parent';
            this.updateRoleDisplay();
            // Redirect to parent dashboard or cases page
            window.location.href = '/my-cases.html';
        }
    }

    updateRoleDisplay() {
        const roleLabel = document.getElementById('currentRoleLabel');
        if (!roleLabel) return;

        switch (this.currentRole) {
            case 'admin':
                roleLabel.textContent = 'Admin Mode';
                break;
            case 'user':
                roleLabel.textContent = 'User Mode';
                break;
            case 'parent':
                roleLabel.textContent = 'Parent Mode';
                break;
            default:
                roleLabel.textContent = 'Admin Mode';
        }
    }

    showNotification(message, type = 'info') {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `alert alert-${type === 'info' ? 'info' : type} alert-dismissible fade show position-fixed`;
        notification.style.cssText = `
            top: 20px;
            right: 20px;
            z-index: 9999;
            min-width: 300px;
        `;
        
        notification.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;

        document.body.appendChild(notification);

        // Auto-remove after 3 seconds
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 3000);
    }

    // Public method to get current role
    getCurrentRole() {
        return this.currentRole;
    }

    // Public method to check if user has specific role
    hasRole(role) {
        if (!this.userData) return false;
        
        if (role === 'admin') {
            return this.userData.role === 'admin' || 
                   (this.userData.allRoles && this.userData.allRoles.includes('admin'));
        }
        
        return this.userData.allRoles && this.userData.allRoles.includes(role);
    }
}

// Initialize role switcher when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.roleSwitcher = new RoleSwitcher();
});

// Export for use in other scripts
window.RoleSwitcher = RoleSwitcher;
