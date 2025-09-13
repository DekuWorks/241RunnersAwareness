/**
 * ============================================
 * 241 RUNNERS AWARENESS - ADMIN UI HELPERS
 * ============================================
 * 
 * UI utilities for admin dashboard components
 */

/**
 * Show toast notification
 * @param {string} message - Message to display
 * @param {string} type - Toast type (success, error, warning, info)
 * @param {number} duration - Duration in milliseconds
 */
export function showToast(message, type = 'info', duration = 5000) {
    // Remove existing toasts
    const existingToasts = document.querySelectorAll('.toast');
    existingToasts.forEach(toast => toast.remove());
    
    // Create toast container if it doesn't exist
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'toast-container';
        document.body.appendChild(toastContainer);
    }
    
    // Create toast element
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `
        <div class="toast-content">
            <span class="toast-message">${escapeHtml(message)}</span>
            <button class="toast-close" onclick="this.parentElement.parentElement.remove()">&times;</button>
        </div>
    `;
    
    // Add to container
    toastContainer.appendChild(toast);
    
    // Show toast
    setTimeout(() => toast.classList.add('show'), 100);
    
    // Auto remove
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => {
            if (toast.parentElement) {
                toast.remove();
            }
        }, 300);
    }, duration);
}

/**
 * Create and show modal
 * @param {Object} options - Modal options
 * @param {string} options.title - Modal title
 * @param {string} options.content - Modal content (HTML)
 * @param {Array} options.buttons - Array of button objects
 * @param {Function} options.onClose - Callback when modal closes
 * @returns {HTMLElement} Modal element
 */
export function showModal({ title, content, buttons = [], onClose = null }) {
    // Remove existing modals
    const existingModals = document.querySelectorAll('.modal');
    existingModals.forEach(modal => modal.remove());
    
    // Create modal
    const modal = document.createElement('div');
    modal.className = 'modal';
    modal.innerHTML = `
        <div class="modal-backdrop"></div>
        <div class="modal-content">
            <div class="modal-header">
                <h2>${escapeHtml(title)}</h2>
                <button class="modal-close" onclick="this.closest('.modal').remove()">&times;</button>
            </div>
            <div class="modal-body">
                ${content}
            </div>
            ${buttons.length > 0 ? `
                <div class="modal-footer">
                    ${buttons.map(btn => `
                        <button class="btn btn-${btn.type || 'secondary'}" 
                                onclick="${btn.onClick || 'this.closest(\'.modal\').remove()'}">
                            ${escapeHtml(btn.text)}
                        </button>
                    `).join('')}
                </div>
            ` : ''}
        </div>
    `;
    
    // Add to page
    document.body.appendChild(modal);
    
    // Show modal
    setTimeout(() => modal.classList.add('show'), 100);
    
    // Handle backdrop click
    const backdrop = modal.querySelector('.modal-backdrop');
    backdrop.addEventListener('click', () => {
        modal.classList.remove('show');
        setTimeout(() => {
            modal.remove();
            if (onClose) onClose();
        }, 300);
    });
    
    return modal;
}

/**
 * Show confirmation dialog
 * @param {string} message - Confirmation message
 * @param {string} title - Dialog title
 * @returns {Promise<boolean>} True if confirmed
 */
export function showConfirm(message, title = 'Confirm Action') {
    return new Promise((resolve) => {
        showModal({
            title,
            content: `<p>${escapeHtml(message)}</p>`,
            buttons: [
                {
                    text: 'Cancel',
                    type: 'secondary',
                    onClick: 'this.closest(\'.modal\').remove()'
                },
                {
                    text: 'Confirm',
                    type: 'danger',
                    onClick: 'this.closest(\'.modal\').remove()'
                }
            ],
            onClose: () => resolve(false)
        });
        
        // Handle confirm button
        const confirmBtn = document.querySelector('.modal .btn-danger');
        if (confirmBtn) {
            confirmBtn.addEventListener('click', () => resolve(true));
        }
    });
}

/**
 * Render data table
 * @param {string} containerId - Container element ID
 * @param {Array} data - Table data
 * @param {Array} columns - Column definitions
 * @param {Object} options - Table options
 */
