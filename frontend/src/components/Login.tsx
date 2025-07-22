import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useToast } from '../hooks/useToast';
import './AuthForms.css';

const Login: React.FC = () => {
  const [formData, setFormData] = useState({
    email: '',
    password: ''
  });
  const [errors, setErrors] = useState<{ [key: string]: string }>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();
  const { showSuccess, showError, showLoading, dismiss } = useToast();

  const validateForm = () => {
    const newErrors: { [key: string]: string } = {};

    // Email validation
    if (!formData.email) {
      newErrors.email = 'Email is required';
    } else if (!/\S+@\S+\.\S+/.test(formData.email)) {
      newErrors.email = 'Please enter a valid email address';
    }

    // Password validation
    if (!formData.password) {
      newErrors.password = 'Password is required';
    } else if (formData.password.length < 6) {
      newErrors.password = 'Password must be at least 6 characters long';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));

    // Clear error when user starts typing
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    setIsSubmitting(true);
    const loadingToast = showLoading('⚡ Signing you in instantly...');

    try {
      console.log('🔐 Starting login process...');
      console.log('📧 Email:', formData.email);
      
      await login(formData);
      
      console.log('✅ Login successful, checking auth state...');
      console.log('🏪 Access Token in localStorage:', !!localStorage.getItem('accessToken'));
      console.log('🔄 Refresh Token in localStorage:', !!localStorage.getItem('refreshToken'));
      console.log('👤 User in localStorage:', !!localStorage.getItem('user'));
      
      // Add a small delay to ensure state is updated
      setTimeout(() => {
        console.log('🚀 Attempting navigation to dashboard...');
        dismiss(loadingToast);
        showSuccess('🎉 Welcome back! Signed in instantly!');
        navigate('/dashboard');
      }, 100);
      
    } catch (error: any) {
      console.error('❌ Login failed:', error);
      dismiss(loadingToast);
      
      // Enhanced error handling with instant feedback
      let errorMessage = '⚡ Please try again!';
      
      if (error.code === 'ECONNABORTED' || error.message?.includes('timeout')) {
        errorMessage = '⏱️ Network timeout - server optimizing. Quick retry!';
      } else if (error.response?.status === 401) {
        errorMessage = '🔑 Invalid email or password. Please check your credentials.';
      } else if (error.response?.status === 400) {
        errorMessage = error.response?.data?.message || '📝 Please check your login information.';
      } else if (error.response?.status >= 500) {
        errorMessage = '🔧 Server optimization in progress. Quick retry!';
      } else if (error.message) {
        errorMessage = `💡 ${error.message}`;
      }
      
      showError(errorMessage);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <div className="auth-header">
          <h1>Welcome Back</h1>
          <p>Sign in to your HabitChain account</p>
        </div>

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              type="email"
              id="email"
              name="email"
              value={formData.email}
              onChange={handleInputChange}
              className={errors.email ? 'error' : ''}
              placeholder="Enter your email"
              disabled={isSubmitting}
            />
            {errors.email && <span className="error-text">{errors.email}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              type="password"
              id="password"
              name="password"
              value={formData.password}
              onChange={handleInputChange}
              className={errors.password ? 'error' : ''}
              placeholder="Enter your password"
              disabled={isSubmitting}
            />
            {errors.password && <span className="error-text">{errors.password}</span>}
          </div>

          <button
            type="submit"
            className="auth-button"
            disabled={isSubmitting}
          >
            {isSubmitting ? '⚡ Signing In...' : '🚀 Sign In Instantly'}
          </button>
        </form>

        <div className="auth-footer">
          <p>
            Don't have an account?{' '}
            <Link to="/register" className="auth-link">
              Sign up here
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
};

export default Login; 