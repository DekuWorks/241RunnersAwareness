/**
 * ============================================
 * 241 RUNNERS AWARENESS - USER FEEDBACK UTILITY
 * ============================================
 * 
 * Provides user-friendly feedback for errors and notifications
 * Instead of just console.error, show actual user-facing messages
 */

class UserFeedback {
    constructor() {
        this.messageContainer = null;
        this.messageQueue = [];
        this.isProcessing = false;
    }

    /**
     * Initialize feedback system
     */
    init() {
        this.createMessageContainer();
    }

    /**
     * Create message container if it doesn't exist
     */
    createMessageContainer() {
        if (this.messageContainer) return;

        const container = document.createElement('div');
        container.id = 'userFeedbackContainer';
        container.style.cssText = `
            position: fixed;
            top: 80px;
            right: 20px;
            z-index: 10000;
            max-width: 400px;
        `;
        document.body.appendChild(container);
        this.messageContainer = container;
    }

    /**
     * Show error message to user
     * @param {string} message - Error message
     * @param {string} title - Optional title
     * @param {number} duration - Display duration in ms
     */
    showError(message, title = 'Error', duration = 5000) {
        this.showMessage(message, title, 'error', duration);
    }

    /**
     * Show success message to user
     * @param {string} message - Success message
     * @param {string} title - Optional title
     * @param {number} duration - Display duration in ms
     */
    showSuccess(message, title = 'Success', duration = 3000) {
        this.showMessage(message, title, 'success', duration);
    }

    /**
     * Show info message to user
     * @param {string} message - Info message
     * @param {string} title - Optional title
     * @param {number} duration - Display duration in ms
     */
    showInfo(message, title = 'Info', duration = 3000) {
        this.showMessage(message, title, 'info', duration);
    }

    /**
     * Show warning message to user
     * @param {string} message - Warning message
     * @param {string} title - Optional title
     * @param {number} duration - Display duration in ms
     */
    showWarning(message, title = 'Warning', duration = 4000) {
        this.showMessage(message, title, 'warning', duration);
    }

    /**
     * Show message to user
     * @param {string} message - Message text
     * @param {string} title - Message title
     * @param {string} type - Message type (error, success, info, warning)
     * @param {number} duration - Display duration in ms
     */
    showMessage(message, title, type = 'info', duration = 3000) {
        const messageId = `msg-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
        
        const messageEl = document.createElement('div');
        messageEl.id = messageId;
        messageEl.style.cssText = `
            background: ${this.getBackgroundColor(type)};
            color: white;
            padding: 16px 20px;
            margin-bottom: 12px;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            animation: slideIn 0.3s ease-out;
            display: flex;
            align-items: flex-start;
            gap: 12px;
            min-width: 300px;
        `;

        const icon = this.getIcon(type);
        const iconEl = document.createElement('div');
        iconEl.innerHTML = icon;
        iconEl.style.cssText = `
            font-size: 24px;
            line-height: 1;
            flex-shrink: 0;
        `;

        const content = document.createElement('div');
        content.style.cssText = 'flex: 1; min-width: 0;';
        
        if (title) {
            const titleEl = document.createElement('div');
            titleEl.textContent = title;
            titleEl.style.cssText = `
                font-weight: 600;
                font-size: 14px;
                margin-bottom: 4px;
            `;
            content.appendChild(titleEl);
        }

        const messageText = document.createElement('div');
        messageText.textContent = message;
        messageText.style.cssText = `
            font-size: 13px;
            line-height: 1.4;
            word-wrap: break-word;
        `;
        content.appendChild(messageText);

        const closeBtn = document.createElement('button');
        closeBtn.innerHTML = '×';
        closeBtn.style.cssText = `
            background: none;
            border: none;
            color: white;
            font-size: 24px;
            cursor: pointer;
            padding: 0;
            width: 24px;
            height: 24px;
            display: flex;
            align-items: center;
            justify-content: center;
            opacity: 0.8;
            flex-shrink: 0;
        `;
        closeBtn.onmouseover = () => closeBtn.style.opacity = '1';
        closeBtn.onmouseout = () => closeBtn.style.opacity = '0.8';
        closeBtn.onclick = () => this.removeMessage(messageId);

        messageEl.appendChild(iconEl);
        messageEl.appendChild(content);
        messageEl.appendChild(closeBtn);

        this.messageContainer.appendChild(messageEl);

        // Auto-remove after duration
        if (duration > 0) {
            setTimeout(() => {
                this.removeMessage(messageId);
            }, duration);
        }
    }

    /**
     * Remove message
     * @param {string} messageId - Message ID
     */
    removeMessage(messageId) {
        const messageEl = document.getElementById(messageId);
        if (messageEl) {
            messageEl.style.animation = 'slideOut 0.3s ease-in';
            setTimeout(() => {
                if (messageEl.parentNode) {
                    messageEl.parentNode.removeChild(messageEl);
                }
            }, 300);
        }
    }

    /**
     * Get background color based on type
     * @param {string} type - Message type
     * @returns {string} Color code
     */
    getBackgroundColor(type) {
        const colors = {
            error: '#dc2626',      // Red
            success: '#16a34a',    // Green
            warning: '#ea580c',    // Orange
            info: '#0284c7'        // Blue
        };
        return colors[type] || colors.info;
    }

    /**
     * Get icon based on type
     * @param {string} type - Message type
     * @returns {string} Icon HTML
     */
    getIcon(type) {
        const icons = {
            error: '❌',
            success: '✅',
            warning: '⚠️',
            info: 'ℹ️'
        };
        return icons[type] || icons.info;
    }
}

// Create global instance
const userFeedback = new UserFeedback();

// Initialize on DOM load
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => userFeedback.init());
} else {
    userFeedback.init();
}

// Add animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(400px);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOut {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(400px);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);

// Make available globally
window.userFeedback = userFeedback;
