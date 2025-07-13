import React, { useState } from 'react';
import { useSelector } from 'react-redux';

const SettingsPage = () => {
  const { user } = useSelector((state) => state.auth);
  const [twoFAStatus, setTwoFAStatus] = useState(null);
  const [loading, setLoading] = useState(false);
  const [qrCodeUrl, setQrCodeUrl] = useState('');
  const [secret, setSecret] = useState('');
  const [backupCodes, setBackupCodes] = useState([]);
  const [totp, setTotp] = useState('');
  const [message, setMessage] = useState('');

  // Fetch 2FA status on demand
  const fetchStatus = async () => {
    setLoading(true);
    setMessage('');
    try {
      const res = await fetch(`/api/auth/2fa/status/${encodeURIComponent(user.email)}`);
      const data = await res.json();
      setTwoFAStatus(data);
    } catch (e) {
      setMessage('Error fetching 2FA status.');
    }
    setLoading(false);
  };

  // Start 2FA setup
  const startSetup = async () => {
    setLoading(true);
    setMessage('');
    try {
      const res = await fetch('/api/auth/2fa/setup', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: user.email })
      });
      const data = await res.json();
      if (data.success) {
        setQrCodeUrl(data.qrCodeUrl);
        setSecret(data.secret);
        setBackupCodes(data.backupCodes);
        setMessage('Scan the QR code with your authenticator app and enter the code below to enable 2FA.');
      } else {
        setMessage(data.message || 'Failed to start 2FA setup.');
      }
    } catch (e) {
      setMessage('Error starting 2FA setup.');
    }
    setLoading(false);
  };

  // Enable 2FA
  const enable2FA = async (e) => {
    e.preventDefault();
    setLoading(true);
    setMessage('');
    try {
      const res = await fetch('/api/auth/2fa/enable', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: user.email, totp })
      });
      const data = await res.json();
      if (data.success) {
        setMessage('Two-factor authentication enabled!');
        setQrCodeUrl('');
        setSecret('');
        setBackupCodes([]);
        setTotp('');
        fetchStatus();
      } else {
        setMessage(data.message || 'Failed to enable 2FA.');
      }
    } catch (e) {
      setMessage('Error enabling 2FA.');
    }
    setLoading(false);
  };

  // Disable 2FA
  const disable2FA = async (e) => {
    e.preventDefault();
    setLoading(true);
    setMessage('');
    try {
      const res = await fetch('/api/auth/2fa/disable', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: user.email, totp })
      });
      const data = await res.json();
      if (data.success) {
        setMessage('Two-factor authentication disabled.');
        setTotp('');
        fetchStatus();
      } else {
        setMessage(data.message || 'Failed to disable 2FA.');
      }
    } catch (e) {
      setMessage('Error disabling 2FA.');
    }
    setLoading(false);
  };

  // Show backup codes
  const showBackupCodes = () => {
    if (backupCodes.length > 0) {
      return (
        <div style={{ margin: '16px 0' }}>
          <strong>Backup Codes:</strong>
          <ul style={{ margin: '8px 0', padding: 0, listStyle: 'none' }}>
            {backupCodes.map((code, idx) => (
              <li key={idx} style={{ fontFamily: 'monospace', background: '#f5f5f5', display: 'inline-block', margin: '2px 8px 2px 0', padding: '4px 8px', borderRadius: 4 }}>{code}</li>
            ))}
          </ul>
          <small>Save these codes in a safe place. Each can be used once if you lose access to your authenticator app.</small>
        </div>
      );
    }
    return null;
  };

  return (
    <div style={{ maxWidth: 500, margin: '0 auto', padding: 24 }}>
      <h2 className="text-xl font-semibold mb-2">Account Security</h2>
      <button onClick={fetchStatus} disabled={loading} style={{ marginBottom: 16 }}>Check 2FA Status</button>
      {twoFAStatus && (
        <div style={{ marginBottom: 16 }}>
          <strong>2FA Enabled:</strong> {twoFAStatus.TwoFactorEnabled ? 'Yes' : 'No'}<br />
          {twoFAStatus.TwoFactorSetupDate && <span><strong>Setup Date:</strong> {new Date(twoFAStatus.TwoFactorSetupDate).toLocaleString()}</span>}
        </div>
      )}
      {message && <div style={{ margin: '12px 0', color: message.includes('error') || message.includes('fail') ? 'red' : 'green' }}>{message}</div>}
      {!twoFAStatus?.TwoFactorEnabled && !qrCodeUrl && (
        <button onClick={startSetup} disabled={loading}>Start 2FA Setup</button>
      )}
      {qrCodeUrl && (
        <div style={{ margin: '24px 0' }}>
          <div><strong>Step 1:</strong> Scan this QR code with your authenticator app:</div>
          <img src={`https://api.qrserver.com/v1/create-qr-code/?data=${encodeURIComponent(qrCodeUrl)}&size=200x200`} alt="2FA QR Code" style={{ margin: '16px 0' }} />
          <div><strong>Or enter this secret manually:</strong> <span style={{ fontFamily: 'monospace', background: '#f5f5f5', padding: '2px 6px', borderRadius: 4 }}>{secret}</span></div>
          {showBackupCodes()}
          <form onSubmit={enable2FA} style={{ marginTop: 16 }}>
            <label htmlFor="totp">Step 2: Enter the 6-digit code from your app:</label><br />
            <input type="text" id="totp" value={totp} onChange={e => setTotp(e.target.value)} maxLength={6} style={{ margin: '8px 0', padding: 6, fontSize: 18, width: 120, textAlign: 'center', letterSpacing: 2 }} required pattern="\d{6}" />
            <button type="submit" disabled={loading}>Enable 2FA</button>
          </form>
        </div>
      )}
      {twoFAStatus?.TwoFactorEnabled && (
        <form onSubmit={disable2FA} style={{ marginTop: 24 }}>
          <label htmlFor="totp">Enter a 2FA code to disable:</label><br />
          <input type="text" id="totp" value={totp} onChange={e => setTotp(e.target.value)} maxLength={6} style={{ margin: '8px 0', padding: 6, fontSize: 18, width: 120, textAlign: 'center', letterSpacing: 2 }} required pattern="\d{6}" />
          <button type="submit" disabled={loading}>Disable 2FA</button>
        </form>
      )}
    </div>
  );
};

export default SettingsPage;
