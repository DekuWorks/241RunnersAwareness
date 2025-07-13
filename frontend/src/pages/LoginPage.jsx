import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { GoogleLogin } from '@react-oauth/google';
import { Link, useNavigate } from 'react-router-dom';
import { login, loginWithGoogle, reset, loginWith2FA } from '../features/auth/authSlice';

const LoginPage = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { user, loading, error, isSuccess, twoFARequired, twoFAEmail } = useSelector((state) => state.auth);

  const [formData, setFormData] = useState({
    email: '',
    password: '',
  });
  const [totp, setTotp] = useState('');
  const [message, setMessage] = useState('');

  const { email, password } = formData;

  useEffect(() => {
    if (error) {
      setMessage(error);
    }
    if (isSuccess || user) {
      navigate('/');
    }
    dispatch(reset());
  }, [user, isSuccess, error, navigate, dispatch]);

  const onChange = (e) => setFormData({ ...formData, [e.target.name]: e.target.value });

  const handleTraditionalLogin = (e) => {
    e.preventDefault();
    setMessage('');
    dispatch(login({ email, password }));
  };

  const handle2FASubmit = (e) => {
    e.preventDefault();
    setMessage('');
    dispatch(loginWith2FA({ email: twoFAEmail, totp }));
  };

  const handleGoogleSuccess = (credentialResponse) => {
    setMessage('');
    dispatch(loginWithGoogle({ token: credentialResponse.credential }));
  };

  const handleGoogleError = () => {
    setMessage('Google Login Failed. Please try again.');
  };

  return (
    <div className="auth-container">
      <h2>Log In</h2>
      <div className="google-auth-container">
        <GoogleLogin
          onSuccess={handleGoogleSuccess}
          onError={handleGoogleError}
          text="signin_with"
          shape="rectangular"
          size="large"
          logo_alignment="left"
          theme="outline"
        />
      </div>
      <div className="divider">
        <span>or</span>
      </div>
      {message && (
        <div className={message.includes('successful') ? 'success-message' : 'error-message'}>
          {message}
        </div>
      )}
      {!twoFARequired ? (
        <form onSubmit={handleTraditionalLogin} className="auth-form">
          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              type="email"
              id="email"
              name="email"
              value={email}
              onChange={onChange}
              required
              placeholder="Enter your email"
            />
          </div>
          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              type="password"
              id="password"
              name="password"
              value={password}
              onChange={onChange}
              required
              placeholder="Enter your password"
            />
          </div>
          <button 
            type="submit" 
            className="auth-button"
            disabled={loading}
          >
            {loading ? 'Logging in...' : 'Log In'}
          </button>
        </form>
      ) : (
        <form onSubmit={handle2FASubmit} className="auth-form">
          <div className="form-group">
            <label htmlFor="totp">Enter 2FA Code</label>
            <input
              type="text"
              id="totp"
              name="totp"
              value={totp}
              onChange={e => setTotp(e.target.value)}
              required
              maxLength={6}
              pattern="\d{6}"
              placeholder="6-digit code"
              style={{ fontSize: 20, letterSpacing: 2, textAlign: 'center', width: 120 }}
            />
          </div>
          <button
            type="submit"
            className="auth-button"
            disabled={loading}
          >
            {loading ? 'Verifying...' : 'Verify 2FA'}
          </button>
        </form>
      )}
      <div className="auth-links">
        <Link to="/register">Don't have an account? Sign up</Link>
      </div>
    </div>
  );
};

export default LoginPage; 