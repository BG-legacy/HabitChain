import React, { Component, ErrorInfo, ReactNode } from 'react';
import { motion } from 'framer-motion';

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error?: Error;
  errorInfo?: ErrorInfo;
}

class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('ErrorBoundary caught an error:', error, errorInfo);
    this.setState({ error, errorInfo });
  }

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback;
      }

      return (
        <motion.div
          initial={{ opacity: 0, scale: 0.9 }}
          animate={{ opacity: 1, scale: 1 }}
          className="error-boundary"
          style={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            minHeight: '100vh',
            padding: '2rem',
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            color: 'white',
            textAlign: 'center',
          }}
        >
          <motion.div
            initial={{ rotate: 0 }}
            animate={{ rotate: 360 }}
            transition={{ duration: 2, repeat: Infinity, ease: 'linear' }}
            style={{
              width: '80px',
              height: '80px',
              border: '4px solid rgba(255, 255, 255, 0.3)',
              borderTop: '4px solid white',
              borderRadius: '50%',
              marginBottom: '2rem',
            }}
          />
          
          <motion.h1
            initial={{ y: -20, opacity: 0 }}
            animate={{ y: 0, opacity: 1 }}
            transition={{ delay: 0.2 }}
            style={{
              fontSize: '2.5rem',
              marginBottom: '1rem',
              fontWeight: 'bold',
            }}
          >
            Oops! Something went wrong
          </motion.h1>
          
          <motion.p
            initial={{ y: -20, opacity: 0 }}
            animate={{ y: 0, opacity: 1 }}
            transition={{ delay: 0.4 }}
            style={{
              fontSize: '1.1rem',
              marginBottom: '2rem',
              maxWidth: '600px',
              lineHeight: '1.6',
            }}
          >
            We're sorry, but something unexpected happened. Our team has been notified and is working to fix the issue.
          </motion.p>
          
          <motion.button
            initial={{ y: 20, opacity: 0 }}
            animate={{ y: 0, opacity: 1 }}
            transition={{ delay: 0.6 }}
            onClick={() => window.location.reload()}
            style={{
              background: 'rgba(255, 255, 255, 0.2)',
              border: '2px solid white',
              color: 'white',
              padding: '12px 24px',
              borderRadius: '8px',
              fontSize: '1rem',
              cursor: 'pointer',
              transition: 'all 0.3s ease',
            }}
            whileHover={{ scale: 1.05, background: 'rgba(255, 255, 255, 0.3)' }}
            whileTap={{ scale: 0.95 }}
          >
            Refresh Page
          </motion.button>
          
          {process.env.NODE_ENV === 'development' && this.state.error && (
            <motion.details
              initial={{ y: 20, opacity: 0 }}
              animate={{ y: 0, opacity: 1 }}
              transition={{ delay: 0.8 }}
              style={{
                marginTop: '2rem',
                textAlign: 'left',
                maxWidth: '800px',
                background: 'rgba(0, 0, 0, 0.2)',
                padding: '1rem',
                borderRadius: '8px',
              }}
            >
              <summary style={{ cursor: 'pointer', marginBottom: '1rem' }}>
                Error Details (Development)
              </summary>
              <pre style={{ fontSize: '0.9rem', overflow: 'auto' }}>
                {this.state.error.toString()}
                {this.state.errorInfo?.componentStack}
              </pre>
            </motion.details>
          )}
        </motion.div>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary; 