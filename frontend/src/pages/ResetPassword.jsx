import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate, Link } from 'react-router-dom';
import { API_BASE_URL } from '../config/api';
import SEO from '../components/SEO';

const ResetPassword = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [token, setToken] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [messageType, setMessageType] = useState('');
  const [isValidToken, setIsValidToken] = useState(false);
  const [isValidating, setIsValidating] = useState(true);

  useEffect(() => {
    const emailParam = searchParams.get('email');
    const tokenParam = searchParams.get('token');

    if (!emailParam || !tokenParam) {
      setMessage('Invalid reset link. Please request a new password reset.');
      setMessageType('error');
      setIsValidating(false);
      return;
    }

    setEmail(emailParam);
    setToken(tokenParam);

    // Validate the token
    validateToken(emailParam, tokenParam);
  }, [searchParams]);

  const validateToken = async (email, token) => {
    try {
      const response = await fetch(`${API_BASE_URL}/passwordreset/validate-token`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, token }),
      });

      if (response.ok) {
        setIsValidToken(true);
      } else {
        setMessage('Invalid or expired reset link. Please request a new password reset.');
        setMessageType('error');
      }
    } catch (error) {
      setMessage('An error occurred while validating the reset link.');
      setMessageType('error');
    } finally {
      setIsValidating(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setMessage('');

    // Validate passwords match
    if (newPassword !== confirmPassword) {
      setMessageType('error');
      setMessage('Passwords do not match.');
      setIsLoading(false);
      return;
    }

    // Validate password strength
    if (newPassword.length < 8) {
      setMessageType('error');
      setMessage('Password must be at least 8 characters long.');
      setIsLoading(false);
      return;
    }

    try {
      const response = await fetch(`${API_BASE_URL}/passwordreset/reset`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, token, newPassword }),
      });

      const data = await response.json();

      if (response.ok) {
        setMessageType('success');
        setMessage(data.message);
        // Redirect to login after 3 seconds
        setTimeout(() => {
          navigate('/login');
        }, 3000);
      } else {
        setMessageType('error');
        setMessage(data.message);
      }
    } catch (error) {
      setMessageType('error');
      setMessage('An error occurred. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  if (isValidating) {
    return (
      <div className="auth-container">
        <div className="auth-card">
          <div className="auth-header">
            <h1>Validating Reset Link</h1>
            <p>Please wait while we validate your password reset link...</p>
          </div>
        </div>
      </div>
    );
  }

  if (!isValidToken) {
    return (
      <div className="auth-container">
        <div className="auth-card">
          <div className="auth-header">
            <h1>Invalid Reset Link</h1>
            <p>The password reset link is invalid or has expired.</p>
          </div>
          
          {message && (
            <div className={`auth-message ${messageType}`}>
              <p>{message}</p>
            </div>
          )}

          <div className="auth-footer">
            <Link to="/forgot-password" className="btn-primary auth-btn">
              Request New Reset Link
            </Link>
            <p>
              <Link to="/login" className="auth-link">
                Back to Login
              </Link>
            </p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <>
      <SEO 
        title="Reset Password"
        description="Set your new password for 241 Runners Awareness account"
        keywords={['reset password', 'new password', 'account security']}
        url="/reset-password"
      />
      
      <div className="auth-container">
        <div className="auth-card">
          <div className="auth-header">
            <h1>Reset Password</h1>
            <p>Enter your new password below</p>
          </div>

          {message && (
            <div className={`auth-message ${messageType}`}>
              <p>{message}</p>
            </div>
          )}

          <form onSubmit={handleSubmit} className="auth-form">
            <div className="form-group">
              <label htmlFor="newPassword">New Password</label>
              <input
                type="password"
                id="newPassword"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                placeholder="Enter your new password"
                required
                disabled={isLoading}
                minLength={8}
              />
              <small>Password must be at least 8 characters long</small>
            </div>

            <div className="form-group">
              <label htmlFor="confirmPassword">Confirm New Password</label>
              <input
                type="password"
                id="confirmPassword"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                placeholder="Confirm your new password"
                required
                disabled={isLoading}
                minLength={8}
              />
            </div>

            <button 
              type="submit" 
              className="btn-primary auth-btn"
              disabled={isLoading}
            >
              {isLoading ? 'Resetting...' : 'Reset Password'}
            </button>
          </form>

          <div className="auth-footer">
            <p>
              Remember your password?{' '}
              <Link to="/login" className="auth-link">
                Back to Login
              </Link>
            </p>
          </div>

          <div className="password-reset-info">
            <h3>Password Requirements</h3>
            <ul>
              <li>Password must be at least <strong>8 characters long</strong></li>
              <li>Use a combination of letters, numbers, and symbols</li>
              <li>Avoid using common passwords or personal information</li>
              <li>Consider using a password manager for better security</li>
            </ul>
          </div>
        </div>
      </div>
    </>
  );
};

export default ResetPassword;
