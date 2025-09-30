/**
 * 241 Runners Awareness - Input Validation Schemas
 * 
 * This file contains Zod validation schemas for all form inputs
 * to prevent XSS attacks, SQL injection, and ensure data integrity.
 */

// Import Zod for validation
import { z } from 'https://cdn.skypack.dev/zod';

/**
 * ============================================
 * VALIDATION SCHEMAS
 * ============================================
 */

// Email validation with proper regex
const emailSchema = z.string()
    .min(1, 'Email is required')
    .max(255, 'Email cannot exceed 255 characters')
    .email('Please enter a valid email address')
    .regex(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/, 'Invalid email format');

// Password validation with strength requirements
const passwordSchema = z.string()
    .min(8, 'Password must be at least 8 characters long')
    .max(128, 'Password cannot exceed 128 characters')
    .regex(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/, 'Password must contain at least one uppercase letter, one lowercase letter, and one number')
    .regex(/^[a-zA-Z0-9!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]*$/, 'Password contains invalid characters');

// Name validation with length limits
const nameSchema = z.string()
    .min(1, 'Name is required')
    .max(100, 'Name cannot exceed 100 characters')
    .regex(/^[a-zA-Z\s\-'\.]+$/, 'Name can only contain letters, spaces, hyphens, apostrophes, and periods')
    .transform(str => str.trim());

// Description validation
const descriptionSchema = z.string()
    .min(1, 'Description is required')
    .max(1000, 'Description cannot exceed 1000 characters')
    .transform(str => str.trim());

// Location validation
const locationSchema = z.string()
    .min(1, 'Location is required')
    .max(200, 'Location cannot exceed 200 characters')
    .regex(/^[a-zA-Z0-9\s\-'\.\,]+$/, 'Location contains invalid characters')
    .transform(str => str.trim());

// Age validation
const ageSchema = z.number()
    .int('Age must be a whole number')
    .min(1, 'Age must be at least 1')
    .max(120, 'Age cannot exceed 120');

// Gender validation
const genderSchema = z.enum(['male', 'female', 'other', 'prefer-not-to-say'], {
    errorMap: () => ({ message: 'Please select a valid gender option' })
});

// File validation
const fileSchema = z.object({
    name: z.string().min(1, 'File name is required'),
    size: z.number().max(5 * 1024 * 1024, 'File size cannot exceed 5MB'),
    type: z.string().regex(/^image\/(jpeg|jpg|png|webp)$/, 'Only JPEG, PNG, and WebP images are allowed')
});

/**
 * ============================================
 * FORM SCHEMAS
 * ============================================
 */

// Sign-up form validation
export const signupSchema = z.object({
    firstName: nameSchema,
    lastName: nameSchema,
    email: emailSchema,
    password: passwordSchema,
    confirmPassword: z.string().min(1, 'Please confirm your password'),
    role: z.enum(['user', 'parent', 'caregiver', 'therapist', 'adoptiveparent'], {
        errorMap: () => ({ message: 'Please select a valid role' })
    }),
    agreeToTerms: z.boolean().refine(val => val === true, 'You must agree to the terms and conditions')
}).refine(data => data.password === data.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword']
});

// Login form validation
export const loginSchema = z.object({
    email: emailSchema,
    password: z.string().min(1, 'Password is required')
});

// Runner report form validation
export const runnerReportSchema = z.object({
    firstName: nameSchema,
    lastName: nameSchema,
    age: ageSchema,
    gender: genderSchema,
    height: z.string()
        .min(1, 'Height is required')
        .max(50, 'Height cannot exceed 50 characters')
        .regex(/^[0-9]+'[0-9]+"$|^[0-9]+cm$|^[0-9]+\.[0-9]+m$/, 'Please enter height in format: 5\'10" or 180cm or 1.8m'),
    weight: z.string()
        .min(1, 'Weight is required')
        .max(50, 'Weight cannot exceed 50 characters')
        .regex(/^[0-9]+lbs?$|^[0-9]+kg$|^[0-9]+\.[0-9]+kg$/, 'Please enter weight in format: 150lbs or 68kg'),
    hairColor: z.string()
        .min(1, 'Hair color is required')
        .max(50, 'Hair color cannot exceed 50 characters')
        .regex(/^[a-zA-Z\s\-]+$/, 'Hair color contains invalid characters'),
    eyeColor: z.string()
        .min(1, 'Eye color is required')
        .max(50, 'Eye color cannot exceed 50 characters')
        .regex(/^[a-zA-Z\s\-]+$/, 'Eye color contains invalid characters'),
    lastSeenLocation: locationSchema,
    lastSeenDate: z.string()
        .min(1, 'Last seen date is required')
        .regex(/^\d{4}-\d{2}-\d{2}$/, 'Please enter date in YYYY-MM-DD format'),
    description: descriptionSchema,
    identifyingMarks: z.string()
        .max(500, 'Identifying marks cannot exceed 500 characters')
        .optional()
        .transform(str => str ? str.trim() : ''),
    medicalConditions: z.string()
        .max(500, 'Medical conditions cannot exceed 500 characters')
        .optional()
        .transform(str => str ? str.trim() : ''),
    isUrgent: z.boolean().default(false),
    contactPhone: z.string()
        .regex(/^[\+]?[1-9][\d]{0,15}$/, 'Please enter a valid phone number')
        .optional(),
    contactEmail: emailSchema.optional(),
    images: z.array(fileSchema).max(5, 'Maximum 5 images allowed').optional()
});

// Profile update validation
export const profileUpdateSchema = z.object({
    firstName: nameSchema,
    lastName: nameSchema,
    email: emailSchema,
    phone: z.string()
        .regex(/^[\+]?[1-9][\d]{0,15}$/, 'Please enter a valid phone number')
        .optional(),
    currentPassword: z.string().min(1, 'Current password is required'),
    newPassword: passwordSchema.optional(),
    confirmNewPassword: z.string().optional()
}).refine(data => {
    if (data.newPassword && data.newPassword !== data.confirmNewPassword) {
        return false;
    }
    return true;
}, {
    message: 'New passwords do not match',
    path: ['confirmNewPassword']
});

/**
 * ============================================
 * VALIDATION FUNCTIONS
 * ============================================
 */

/**
 * Validate form data against a schema
 * @param {Object} data - Form data to validate
 * @param {Object} schema - Zod schema to validate against
 * @returns {Object} - Validation result with success boolean and errors
 */
export function validateFormData(data, schema) {
    try {
        const validatedData = schema.parse(data);
        return {
            success: true,
            data: validatedData,
            errors: null
        };
    } catch (error) {
        if (error instanceof z.ZodError) {
            const errors = {};
            error.errors.forEach(err => {
                const path = err.path.join('.');
                errors[path] = err.message;
            });
            return {
                success: false,
                data: null,
                errors: errors
            };
        }
        return {
            success: false,
            data: null,
            errors: { general: 'Validation failed' }
        };
    }
}

/**
 * Sanitize text input to prevent XSS attacks
 * @param {string} input - Text to sanitize
 * @returns {string} - Sanitized text
 */
export function sanitizeInput(input) {
    if (typeof input !== 'string') return input;
    
    return input
        .replace(/[<>]/g, '') // Remove < and > characters
        .replace(/javascript:/gi, '') // Remove javascript: protocol
        .replace(/on\w+=/gi, '') // Remove event handlers
        .trim();
}

/**
 * Validate file upload
 * @param {File} file - File to validate
 * @returns {Object} - Validation result
 */
export function validateFile(file) {
    try {
        const fileData = {
            name: file.name,
            size: file.size,
            type: file.type
        };
        
        const result = fileSchema.parse(fileData);
        return {
            success: true,
            data: result,
            errors: null
        };
    } catch (error) {
        if (error instanceof z.ZodError) {
            return {
                success: false,
                data: null,
                errors: error.errors.map(err => err.message)
            };
        }
        return {
            success: false,
            data: null,
            errors: ['Invalid file']
        };
    }
}

/**
 * Validate email format
 * @param {string} email - Email to validate
 * @returns {boolean} - True if valid email
 */
export function isValidEmail(email) {
    try {
        emailSchema.parse(email);
        return true;
    } catch {
        return false;
    }
}

/**
 * Validate password strength
 * @param {string} password - Password to validate
 * @returns {Object} - Validation result with strength score
 */
export function validatePasswordStrength(password) {
    const result = {
        isValid: false,
        score: 0,
        feedback: []
    };
    
    if (password.length < 8) {
        result.feedback.push('Password must be at least 8 characters long');
    } else {
        result.score += 1;
    }
    
    if (!/[a-z]/.test(password)) {
        result.feedback.push('Password must contain at least one lowercase letter');
    } else {
        result.score += 1;
    }
    
    if (!/[A-Z]/.test(password)) {
        result.feedback.push('Password must contain at least one uppercase letter');
    } else {
        result.score += 1;
    }
    
    if (!/\d/.test(password)) {
        result.feedback.push('Password must contain at least one number');
    } else {
        result.score += 1;
    }
    
    if (!/[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password)) {
        result.feedback.push('Password should contain at least one special character');
    } else {
        result.score += 1;
    }
    
    result.isValid = result.score >= 4 && password.length >= 8;
    
    return result;
}

/**
 * ============================================
 * REAL-TIME VALIDATION HELPERS
 * ============================================
 */

/**
 * Add real-time validation to form inputs
 * @param {HTMLFormElement} form - Form element
 * @param {Object} schema - Zod schema for validation
 */
export function addRealTimeValidation(form, schema) {
    const inputs = form.querySelectorAll('input, textarea, select');
    
    inputs.forEach(input => {
        input.addEventListener('blur', () => {
            validateField(input, schema);
        });
        
        input.addEventListener('input', () => {
            clearFieldError(input);
        });
    });
}

/**
 * Validate individual field
 * @param {HTMLElement} field - Field to validate
 * @param {Object} schema - Schema to validate against
 */
function validateField(field, schema) {
    const fieldName = field.name;
    if (!fieldName) return;
    
    try {
        const fieldSchema = schema.shape[fieldName];
        if (fieldSchema) {
            fieldSchema.parse(field.value);
            clearFieldError(field);
        }
    } catch (error) {
        if (error instanceof z.ZodError) {
            showFieldError(field, error.errors[0].message);
        }
    }
}

/**
 * Show field error
 * @param {HTMLElement} field - Field to show error for
 * @param {string} message - Error message
 */
function showFieldError(field, message) {
    clearFieldError(field);
    
    const errorElement = document.createElement('div');
    errorElement.className = 'field-error';
    errorElement.textContent = message;
    errorElement.style.cssText = `
        color: #dc3545;
        font-size: 0.875rem;
        margin-top: 0.25rem;
    `;
    
    field.parentNode.insertBefore(errorElement, field.nextSibling);
    field.classList.add('error');
}

/**
 * Clear field error
 * @param {HTMLElement} field - Field to clear error for
 */
function clearFieldError(field) {
    const errorElement = field.parentNode.querySelector('.field-error');
    if (errorElement) {
        errorElement.remove();
    }
    field.classList.remove('error');
}

// Export schemas for use in other files
export {
    emailSchema,
    passwordSchema,
    nameSchema,
    descriptionSchema,
    locationSchema,
    ageSchema,
    genderSchema,
    fileSchema
};
