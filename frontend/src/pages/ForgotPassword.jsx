import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { API_BASE_URL } from '../config/api';
import SEO from '../components/SEO';

const ForgotPassword = () => {
  const [email, setEmail] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [messageType, setMessageType] = useState('');
  const [remainingResets, setRemainingResets] = useState(null);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setMessage('');

    try {
      const response = await fetch(`${API_BASE_URL}/passwordreset/request`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email }),
      });

      const data = await response.json();

      if (response.ok) {
        setMessageType('success');
        setMessage(data.message);
        setRemainingResets(data.remainingResets);
      } else {
        setMessageType('error');
        setMessage(data.message);
        setRemainingResets(data.remainingResets || 0);
      }
    } catch (error) {
      setMessageType('error');
      setMessage('An error occurred. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <>
      <SEO 
        title="Forgot Password"
        description="Reset your password for 241 Runners Awareness account"
        keywords={['password reset', 'forgot password', 'account recovery']}
        url="/forgot-password"
      />
      
      <div className="auth-container">
        <div className="auth-card">
          <div className="auth-header">
            <h1>Forgot Password</h1>
            <p>Enter your email address to reset your password</p>
          </div>

          {message && (
            <div className={`auth-message ${messageType}`}>
              <p>{message}</p>
              {remainingResets !== null && (
                <p className="reset-info">
                  Remaining resets this year: <strong>{remainingResets}/3</strong>
                </p>
              )}
            </div>
          )}

          <form onSubmit={handleSubmit} className="auth-form">
            <div className="form-group">
              <label htmlFor="email">Email Address</label>
              <input
                type="email"
                id="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="Enter your email address"
                required
                disabled={isLoading}
              />
            </div>

            <button 
              type="submit" 
              className="btn-primary auth-btn"
              disabled={isLoading}
            >
              {isLoading ? 'Sending...' : 'Send Reset Link'}
            </button>
          </form>

          <div className="auth-footer">
            <p>
              Remember your password?{' '}
              <Link to="/login" className="auth-link">
                Back to Login
              </Link>
            </p>
            <p>
              Don't have an account?{' '}
              <Link to="/register" className="auth-link">
                Sign Up
              </Link>
            </p>
          </div>

          <div className="password-reset-info">
            <h3>Password Reset Information</h3>
            <ul>
              <li>You can reset your password up to <strong>3 times per year</strong></li>
              <li>Reset links expire after <strong>24 hours</strong></li>
              <li>Check your email (and spam folder) for the reset link</li>
              <li>If you don't receive an email, the account may not exist</li>
            </ul>
          </div>
        </div>
      </div>
    </>
  );
};

export default ForgotPassword;
