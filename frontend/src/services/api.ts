import axios, { AxiosInstance, AxiosResponse, AxiosError } from 'axios';
import { toast } from 'react-hot-toast';
import { getApiUrl, config } from '../config/environment';

// Types
export interface ApiResponse<T = any> {
  data: T;
  message?: string;
  success: boolean;
}

export interface ApiError {
  message: string;
  status: number;
  errors?: Record<string, string[]>;
}

// Create axios instance
const api: AxiosInstance = axios.create({
  baseURL: getApiUrl(),
  timeout: config.apiTimeout,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add auth token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor to handle errors and token refresh
api.interceptors.response.use(
  (response: AxiosResponse) => {
    return response;
  },
  async (error: AxiosError<ApiError>) => {
    const originalRequest = error.config as any;

    // Handle 401 Unauthorized errors
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = localStorage.getItem('refreshToken');
        if (refreshToken) {
          // Attempt to refresh the token
          const refreshResponse = await axios.post(`${getApiUrl()}/auth/refresh`, {
            refreshToken,
          });

          const { token, refreshToken: newRefreshToken } = refreshResponse.data;
          
          // Update stored tokens
          localStorage.setItem('accessToken', token);
          localStorage.setItem('refreshToken', newRefreshToken);

          // Retry the original request with new token
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return api(originalRequest);
        }
      } catch (refreshError) {
        // Refresh failed, clear auth and redirect to login
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
        
        // Show error message
        toast.error('Session expired. Please log in again.');
        
        // Redirect to login page
        window.location.href = '/login';
        return Promise.reject(error);
      }
    }

    // Handle other errors
    const errorMessage = error.response?.data?.message || error.message || 'An error occurred';
    
    // Show error toast for non-401 errors
    if (error.response?.status !== 401) {
      toast.error(errorMessage);
    }

    return Promise.reject(error);
  }
);

// API Service class
export class ApiService {
  // Generic GET request
  static async get<T>(url: string, params?: any): Promise<T> {
    const response = await api.get<T>(url, { params });
    return response.data;
  }

  // Generic POST request
  static async post<T>(url: string, data?: any): Promise<T> {
    console.log('ApiService.post - URL:', url);
    console.log('ApiService.post - Data:', data);
    const response = await api.post<T>(url, data);
    return response.data;
  }

  // Generic PUT request
  static async put<T>(url: string, data?: any): Promise<T> {
    const response = await api.put<T>(url, data);
    return response.data;
  }

  // Generic DELETE request
  static async delete<T>(url: string): Promise<T> {
    const response = await api.delete<T>(url);
    return response.data;
  }

  // Generic PATCH request
  static async patch<T>(url: string, data?: any): Promise<T> {
    const response = await api.patch<T>(url, data);
    return response.data;
  }
}

// Export the axios instance for direct use if needed
export default api; 