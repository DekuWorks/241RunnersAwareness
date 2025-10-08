/**
 * Comprehensive validation utilities for admin dashboard
 * Provides client-side validation for all management operations
 */

class AdminValidation {
    constructor() {
        this.validationRules = {
            // User validation rules
            user: {
                firstName: {
                    required: true,
                    minLength: 1,
                    maxLength: 100,
                    pattern: /^[a-zA-Z\s\-'\.]+$/,
                    message: 'First name can only contain letters, spaces, hyphens, apostrophes, and periods'
                },
                lastName: {
                    required: true,
                    minLength: 1,
                    maxLength: 100,
                    pattern: /^[a-zA-Z\s\-'\.]+$/,
                    message: 'Last name can only contain letters, spaces, hyphens, apostrophes, and periods'
                },
                email: {
                    required: true,
                    pattern: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                    message: 'Please enter a valid email address'
                },
                phoneNumber: {
                    required: false,
                    pattern: /^[\+]?[1-9][\d]{0,15}$/,
                    message: 'Please enter a valid phone number'
                },
                city: {
                    required: false,
                    pattern: /^[a-zA-Z\s\-'\.]+$/,
                    message: 'City can only contain letters, spaces, hyphens, apostrophes, and periods'
                },
                state: {
                    required: false,
                    pattern: /^[a-zA-Z\s\-'\.]+$/,
                    message: 'State can only contain letters, spaces, hyphens, apostrophes, and periods'
                },
                zipCode: {
                    required: false,
                    pattern: /^[\d\-]+$/,
                    message: 'Zip code can only contain numbers and hyphens'
                },
                organization: {
                    required: false,
                    pattern: /^[a-zA-Z0-9\s\-'\.&()]+$/,
                    message: 'Organization can only contain letters, numbers, spaces, hyphens, apostrophes, periods, ampersands, and parentheses'
                },
                title: {
                    required: false,
                    pattern: /^[a-zA-Z\s\-'\.]+$/,
                    message: 'Title can only contain letters, spaces, hyphens, apostrophes, and periods'
                },
                emergencyContactPhone: {
                    required: false,
                    pattern: /^[\+]?[1-9][\d]{0,15}$/,
                    message: 'Please enter a valid emergency contact phone number'
                },
                notes: {
                    required: false,
                    maxLength: 2000,
                    message: 'Notes cannot exceed 2000 characters'
                }
            },
            // Case validation rules
            case: {
                title: {
                    required: true,
                    minLength: 1,
                    maxLength: 200,
                    message: 'Case title is required and cannot exceed 200 characters'
                },
                description: {
                    required: true,
                    minLength: 1,
                    maxLength: 2000,
                    message: 'Case description is required and cannot exceed 2000 characters'
                },
                lastSeenLocation: {
                    required: true,
                    minLength: 1,
                    maxLength: 500,
                    message: 'Last seen location is required and cannot exceed 500 characters'
                },
                lastSeenDate: {
                    required: true,
                    message: 'Last seen date is required'
                },
                priority: {
                    required: true,
                    pattern: /^(Low|Medium|High|Critical)$/,
                    message: 'Priority must be one of: Low, Medium, High, Critical'
                },
                contactPersonPhone: {
                    required: false,
                    pattern: /^[\+]?[1-9][\d]{0,15}$/,
                    message: 'Please enter a valid contact phone number'
                },
                contactPersonEmail: {
                    required: false,
                    pattern: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                    message: 'Please enter a valid contact email'
                }
            }
        };
    }

    /**
     * Validate a single field
     */
    validateField(fieldName, value, rules) {
        const errors = [];
        const warnings = [];

        // Required validation
        if (rules.required && (!value || value.toString().trim() === '')) {
            errors.push(`${fieldName} is required`);
            return { isValid: false, errors, warnings };
        }

        // Skip other validations if field is empty and not required
        if (!value || value.toString().trim() === '') {
            return { isValid: true, errors, warnings };
        }

        const stringValue = value.toString().trim();

        // Min length validation
        if (rules.minLength && stringValue.length < rules.minLength) {
            errors.push(`${fieldName} must be at least ${rules.minLength} characters long`);
        }

        // Max length validation
        if (rules.maxLength && stringValue.length > rules.maxLength) {
            errors.push(`${fieldName} cannot exceed ${rules.maxLength} characters`);
        }

        // Pattern validation
        if (rules.pattern && !rules.pattern.test(stringValue)) {
            errors.push(rules.message || `${fieldName} format is invalid`);
        }

        // Custom validation
        if (rules.custom) {
            const customResult = rules.custom(stringValue);
            if (customResult && !customResult.isValid) {
                if (customResult.isWarning) {
                    warnings.push(customResult.message);
                } else {
                    errors.push(customResult.message);
                }
            }
        }

        return {
            isValid: errors.length === 0,
            errors,
            warnings
        };
    }

    /**
     * Validate user data
     */
    validateUser(userData) {
        const errors = [];
        const warnings = [];
        const rules = this.validationRules.user;

        // Validate each field
        Object.keys(rules).forEach(fieldName => {
            const fieldRules = rules[fieldName];
            const fieldValue = userData[fieldName];
            const result = this.validateField(fieldName, fieldValue, fieldRules);
            
            errors.push(...result.errors);
            warnings.push(...result.warnings);
        });

        // Additional business logic validations
        this.validateUserBusinessRules(userData, errors, warnings);

        return {
            isValid: errors.length === 0,
            errors,
            warnings
        };
    }

    /**
     * Validate case data
     */
    validateCase(caseData) {
        const errors = [];
        const warnings = [];
        const rules = this.validationRules.case;

        // Validate each field
        Object.keys(rules).forEach(fieldName => {
            const fieldRules = rules[fieldName];
            const fieldValue = caseData[fieldName];
            const result = this.validateField(fieldName, fieldValue, fieldRules);
            
            errors.push(...result.errors);
            warnings.push(...result.warnings);
        });

        // Additional business logic validations
        this.validateCaseBusinessRules(caseData, errors, warnings);

        return {
            isValid: errors.length === 0,
            errors,
            warnings
        };
    }

    /**
     * Validate user business rules
     */
    validateUserBusinessRules(userData, errors, warnings) {
        // Check if email is already in use (would need API call)
        // This would be handled on the server side

        // Check if phone number format is valid for the country
        if (userData.phoneNumber && userData.phoneNumber.length < 10) {
            warnings.push('Phone number seems too short for a valid US number');
        }

        // Check if emergency contact is provided when required
        if (userData.role === 'parent' || userData.role === 'caregiver') {
            if (!userData.emergencyContactName || !userData.emergencyContactPhone) {
                warnings.push('Emergency contact information is recommended for this role');
            }
        }
    }

    /**
     * Validate case business rules
     */
    validateCaseBusinessRules(caseData, errors, warnings) {
        // Check if last seen date is not in the future
        if (caseData.lastSeenDate) {
            const lastSeenDate = new Date(caseData.lastSeenDate);
            const now = new Date();
            if (lastSeenDate > now) {
                errors.push('Last seen date cannot be in the future');
            }

            // Check if date is too far in the past (more than 1 year)
            const oneYearAgo = new Date();
            oneYearAgo.setFullYear(oneYearAgo.getFullYear() - 1);
            if (lastSeenDate < oneYearAgo) {
                warnings.push('Last seen date is more than 1 year ago. Please verify the date.');
            }
        }

        // Check if high priority cases have contact information
        if (caseData.priority === 'High' || caseData.priority === 'Critical') {
            if (!caseData.contactPersonPhone && !caseData.contactPersonEmail) {
                warnings.push('High priority cases should have contact information');
            }
        }
    }

    /**
     * Validate deletion operation
     */
    async validateDeletion(entityType, entityId, currentUserId) {
        const errors = [];
        const warnings = [];

        // Prevent self-deletion
        if (entityId === currentUserId) {
            errors.push('You cannot delete your own account');
        }

        // Check for related data (would need API call)
        try {
            const response = await fetch(`/api/admin/validate-deletion/${entityType}/${entityId}`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('adminToken')}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const result = await response.json();
                if (result.warnings) {
                    warnings.push(...result.warnings.map(w => w.message));
                }
                if (result.errors) {
                    errors.push(...result.errors.map(e => e.message));
                }
            }
        } catch (error) {
            console.warn('Could not validate deletion constraints:', error);
            warnings.push('Could not verify deletion constraints. Proceed with caution.');
        }

        return {
            isValid: errors.length === 0,
            errors,
            warnings
        };
    }

    /**
     * Show validation errors in UI
     */
    showValidationErrors(containerId, errors, warnings = []) {
        const container = document.getElementById(containerId);
        if (!container) return;

        // Clear existing messages
        container.innerHTML = '';

        // Show errors
        if (errors.length > 0) {
            const errorDiv = document.createElement('div');
            errorDiv.className = 'validation-errors alert alert-danger';
            errorDiv.innerHTML = `
                <h6><i class="fas fa-exclamation-triangle"></i> Validation Errors:</h6>
                <ul>
                    ${errors.map(error => `<li>${error}</li>`).join('')}
                </ul>
            `;
            container.appendChild(errorDiv);
        }

        // Show warnings
        if (warnings.length > 0) {
            const warningDiv = document.createElement('div');
            warningDiv.className = 'validation-warnings alert alert-warning';
            warningDiv.innerHTML = `
                <h6><i class="fas fa-exclamation-circle"></i> Warnings:</h6>
                <ul>
                    ${warnings.map(warning => `<li>${warning}</li>`).join('')}
                </ul>
            `;
            container.appendChild(warningDiv);
        }
    }

    /**
     * Clear validation messages
     */
    clearValidationMessages(containerId) {
        const container = document.getElementById(containerId);
        if (container) {
            container.innerHTML = '';
        }
    }

    /**
     * Validate form in real-time
     */
    setupRealTimeValidation(formId, entityType) {
        const form = document.getElementById(formId);
        if (!form) return;

        const rules = this.validationRules[entityType];
        if (!rules) return;

        // Add event listeners to form fields
        Object.keys(rules).forEach(fieldName => {
            const field = form.querySelector(`[name="${fieldName}"]`);
            if (field) {
                field.addEventListener('blur', () => {
                    this.validateFormField(field, rules[fieldName]);
                });

                field.addEventListener('input', () => {
                    // Clear previous validation state
                    field.classList.remove('is-valid', 'is-invalid');
                    const feedback = field.parentNode.querySelector('.invalid-feedback');
                    if (feedback) {
                        feedback.remove();
                    }
                });
            }
        });
    }

    /**
     * Validate a single form field
     */
    validateFormField(field, rules) {
        const value = field.value;
        const result = this.validateField(field.name, value, rules);

        // Update field appearance
        field.classList.remove('is-valid', 'is-invalid');
        
        if (value.trim() !== '') {
            if (result.isValid) {
                field.classList.add('is-valid');
            } else {
                field.classList.add('is-invalid');
                
                // Show error message
                const feedback = document.createElement('div');
                feedback.className = 'invalid-feedback';
                feedback.textContent = result.errors[0] || 'Invalid value';
                field.parentNode.appendChild(feedback);
            }
        }

        return result;
    }
}

// Create global instance
window.adminValidation = new AdminValidation();

// Export for module usage
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AdminValidation;
}