export function renderTable(containerId, data, columns, options = {}) {
    const container = document.getElementById(containerId);
    if (!container) return;
    
    const {
        emptyMessage = 'No data available',
        loadingMessage = 'Loading...',
        showPagination = true,
        pageSize = 10,
        currentPage = 1,
        onRowClick = null,
        actions = []
    } = options;
    
    // Show loading state
    if (data === null) {
        container.innerHTML = `<div class="loading">${loadingMessage}</div>`;
        return;
    }
    
    // Handle empty data
    if (!data || data.length === 0) {
        container.innerHTML = `<div class="empty-state">${emptyMessage}</div>`;
        return;
    }
    
    // Calculate pagination
    const totalPages = Math.ceil(data.length / pageSize);
    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const pageData = data.slice(startIndex, endIndex);
    
    // Build table HTML
    let tableHtml = `
        <table class="data-table">
            <thead>
                <tr>
                    ${columns.map(col => `<th>${escapeHtml(col.title)}</th>`).join('')}
                    ${actions.length > 0 ? '<th>Actions</th>' : ''}
                </tr>
            </thead>
            <tbody>
    `;
    
    // Add rows
    pageData.forEach((row, index) => {
        const rowIndex = startIndex + index;
        tableHtml += `<tr data-index="${rowIndex}"${onRowClick ? ' class="clickable"' : ''}>`;
        
        columns.forEach(col => {
            const value = col.render ? col.render(row[col.key], row) : row[col.key];
            tableHtml += `<td>${escapeHtml(value || '')}</td>`;
        });
        
        // Add action buttons
        if (actions.length > 0) {
            tableHtml += '<td class="actions">';
            actions.forEach(action => {
                const isVisible = action.visible ? action.visible(row) : true;
                if (isVisible) {
                    tableHtml += `
                        <button class="btn btn-sm btn-${action.type || 'secondary'}" 
                                onclick="${action.onClick}(${rowIndex})" 
                                title="${escapeHtml(action.title || '')}">
                            ${action.icon || action.text}
                        </button>
                    `;
                }
            });
            tableHtml += '</td>';
        }
        
        tableHtml += '</tr>';
    });
    
    tableHtml += `
            </tbody>
        </table>
    `;
    
    // Add pagination
    if (showPagination && totalPages > 1) {
        tableHtml += `
            <div class="pagination">
                <button class="btn btn-sm" onclick="changePage(${currentPage - 1})" ${currentPage <= 1 ? 'disabled' : ''}>
                    Previous
                </button>
                <span class="page-info">Page ${currentPage} of ${totalPages}</span>
                <button class="btn btn-sm" onclick="changePage(${currentPage + 1})" ${currentPage >= totalPages ? 'disabled' : ''}>
                    Next
                </button>
            </div>
        `;
    }
    
    container.innerHTML = tableHtml;
    
    // Add row click handlers
    if (onRowClick) {
        const rows = container.querySelectorAll('tr.clickable');
        rows.forEach(row => {
            row.addEventListener('click', () => {
                const index = parseInt(row.dataset.index);
                onRowClick(data[index], index);
            });
        });
    }
}

/**
 * Create search input with debouncing
 * @param {string} containerId - Container element ID
 * @param {Function} onSearch - Search callback function
 * @param {Object} options - Search options
 */
export function createSearchInput(containerId, onSearch, options = {}) {
    const container = document.getElementById(containerId);
    if (!container) return;
    
    const {
        placeholder = 'Search...',
        debounceMs = 300,
        minLength = 2
    } = options;
    
    let debounceTimer;
    
    container.innerHTML = `
        <div class="search-input">
            <input type="text" 
                   placeholder="${escapeHtml(placeholder)}" 
                   class="search-field"
                   minlength="${minLength}">
            <button class="search-clear" onclick="clearSearch()" style="display: none;">&times;</button>
        </div>
    `;
    
    const input = container.querySelector('.search-field');
    const clearBtn = container.querySelector('.search-clear');
    
    // Handle input changes
    input.addEventListener('input', (e) => {
        const value = e.target.value.trim();
        
        // Show/hide clear button
        clearBtn.style.display = value ? 'block' : 'none';
        
        // Clear previous timer
        clearTimeout(debounceTimer);
        
        // Set new timer
        debounceTimer = setTimeout(() => {
            if (value.length >= minLength || value.length === 0) {
                onSearch(value);
            }
        }, debounceMs);
    });
    
    // Handle clear button
    window.clearSearch = () => {
        input.value = '';
        clearBtn.style.display = 'none';
        onSearch('');
    };
}

