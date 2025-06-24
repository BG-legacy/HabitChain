import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import './SessionExpiration.css';

interface SessionExpirationProps {
  warningMinutes?: number;
}

const SessionExpiration: React.FC<SessionExpirationProps> = ({ 
  warningMinutes = 5 
}) => {
  const { token, refreshAuth, logout, isAuthenticated } = useAuth();
  const [showWarning, setShowWarning] = useState(false);
  const [timeRemaining, setTimeRemaining] = useState(0);

  useEffect(() => {
    if (!isAuthenticated || !token) {
      setShowWarning(false);
      return;
    }

    const checkTokenExpiration = () => {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const expirationTime = payload.exp * 1000; // Convert to milliseconds
        const currentTime = Date.now();
        const timeUntilExpiration = expirationTime - currentTime;
        const warningTime = warningMinutes * 60 * 1000; // Convert to milliseconds

        if (timeUntilExpiration <= warningTime && timeUntilExpiration > 0) {
          setTimeRemaining(Math.ceil(timeUntilExpiration / 1000 / 60)); // Convert to minutes
          setShowWarning(true);
        } else {
          setShowWarning(false);
        }
      } catch (error) {
        // If we can't parse the token, don't show warning
        setShowWarning(false);
      }
    };

    // Check immediately
    checkTokenExpiration();

    // Check every 30 seconds
    const interval = setInterval(checkTokenExpiration, 30000);

    return () => clearInterval(interval);
  }, [token, isAuthenticated, warningMinutes]);

  const handleExtendSession = async () => {
    try {
      await refreshAuth();
      setShowWarning(false);
    } catch (error) {
      console.error('Failed to extend session:', error);
      // If refresh fails, logout
      await logout();
    }
  };

  const handleLogout = async () => {
    await logout();
  };

  if (!showWarning) {
    return null;
  }

  return (
    <div className="session-expiration-overlay">
      <div className="session-expiration-modal">
        <div className="session-expiration-header">
          <h3>Session Expiring Soon</h3>
          <div className="session-expiration-timer">
            {timeRemaining} minute{timeRemaining !== 1 ? 's' : ''} remaining
          </div>
        </div>
        
        <div className="session-expiration-content">
          <p>
            Your session will expire in {timeRemaining} minute{timeRemaining !== 1 ? 's' : ''}. 
            Would you like to extend your session or logout now?
          </p>
        </div>
        
        <div className="session-expiration-actions">
          <button 
            onClick={handleExtendSession}
            className="session-extend-button"
          >
            Extend Session
          </button>
          <button 
            onClick={handleLogout}
            className="session-logout-button"
          >
            Logout Now
          </button>
        </div>
      </div>
    </div>
  );
};

export default SessionExpiration; 