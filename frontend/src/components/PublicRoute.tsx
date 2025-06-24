import React, { useEffect } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './RouteProtection.css';

interface PublicRouteProps {
  children: React.ReactNode;
  redirectTo?: string;
}

export const PublicRoute: React.FC<PublicRouteProps> = ({ 
  children, 
  redirectTo = '/dashboard' 
}) => {
  const { isAuthenticated, isLoading, isTokenExpired, checkTokenExpiration } = useAuth();
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
  if (isAuthenticated && !isTokenExpired) {
    // Redirect authenticated users to the specified route
    // Use the from location if available, otherwise use redirectTo
    const from = location.state?.from?.pathname || redirectTo;
    return <Navigate to={from} replace />;
  }

  return <>{children}</>;
};

export default PublicRoute; 