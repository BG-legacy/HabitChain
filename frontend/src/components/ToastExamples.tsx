import React from 'react';
import { toast } from 'react-hot-toast';
import { useToast } from '../hooks/useToast';

const ToastExamples: React.FC = () => {
  const { showSuccess, showError, showLoading, showCustom, dismiss } = useToast();

  const handleSuccessToast = () => {
    showSuccess('Operation completed successfully!');
  };

  const handleErrorToast = () => {
    showError('Something went wrong. Please try again.');
  };

  const handleLoadingToast = () => {
    const loadingToast = showLoading('Processing your request...');
    
    // Simulate an async operation
    setTimeout(() => {
      dismiss(loadingToast);
      showSuccess('Request completed!');
    }, 3000);
  };

  const handleCustomToast = () => {
    showCustom('This is a custom toast message!', {
      icon: '🎉',
      duration: 6000,
    });
  };

  const handlePromiseToast = () => {
    const promise = new Promise((resolve, reject) => {
      setTimeout(() => {
        Math.random() > 0.5 ? resolve('Success!') : reject('Failed!');
      }, 2000);
    });

    toast.promise(
      promise,
      {
        loading: 'Processing...',
        success: 'Operation completed successfully!',
        error: 'Operation failed. Please try again.',
      }
    );
  };

  return (
    <div className="main-container">
      <div className="page-header">
        <h1 className="page-title">Toast Notifications</h1>
        <p className="page-subtitle">Examples of different toast notification types</p>
      </div>
      
      <div className="glass-card p-lg">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <button
            onClick={handleSuccessToast}
            className="btn btn-primary"
          >
            Success Toast
          </button>
          
          <button
            onClick={handleErrorToast}
            className="btn btn-danger"
          >
            Error Toast
          </button>
          
          <button
            onClick={handleLoadingToast}
            className="btn btn-secondary"
          >
            Loading Toast
          </button>
          
          <button
            onClick={handleCustomToast}
            className="btn btn-accent"
          >
            Custom Toast
          </button>
          
          <button
            onClick={handlePromiseToast}
            className="btn btn-info"
          >
            Promise Toast
          </button>
          
          <button
            onClick={() => dismiss()}
            className="btn btn-warning"
          >
            Dismiss All
          </button>
        </div>
        
        <div className="mt-8 p-4 bg-gray-50 rounded-lg">
          <h3 className="text-lg font-semibold mb-2">Usage Examples:</h3>
          <div className="space-y-2 text-sm">
            <p><strong>Success:</strong> Use for successful operations, form submissions, etc.</p>
            <p><strong>Error:</strong> Use for error messages, validation failures, etc.</p>
            <p><strong>Loading:</strong> Use during async operations, API calls, etc.</p>
            <p><strong>Custom:</strong> Use for custom messages with specific styling or icons.</p>
            <p><strong>Promise:</strong> Use for operations that return promises (loading → success/error).</p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ToastExamples; 