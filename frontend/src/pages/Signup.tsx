import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../hooks/redux';
import { register, clearError } from '../store/slices/authSlice';

const Signup: React.FC = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  
  const { isLoading, error, isAuthenticated } = useAppSelector((state) => state.auth);
  
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
  });

  const [validationErrors, setValidationErrors] = useState<string[]>([]);

  // Redirect if already authenticated
  useEffect(() => {
    if (isAuthenticated) {
      navigate('/profile', { replace: true });
    }
  }, [isAuthenticated, navigate]);

  // Clear error when component unmounts
  useEffect(() => {
    return () => {
      dispatch(clearError());
    };
  }, [dispatch]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    
    // Clear validation errors when user starts typing
    if (validationErrors.length > 0) {
      setValidationErrors([]);
    }
  };

  const validateForm = () => {
    const errors: string[] = [];
    
    if (formData.password.length < 6) {
      errors.push('Password must be at least 6 characters long');
    }
    
    if (formData.password !== formData.confirmPassword) {
      errors.push('Passwords do not match');
    }
    
    if (!formData.firstName.trim()) {
      errors.push('First name is required');
    }
    
    if (!formData.lastName.trim()) {
      errors.push('Last name is required');
    }
    
    if (!formData.email.trim()) {
      errors.push('Email is required');
    }
    
    setValidationErrors(errors);
    return errors.length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    try {
      const { confirmPassword, ...registrationData } = formData;
      await dispatch(register(registrationData)).unwrap();
      // Navigation will happen in useEffect when isAuthenticated becomes true
    } catch (error) {
      // Error is handled by Redux state
      console.error('Registration failed:', error);
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <div className="auth-header">
          <h1>Join 241 Runners Awareness</h1>
          <p>Create your account to get started</p>
        </div>

        {error && (
          <div className="error-message">
            {error}
          </div>
        )}

        {validationErrors.length > 0 && (
          <div className="error-message">
            <ul>
              {validationErrors.map((error, index) => (
                <li key={index}>{error}</li>
              ))}
            </ul>
          </div>
        )}

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="firstName">First Name</label>
              <input
                type="text"
                id="firstName"
                name="firstName"
                value={formData.firstName}
                onChange={handleChange}
                required
                disabled={isLoading}
                placeholder="First name"
              />
            </div>

            <div className="form-group">
              <label htmlFor="lastName">Last Name</label>
              <input
                type="text"
                id="lastName"
                name="lastName"
                value={formData.lastName}
                onChange={handleChange}
                required
                disabled={isLoading}
                placeholder="Last name"
              />
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              type="email"
              id="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              required
              disabled={isLoading}
              placeholder="Enter your email"
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              type="password"
              id="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              required
              disabled={isLoading}
              placeholder="Create a password (min 6 characters)"
            />
          </div>

          <div className="form-group">
            <label htmlFor="confirmPassword">Confirm Password</label>
            <input
              type="password"
              id="confirmPassword"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleChange}
              required
              disabled={isLoading}
              placeholder="Confirm your password"
            />
          </div>

          <button 
            type="submit" 
            className="auth-btn primary"
            disabled={isLoading || validationErrors.length > 0}
          >
            {isLoading ? 'Creating Account...' : 'Create Account'}
          </button>
        </form>

        <div className="auth-footer">
          <p>
            Already have an account?{' '}
            <Link to="/login" className="auth-link">
              Sign in here
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
};

export default Signup;
