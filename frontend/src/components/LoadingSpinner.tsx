import React from 'react';
import { motion } from 'framer-motion';

interface LoadingSpinnerProps {
  size?: 'small' | 'medium' | 'large';
  color?: string;
  text?: string;
  fullScreen?: boolean;
  className?: string;
}

const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({
  size = 'medium',
  color = '#667eea',
  text,
  fullScreen = false,
  className = '',
}) => {
  const sizeMap = {
    small: 24,
    medium: 40,
    large: 60,
  };

  const spinnerSize = sizeMap[size];

  const spinner = (
    <motion.div
      className={`loading-spinner ${className}`}
      style={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        ...(fullScreen && {
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          backgroundColor: 'rgba(255, 255, 255, 0.9)',
          backdropFilter: 'blur(4px)',
          zIndex: 9999,
        }),
      }}
    >
      <motion.div
        style={{
          width: spinnerSize,
          height: spinnerSize,
          border: `3px solid ${color}20`,
          borderTop: `3px solid ${color}`,
          borderRadius: '50%',
          marginBottom: text ? '1rem' : 0,
        }}
        animate={{ rotate: 360 }}
        transition={{
          duration: 1,
          repeat: Infinity,
          ease: 'linear',
        }}
      />
      {text && (
        <motion.p
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.2 }}
          style={{
            color: '#666',
            fontSize: '0.9rem',
            margin: 0,
            textAlign: 'center',
          }}
        >
          {text}
        </motion.p>
      )}
    </motion.div>
  );

  return spinner;
};

export default LoadingSpinner; 