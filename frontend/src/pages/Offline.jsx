import React from 'react';
import { Link } from 'react-router-dom';

const Offline = () => {
  return (
    <div className="offline-container">
      <div className="offline-content">
        <div className="offline-icon">
          <svg width="64" height="64" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="#dc2626"/>
          </svg>
        </div>
        
        <h1>You're Offline</h1>
        <p>It looks like you've lost your internet connection. Don't worry - some features are still available offline.</p>
        
        <div className="offline-features">
          <h3>Available Offline:</h3>
          <ul>
            <li>View previously loaded cases</li>
            <li>Access your profile information</li>
            <li>Browse cached content</li>
            <li>Emergency contact information</li>
          </ul>
        </div>
        
        <div className="offline-actions">
          <button 
            onClick={() => window.location.reload()} 
            className="retry-button"
          >
            Try Again
          </button>
          
          <Link to="/" className="home-button">
            Go to Home
          </Link>
        </div>
        
        <div className="emergency-info">
          <h4>Emergency Information</h4>
          <p>If you need to report an emergency:</p>
          <ul>
            <li><strong>Call 911</strong> for immediate emergency assistance</li>
            <li><strong>National Center for Missing & Exploited Children:</strong> 1-800-THE-LOST</li>
            <li>Contact your local law enforcement</li>
          </ul>
        </div>
      </div>
    </div>
  );
};

export default Offline;
