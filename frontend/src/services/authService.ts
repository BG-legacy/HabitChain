import { ApiService } from './api';

// Types
export interface User {
  id: string;
  email: string;
  username: string;
  firstName: string;
  lastName: string;
  profilePictureUrl?: string;
  lastLoginAt?: string;
  isActive: boolean;
  createdAt: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface RegisterCredentials {
  email: string;
  username: string;
  firstName: string;
  lastName: string;
  password: string;
  confirmPassword: string;
}

export interface ChangePasswordData {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

// Authentication Service
export class AuthService {
  // Login user
  static async login(credentials: LoginCredentials): Promise<AuthResponse> {
    return ApiService.post<AuthResponse>('/auth/login', credentials);
  }

  // Register new user
  static async register(credentials: RegisterCredentials): Promise<AuthResponse> {
    return ApiService.post<AuthResponse>('/auth/register', credentials);
  }

  // Refresh access token
  static async refreshToken(refreshToken: string): Promise<AuthResponse> {
    return ApiService.post<AuthResponse>('/auth/refresh', { refreshToken });
  }

  // Revoke refresh token (logout)
  static async revokeToken(refreshToken: string): Promise<{ message: string }> {
    return ApiService.post<{ message: string }>('/auth/revoke', { refreshToken });
  }

  // Change password
  static async changePassword(data: ChangePasswordData): Promise<{ message: string }> {
    return ApiService.post<{ message: string }>('/auth/change-password', data);
  }

  // Get current user information
  static async getCurrentUser(): Promise<User> {
    return ApiService.get<User>('/auth/me');
  }
}

export default AuthService; 