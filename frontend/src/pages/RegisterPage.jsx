import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { GoogleLogin } from '@react-oauth/google';
import { useNavigate, Link } from 'react-router-dom';
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
    role: 'user',
    // Common fields
    address: '',
    city: '',
    state: '',
    zipCode: '',
    emergencyContactName: '',
    emergencyContactPhone: '',
    emergencyContactRelationship: '',
    // Role-specific fields
    relationshipToRunner: '',
    licenseNumber: '',
    organization: '',
    credentials: '',
    specialization: '',
    yearsOfExperience: '',
    // Individual fields (for non-user roles)
    individualFullName: '',
    individualDateOfBirth: '',
    individualGender: '',
    individualEmergencyContactName: '',
    individualEmergencyContactPhone: ''
  });
  const [message, setMessage] = useState('');

  const { 
    fullName, email, phoneNumber, password, role,
    address, city, state, zipCode, emergencyContactName, emergencyContactPhone, emergencyContactRelationship,
    relationshipToRunner, licenseNumber, organization, credentials, specialization, yearsOfExperience,
    individualFullName, individualDateOfBirth, individualGender, individualEmergencyContactName, individualEmergencyContactPhone
  } = formData;

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
    
    // Prepare the registration data
    const registrationData = {
      fullName,
      email,
      phoneNumber,
      password,
      role,
      address,
      city,
      state,
      zipCode,
      emergencyContactName,
      emergencyContactPhone,
      emergencyContactRelationship,
      relationshipToRunner,
      licenseNumber,
      organization,
      credentials,
      specialization,
      yearsOfExperience
    };

    // Add individual data if role is not 'user' and individual info is provided
    if (role !== 'user' && individualFullName) {
      registrationData.individual = {
        fullName: individualFullName,
        dateOfBirth: individualDateOfBirth,
        gender: individualGender,
        emergencyContact: {
          name: individualEmergencyContactName,
          phone: individualEmergencyContactPhone
        }
      };
    }

    dispatch(register(registrationData));
  };
  
  const handleGoogleSuccess = (credentialResponse) => {
    setMessage('');
    dispatch(loginWithGoogle({ token: credentialResponse.credential }));
  };

  const handleGoogleError = () => {
    setMessage('Google Registration Failed. Please try again.');
  };

  const getRoleSpecificFields = () => {
    switch (role) {
      case 'parent':
      case 'caregiver':
      case 'adoptive_parent':
        return (
          <>
            <div className="form-group">
              <label htmlFor="relationshipToRunner">Relationship to Runner</label>
              <select
                id="relationshipToRunner"
                name="relationshipToRunner"
                value={relationshipToRunner}
                onChange={onChange}
                required
              >
                <option value="">Select relationship</option>
                <option value="parent">Parent</option>
                <option value="guardian">Guardian</option>
                <option value="caregiver">Caregiver</option>
                <option value="adoptive_parent">Adoptive Parent</option>
                <option value="foster_parent">Foster Parent</option>
                <option value="grandparent">Grandparent</option>
                <option value="sibling">Sibling</option>
                <option value="other">Other</option>
              </select>
            </div>
            <div className="form-group">
              <label htmlFor="organization">Organization (if applicable)</label>
              <input
                type="text"
                id="organization"
                name="organization"
                value={organization}
                onChange={onChange}
                placeholder="Enter organization name"
              />
            </div>
            <div className="form-group">
              <label htmlFor="yearsOfExperience">Years of Experience</label>
              <input
                type="text"
                id="yearsOfExperience"
                name="yearsOfExperience"
                value={yearsOfExperience}
                onChange={onChange}
                placeholder="e.g., 5 years"
              />
            </div>
          </>
        );
      case 'aba_therapist':
        return (
          <>
            <div className="form-group">
              <label htmlFor="licenseNumber">License Number</label>
              <input
                type="text"
                id="licenseNumber"
                name="licenseNumber"
                value={licenseNumber}
                onChange={onChange}
                required
                placeholder="Enter your license number"
              />
            </div>
            <div className="form-group">
              <label htmlFor="organization">Organization/Clinic</label>
              <input
                type="text"
                id="organization"
                name="organization"
                value={organization}
                onChange={onChange}
                required
                placeholder="Enter organization name"
              />
            </div>
            <div className="form-group">
              <label htmlFor="credentials">Credentials</label>
              <input
                type="text"
                id="credentials"
                name="credentials"
                value={credentials}
                onChange={onChange}
                required
                placeholder="e.g., BCBA, LPC, etc."
              />
            </div>
            <div className="form-group">
              <label htmlFor="specialization">Specialization</label>
              <input
                type="text"
                id="specialization"
                name="specialization"
                value={specialization}
                onChange={onChange}
                placeholder="e.g., Autism, ADHD, etc."
              />
            </div>
            <div className="form-group">
              <label htmlFor="yearsOfExperience">Years of Experience</label>
              <input
                type="text"
                id="yearsOfExperience"
                name="yearsOfExperience"
                value={yearsOfExperience}
                onChange={onChange}
                required
                placeholder="e.g., 5 years"
              />
            </div>
          </>
        );
      default:
        return null;
    }
  };

  const getIndividualFields = () => {
    if (role === 'user') return null;
    
    return (
      <div className="form-section">
        <h3>Runner Information</h3>
        <div className="form-group">
          <label htmlFor="individualFullName">Runner's Full Name</label>
          <input
            type="text"
            id="individualFullName"
            name="individualFullName"
            value={individualFullName}
            onChange={onChange}
            required
            placeholder="Enter runner's full name"
          />
        </div>
        <div className="form-group">
          <label htmlFor="individualDateOfBirth">Runner's Date of Birth</label>
          <input
            type="date"
            id="individualDateOfBirth"
            name="individualDateOfBirth"
            value={individualDateOfBirth}
            onChange={onChange}
          />
        </div>
        <div className="form-group">
          <label htmlFor="individualGender">Runner's Gender</label>
          <select
            id="individualGender"
            name="individualGender"
            value={individualGender}
            onChange={onChange}
          >
            <option value="">Select gender</option>
            <option value="male">Male</option>
            <option value="female">Female</option>
            <option value="other">Other</option>
            <option value="prefer_not_to_say">Prefer not to say</option>
          </select>
        </div>
        <div className="form-group">
          <label htmlFor="individualEmergencyContactName">Runner's Emergency Contact Name</label>
          <input
            type="text"
            id="individualEmergencyContactName"
            name="individualEmergencyContactName"
            value={individualEmergencyContactName}
            onChange={onChange}
            placeholder="Emergency contact name"
          />
        </div>
        <div className="form-group">
          <label htmlFor="individualEmergencyContactPhone">Runner's Emergency Contact Phone</label>
          <input
            type="tel"
            id="individualEmergencyContactPhone"
            name="individualEmergencyContactPhone"
            value={individualEmergencyContactPhone}
            onChange={onChange}
            placeholder="Emergency contact phone"
          />
        </div>
      </div>
    );
  };

  return (
    <div className="auth-container">
      <h2>Sign Up</h2>
      
      <div className="google-auth-container">
        <GoogleLogin
          onSuccess={handleGoogleSuccess}
          onError={handleGoogleError}
          text="signup_with"
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

      <form onSubmit={handleTraditionalRegister} className="auth-form">
        {/* Basic Information */}
        <div className="form-section">
          <h3>Basic Information</h3>
          <div className="form-group">
            <label htmlFor="fullName">Full Name</label>
            <input
              type="text"
              id="fullName"
              name="fullName"
              value={fullName}
              onChange={onChange}
              required
              placeholder="Enter your full name"
            />
          </div>
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
            <label htmlFor="phoneNumber">Phone Number</label>
            <input
              type="tel"
              id="phoneNumber"
              name="phoneNumber"
              value={phoneNumber}
              onChange={onChange}
              required
              placeholder="Enter your phone number"
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
        </div>

        {/* Role Selection */}
        <div className="form-section">
          <h3>Account Type</h3>
          <div className="form-group">
            <label htmlFor="role">I am a:</label>
            <select
              id="role"
              name="role"
              value={role}
              onChange={onChange}
              required
            >
              <option value="user">General User</option>
              <option value="parent">Parent</option>
              <option value="caregiver">Caregiver</option>
              <option value="aba_therapist">ABA Therapist</option>
              <option value="adoptive_parent">Adoptive Parent</option>
            </select>
          </div>
        </div>

        {/* Role-specific fields */}
        {role !== 'user' && (
          <div className="form-section">
            <h3>Professional Information</h3>
            {getRoleSpecificFields()}
          </div>
        )}

        {/* Address Information */}
        <div className="form-section">
          <h3>Address Information</h3>
          <div className="form-group">
            <label htmlFor="address">Address</label>
            <input
              type="text"
              id="address"
              name="address"
              value={address}
              onChange={onChange}
              placeholder="Enter your address"
            />
          </div>
          <div className="form-group">
            <label htmlFor="city">City</label>
            <input
              type="text"
              id="city"
              name="city"
              value={city}
              onChange={onChange}
              placeholder="Enter your city"
            />
          </div>
          <div className="form-group">
            <label htmlFor="state">State</label>
            <input
              type="text"
              id="state"
              name="state"
              value={state}
              onChange={onChange}
              placeholder="Enter your state"
            />
          </div>
          <div className="form-group">
            <label htmlFor="zipCode">Zip Code</label>
            <input
              type="text"
              id="zipCode"
              name="zipCode"
              value={zipCode}
              onChange={onChange}
              placeholder="Enter your zip code"
            />
          </div>
        </div>

        {/* Emergency Contact */}
        <div className="form-section">
          <h3>Emergency Contact</h3>
          <div className="form-group">
            <label htmlFor="emergencyContactName">Emergency Contact Name</label>
            <input
              type="text"
              id="emergencyContactName"
              name="emergencyContactName"
              value={emergencyContactName}
              onChange={onChange}
              placeholder="Emergency contact name"
            />
          </div>
          <div className="form-group">
            <label htmlFor="emergencyContactPhone">Emergency Contact Phone</label>
            <input
              type="tel"
              id="emergencyContactPhone"
              name="emergencyContactPhone"
              value={emergencyContactPhone}
              onChange={onChange}
              placeholder="Emergency contact phone"
            />
          </div>
          <div className="form-group">
            <label htmlFor="emergencyContactRelationship">Relationship to You</label>
            <input
              type="text"
              id="emergencyContactRelationship"
              name="emergencyContactRelationship"
              value={emergencyContactRelationship}
              onChange={onChange}
              placeholder="e.g., Spouse, Parent, Friend"
            />
          </div>
        </div>

        {/* Individual Information */}
        {getIndividualFields()}

        <button type="submit" className="auth-button" disabled={loading}>
          {loading ? 'Creating Account...' : 'Create Account'}
        </button>
      </form>
      
      <div className="auth-links">
        <Link to="/login">Already have an account? Log in</Link>
      </div>
    </div>
  );
};

export default RegisterPage; 