/**
 * Create filter dropdown
 * @param {string} containerId - Container element ID
 * @param {Array} options - Filter options
 * @param {Function} onChange - Change callback function
 * @param {Object} config - Filter configuration
 */
export function createFilterDropdown(containerId, options, onChange, config = {}) {
    const container = document.getElementById(containerId);
    if (!container) return;
    
    const {
        placeholder = 'Filter by...',
        defaultValue = '',
        label = ''
    } = config;
    
    container.innerHTML = `
        <div class="filter-dropdown">
            ${label ? `<label>${escapeHtml(label)}</label>` : ''}
            <select class="filter-select">
                <option value="">${escapeHtml(placeholder)}</option>
                ${options.map(option => `
                    <option value="${escapeHtml(option.value)}" ${option.value === defaultValue ? 'selected' : ''}>
                        ${escapeHtml(option.label)}
                    </option>
                `).join('')}
            </select>
        </div>
    `;
    
    const select = container.querySelector('.filter-select');
    select.addEventListener('change', (e) => {
        onChange(e.target.value);
    });
}

/**
 * Show loading spinner
 * @param {string} containerId - Container element ID
 * @param {string} message - Loading message
 */
export function showLoading(containerId, message = 'Loading...') {
    const container = document.getElementById(containerId);
    if (!container) return;
    
    container.innerHTML = `
        <div class="loading-spinner">
            <div class="spinner"></div>
            <p>${escapeHtml(message)}</p>
        </div>
    `;
}

/**
 * Hide loading spinner
 * @param {string} containerId - Container element ID
 */
export function hideLoading(containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;
    
    const loadingElement = container.querySelector('.loading-spinner');
    if (loadingElement) {
        loadingElement.remove();
    }
}

/**
 * Format date for display
 * @param {string|Date} date - Date to format
 * @param {string} format - Date format
 * @returns {string} Formatted date
 */
export function formatDate(date, format = 'MM/DD/YYYY') {
    if (!date) return '';
    
    const d = new Date(date);
    if (isNaN(d.getTime())) return '';
    
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    
    return format
        .replace('YYYY', year)
        .replace('MM', month)
        .replace('DD', day)
        .replace('HH', hours)
        .replace('mm', minutes);
}

/**
 * Escape HTML to prevent XSS
 * @param {string} text - Text to escape
 * @returns {string} Escaped text
 */
export function escapeHtml(text) {
    if (typeof text !== 'string') return text;
    
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

/**
 * Validate form data
 * @param {Object} data - Form data to validate
 * @param {Object} rules - Validation rules
 * @returns {Object} Validation result
 */
export function validateForm(data, rules) {
    const errors = {};
    
    Object.keys(rules).forEach(field => {
        const value = data[field];
        const fieldRules = rules[field];
        
        // Required validation
        if (fieldRules.required && (!value || value.trim() === '')) {
            errors[field] = fieldRules.required;
            return;
        }
        
        // Skip other validations if field is empty and not required
        if (!value || value.trim() === '') return;
        
        // Email validation
        if (fieldRules.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
            errors[field] = fieldRules.email;
            return;
        }
        
        // Min length validation
        if (fieldRules.minLength && value.length < fieldRules.minLength) {
            errors[field] = fieldRules.minLength;
            return;
        }
        
        // Max length validation
        if (fieldRules.maxLength && value.length > fieldRules.maxLength) {
            errors[field] = fieldRules.maxLength;
            return;
        }
        
        // Pattern validation
        if (fieldRules.pattern && !fieldRules.pattern.test(value)) {
            errors[field] = fieldRules.pattern;
            return;
        }
        
        // Custom validation
        if (fieldRules.custom) {
            const customError = fieldRules.custom(value, data);
            if (customError) {
                errors[field] = customError;
            }
        }
    });
    
    return {
        isValid: Object.keys(errors).length === 0,
        errors
    };
} 