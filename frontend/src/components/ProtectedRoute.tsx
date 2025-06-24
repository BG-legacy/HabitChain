import React, { useEffect } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './RouteProtection.css';

interface ProtectedRouteProps {
  children: React.ReactNode;
  redirectTo?: string;
  requireActiveUser?: boolean;
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ 
  children, 
  redirectTo = '/login',
  requireActiveUser = true
}) => {
  const { isAuthenticated, isLoading, isTokenExpired, user, checkTokenExpiration } = useAuth();
  const location = useLocation();

  // Check token expiration on mount and when component updates
  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      checkTokenExpiration();
    }
  }, [isLoading, isAuthenticated, checkTokenExpiration]);

  if (isLoading) {
    // Show loading spinner while checking authentication
    return (
      <div className="route-loading-container">
        <div className="route-loading-spinner">
          <div className="spinner"></div>
          <p>Checking authentication...</p>
        </div>
      </div>
    );
  }

  // Check if user is authenticated and token is not expired
  if (!isAuthenticated || isTokenExpired) {
    // Redirect to login page, saving the attempted location
    return <Navigate to={redirectTo} state={{ from: location }} replace />;
  }

  // Check if user account is active (if required)
  if (requireActiveUser && user && !user.isActive) {
    return (
      <div className="route-error-container">
        <div className="route-error-card">
          <h2>Account Suspended</h2>
          <p>Your account has been deactivated. Please contact support for assistance.</p>
          <button 
            onClick={() => window.location.href = '/login'}
            className="route-error-button"
          >
            Return to Login
          </button>
        </div>
      </div>
    );
  }

  return <>{children}</>;
};

export default ProtectedRoute; 