import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useToast } from '../hooks/useToast';
import './AuthForms.css';

const Register: React.FC = () => {
  const [formData, setFormData] = useState({
    email: '',
    username: '',
    firstName: '',
    lastName: '',
    password: '',
    confirmPassword: ''
  });
  const [errors, setErrors] = useState<{ [key: string]: string }>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { register } = useAuth();
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

    // Username validation
    if (!formData.username) {
      newErrors.username = 'Username is required';
    } else if (formData.username.length < 3) {
      newErrors.username = 'Username must be at least 3 characters long';
    } else if (formData.username.length > 50) {
      newErrors.username = 'Username must be less than 50 characters';
    } else if (!/^[a-zA-Z0-9_]+$/.test(formData.username)) {
      newErrors.username = 'Username can only contain letters, numbers, and underscores';
    }

    // First name validation
    if (!formData.firstName) {
      newErrors.firstName = 'First name is required';
    } else if (formData.firstName.length > 100) {
      newErrors.firstName = 'First name must be less than 100 characters';
    }

    // Last name validation
    if (!formData.lastName) {
      newErrors.lastName = 'Last name is required';
    } else if (formData.lastName.length > 100) {
      newErrors.lastName = 'Last name must be less than 100 characters';
    }

    // Password validation
    if (!formData.password) {
      newErrors.password = 'Password is required';
    } else if (formData.password.length < 6) {
      newErrors.password = 'Password must be at least 6 characters long';
    } else if (formData.password.length > 100) {
      newErrors.password = 'Password must be less than 100 characters';
    } else {
      // Check for required character types
      const hasLowercase = /[a-z]/.test(formData.password);
      const hasUppercase = /[A-Z]/.test(formData.password);
      const hasDigit = /\d/.test(formData.password);
      
      if (!hasLowercase) {
        newErrors.password = 'Password must contain at least one lowercase letter';
      } else if (!hasUppercase) {
        newErrors.password = 'Password must contain at least one uppercase letter';
      } else if (!hasDigit) {
        newErrors.password = 'Password must contain at least one number';
      }
    }

    // Confirm password validation
    if (!formData.confirmPassword) {
      newErrors.confirmPassword = 'Please confirm your password';
    } else if (formData.password !== formData.confirmPassword) {
      newErrors.confirmPassword = 'Passwords do not match';
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
    const loadingToast = showLoading('✨ Creating your account instantly...');

    try {
      // Optimistic UI - show immediate feedback
      const quickSuccessToast = showLoading('🚀 Processing...');
      
      await register(formData);
      
      dismiss(loadingToast);
      dismiss(quickSuccessToast);
      showSuccess('🎉 Welcome to HabitChain! Account created instantly!');
      
      // Immediate navigation for instant feel
      navigate('/dashboard');
    } catch (error: any) {
      dismiss(loadingToast);
      
      // Enhanced error handling with instant feedback
      let errorMessage = '⚡ Quick retry recommended!';
      
      if (error.code === 'ECONNABORTED' || error.message?.includes('timeout')) {
        errorMessage = '⏱️ Network timeout - the server is optimizing. Try again!';
      } else if (error.response?.status === 400) {
        errorMessage = error.response?.data?.message || '📝 Please check your information and try again.';
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
          <h1>Create Account</h1>
          <p>Join HabitChain and start building better habits</p>
        </div>

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="firstName">First Name</label>
              <input
                type="text"
                id="firstName"
                name="firstName"
                value={formData.firstName}
                onChange={handleInputChange}
                className={errors.firstName ? 'error' : ''}
                placeholder="Enter your first name"
                disabled={isSubmitting}
              />
              {errors.firstName && <span className="error-text">{errors.firstName}</span>}
            </div>

            <div className="form-group">
              <label htmlFor="lastName">Last Name</label>
              <input
                type="text"
                id="lastName"
                name="lastName"
                value={formData.lastName}
                onChange={handleInputChange}
                className={errors.lastName ? 'error' : ''}
                placeholder="Enter your last name"
                disabled={isSubmitting}
              />
              {errors.lastName && <span className="error-text">{errors.lastName}</span>}
            </div>
          </div>

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
            <label htmlFor="username">Username</label>
            <input
              type="text"
              id="username"
              name="username"
              value={formData.username}
              onChange={handleInputChange}
              className={errors.username ? 'error' : ''}
              placeholder="Choose a username"
              disabled={isSubmitting}
            />
            {errors.username && <span className="error-text">{errors.username}</span>}
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
              placeholder="Create a password"
              disabled={isSubmitting}
            />
            {errors.password && <span className="error-text">{errors.password}</span>}
            <div className="password-requirements">
              <small>Password must contain:</small>
              <ul>
                <li className={formData.password.length >= 6 ? 'valid' : ''}>At least 6 characters</li>
                <li className={/[a-z]/.test(formData.password) ? 'valid' : ''}>One lowercase letter</li>
                <li className={/[A-Z]/.test(formData.password) ? 'valid' : ''}>One uppercase letter</li>
                <li className={/\d/.test(formData.password) ? 'valid' : ''}>One number</li>
              </ul>
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="confirmPassword">Confirm Password</label>
            <input
              type="password"
              id="confirmPassword"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleInputChange}
              className={errors.confirmPassword ? 'error' : ''}
              placeholder="Confirm your password"
              disabled={isSubmitting}
            />
            {errors.confirmPassword && <span className="error-text">{errors.confirmPassword}</span>}
          </div>

          <button
            type="submit"
            className="auth-button"
            disabled={isSubmitting}
          >
            {isSubmitting ? '⚡ Creating Instantly...' : '🚀 Create Account Instantly'}
          </button>
        </form>

        <div className="auth-footer">
          <p>
            Already have an account?{' '}
            <Link to="/login" className="auth-link">
              Sign in here
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
};

export default Register; 