import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { GoogleLogin } from '@react-oauth/google';
import { Link, useNavigate } from 'react-router-dom';
import { login, loginWithGoogle, reset } from '../features/auth/authSlice';

const LoginPage = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { user, loading, error, isSuccess } = useSelector((state) => state.auth);

  const [formData, setFormData] = useState({
    email: '',
    password: '',
  });
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

  const handleGoogleSuccess = (credentialResponse) => {
    setMessage('');
    dispatch(loginWithGoogle({ token: credentialResponse.credential }));
  };

  const handleGoogleError = () => {
    setMessage('Google Login Failed. Please try again.');
  };

  return (
    <div className="form-container">
      <h2>Log In</h2>
      <div className="divider"></div>
      
      <div className="google-login">
        <GoogleLogin
          onSuccess={handleGoogleSuccess}
          onError={handleGoogleError}
          text="signin_with"
          shape="rectangular"
          size="large"
          logo_alignment="left"
        />
      </div>
      
      <div className="divider"></div>
      
      {message && <p style={{ color: 'red', textAlign: 'center' }}>{message}</p>}

      <form onSubmit={handleTraditionalLogin}>
        <div>
          <label htmlFor="email">Email *</label>
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
        <div>
          <label htmlFor="password">Password *</label>
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
        <button type="submit" disabled={loading}>
          {loading ? 'Logging In...' : 'Continue'}
        </button>
      </form>
      <p style={{ marginTop: '1rem', textAlign: 'center' }}>
        Don't have an account? <Link to="/register" style={{ color: '#007acc' }}>Sign Up</Link>
      </p>
    </div>
  );
};

export default LoginPage; 