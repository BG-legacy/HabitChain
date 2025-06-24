import React, { createContext, useContext, useEffect, useState, ReactNode, useCallback } from 'react';
import AuthService, { 
  User, 
  AuthResponse, 
  LoginCredentials, 
  RegisterCredentials, 
  ChangePasswordData 
} from '../services/authService';

// Types
interface AuthContextType {
  user: User | null;
  token: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  isTokenExpired: boolean;
  login: (credentials: LoginCredentials) => Promise<void>;
  register: (credentials: RegisterCredentials) => Promise<void>;
  logout: () => Promise<void>;
  refreshAuth: () => Promise<void>;
  changePassword: (data: ChangePasswordData) => Promise<void>;
  clearAuth: () => void;
  checkTokenExpiration: () => boolean;
}

// Create the context
const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Token expiration check utility
const checkTokenExpired = (token: string): boolean => {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const expirationTime = payload.exp * 1000; // Convert to milliseconds
    const currentTime = Date.now();
    return currentTime >= expirationTime;
  } catch (error) {
    return true; // If we can't parse the token, consider it expired
  }
};

// Provider component
interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [refreshToken, setRefreshToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isTokenExpired, setIsTokenExpired] = useState(false);

  const clearAuth = useCallback((): void => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    setToken(null);
    setRefreshToken(null);
    setUser(null);
    setIsTokenExpired(false);
  }, []);

  const checkTokenExpiration = useCallback((): boolean => {
    const currentToken = localStorage.getItem('accessToken');
    if (!currentToken) {
      setIsTokenExpired(true);
      return true;
    }

    const expired = checkTokenExpired(currentToken);
    setIsTokenExpired(expired);
    return expired;
  }, []);

  const refreshAuth = useCallback(async (): Promise<void> => {
    try {
      const currentRefreshToken = localStorage.getItem('refreshToken');
      if (!currentRefreshToken) {
        throw new Error('No refresh token available');
      }

      const response: AuthResponse = await AuthService.refreshToken(currentRefreshToken);
      
      // Update stored tokens and user data
      localStorage.setItem('accessToken', response.token);
      localStorage.setItem('refreshToken', response.refreshToken);
      localStorage.setItem('user', JSON.stringify(response.user));
      
      // Update state
      setToken(response.token);
      setRefreshToken(response.refreshToken);
      setUser(response.user);
      setIsTokenExpired(false);
    } catch (error) {
      console.error('Failed to refresh token:', error);
      clearAuth();
      throw error;
    }
  }, [clearAuth]);

  // Initialize authentication state
  const initializeAuth = async () => {
    try {
      const storedToken = localStorage.getItem('accessToken');
      const storedRefreshToken = localStorage.getItem('refreshToken');
      const storedUser = localStorage.getItem('user');

      if (storedToken && storedRefreshToken && storedUser) {
        // Check if token is expired
        if (checkTokenExpired(storedToken)) {
          // Token is expired, try to refresh
          try {
            await refreshAuth();
          } catch (error) {
            // Refresh failed, clear auth
            clearAuth();
          }
        } else {
          // Token is valid, set state
          setToken(storedToken);
          setRefreshToken(storedRefreshToken);
          setUser(JSON.parse(storedUser));
          setIsTokenExpired(false);
        }
      }
    } catch (error) {
      console.error('Error initializing auth:', error);
      clearAuth();
    } finally {
      setIsLoading(false);
    }
  };

  // Check token expiration periodically
  useEffect(() => {
    const checkExpiration = () => {
      checkTokenExpiration();
    };

    // Check immediately
    checkExpiration();

    // Check every minute
    const interval = setInterval(checkExpiration, 60000);

    return () => clearInterval(interval);
  }, [checkTokenExpiration]);

  // Initialize auth on mount
  useEffect(() => {
    initializeAuth();
  }, []);

  const login = async (credentials: LoginCredentials): Promise<void> => {
    try {
      const response: AuthResponse = await AuthService.login(credentials);
      
      // Store tokens and user data
      localStorage.setItem('accessToken', response.token);
      localStorage.setItem('refreshToken', response.refreshToken);
      localStorage.setItem('user', JSON.stringify(response.user));
      
      // Update state
      setToken(response.token);
      setRefreshToken(response.refreshToken);
      setUser(response.user);
      setIsTokenExpired(false);
    } catch (error) {
      console.error('Login failed:', error);
      throw error;
    }
  };

  const register = async (credentials: RegisterCredentials): Promise<void> => {
    try {
      const response: AuthResponse = await AuthService.register(credentials);
      
      // Store tokens and user data
      localStorage.setItem('accessToken', response.token);
      localStorage.setItem('refreshToken', response.refreshToken);
      localStorage.setItem('user', JSON.stringify(response.user));
      
      // Update state
      setToken(response.token);
      setRefreshToken(response.refreshToken);
      setUser(response.user);
      setIsTokenExpired(false);
    } catch (error) {
      console.error('Registration failed:', error);
      throw error;
    }
  };

  const logout = async (): Promise<void> => {
    try {
      const currentRefreshToken = localStorage.getItem('refreshToken');
      if (currentRefreshToken) {
        await AuthService.revokeToken(currentRefreshToken);
      }
    } catch (error) {
      console.error('Error revoking token:', error);
    } finally {
      clearAuth();
    }
  };

  const changePassword = async (data: ChangePasswordData): Promise<void> => {
    try {
      await AuthService.changePassword(data);
    } catch (error) {
      console.error('Password change failed:', error);
      throw error;
    }
  };

  const value: AuthContextType = {
    user,
    token,
    refreshToken,
    isAuthenticated: !!user && !!token && !isTokenExpired,
    isLoading,
    isTokenExpired,
    login,
    register,
    logout,
    refreshAuth,
    changePassword,
    clearAuth,
    checkTokenExpiration,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}; 