import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { GoogleLogin } from '@react-oauth/google';
import { useNavigate } from 'react-router-dom';
import { register, loginWithGoogle, reset } from '../features/auth/authSlice';

// TODO: Create and import the register and googleLogin async thunks from authSlice
// import { register, loginWithGoogle } from '../features/auth/authSlice';

const RegisterPage = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { user, loading, error, isSuccess } = useSelector((state) => state.auth);
  
  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    phoneNumber: '',
    password: '',
  });
  const [message, setMessage] = useState('');

  const { fullName, email, phoneNumber, password } = formData;

  useEffect(() => {
    if (error) {
      setMessage(error);
    }
    if (isSuccess || user) {
      navigate('/'); // Redirect on successful login/registration
    }
    dispatch(reset());
  }, [user, isSuccess, error, navigate, dispatch]);

  const onChange = (e) => setFormData({ ...formData, [e.target.name]: e.target.value });

  const handleTraditionalRegister = (e) => {
    e.preventDefault();
    setMessage('');
    dispatch(register({ fullName, email, phoneNumber, password }));
  };
  
  const handleGoogleSuccess = (credentialResponse) => {
    setMessage('');
    dispatch(loginWithGoogle({ token: credentialResponse.credential }));
  };

  const handleGoogleError = () => {
    setMessage('Google Registration Failed. Please try again.');
  };

  return (
    <div className="signup-card" style={{ background: 'white', maxWidth: '600px', margin: 'auto', padding: '2rem', borderRadius: '10px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)' }}>
      <h2 style={{ textAlign: 'center', color: '#222' }}>Sign Up</h2>
      <div style={{ width: '100%', height: '1px', background: '#eee', margin: '1rem 0' }}></div>

      <GoogleLogin
        onSuccess={handleGoogleSuccess}
        onError={handleGoogleError}
        text="signup_with"
        shape="rectangular"
        size="large"
        logo_alignment="left"
      />

      <div style={{ width: '100%', height: '1px', background: '#eee', margin: '1rem 0' }}></div>
      
      {message && <p style={{ color: 'red', textAlign: 'center' }}>{message}</p>}

      <form onSubmit={handleTraditionalRegister} style={{ width: '100%', maxWidth: '400px', display: 'flex', flexDirection: 'column', gap: '1rem' }}>
        <div>
          <label htmlFor="fullName">Full Name *</label>
          <input
            type="text"
            id="fullName"
            name="fullName"
            value={fullName}
            onChange={onChange}
            required
            placeholder="Enter your full name"
            style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #bbb' }}
          />
        </div>
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
            style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #bbb' }}
          />
        </div>
        <div>
          <label htmlFor="phoneNumber">Phone Number *</label>
          <input
            type="tel"
            id="phoneNumber"
            name="phoneNumber"
            value={phoneNumber}
            onChange={onChange}
            required
            placeholder="Enter your phone number"
            style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #bbb' }}
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
            style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #bbb' }}
          />
        </div>
        <button type="submit" disabled={loading} style={{ padding: '10px', background: '#007acc', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
          {loading ? 'Creating Account...' : 'Continue'}
        </button>
      </form>
    </div>
  );
};

export default RegisterPage; 