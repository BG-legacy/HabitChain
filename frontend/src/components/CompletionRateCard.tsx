import React from 'react';
import { motion } from 'framer-motion';
import './CompletionRateCard.css';

interface CompletionRateCardProps {
  title: string;
  rate: number;
  subtitle?: string;
  color?: string;
  icon?: string;
  onClick?: () => void;
}

const CompletionRateCard: React.FC<CompletionRateCardProps> = ({
  title,
  rate,
  subtitle,
  color = '#667eea',
  icon = '📊',
  onClick
}) => {
  const getRateColor = (rate: number) => {
    if (rate >= 80) return '#10b981'; // Green for high rates
    if (rate >= 60) return '#f59e0b'; // Yellow for medium rates
    return '#ef4444'; // Red for low rates
  };

  const rateColor = getRateColor(rate);

  return (
    <motion.div
      className="completion-rate-card"
      style={{ cursor: onClick ? 'pointer' : 'default' }}
      onClick={onClick}
      whileHover={onClick ? { scale: 1.02, y: -2 } : {}}
      whileTap={onClick ? { scale: 0.98 } : {}}
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
    >
      <div className="card-header">
        <div className="card-icon" style={{ backgroundColor: color }}>
          {icon}
        </div>
        <div className="card-title">
          <h3>{title}</h3>
          {subtitle && <p className="subtitle">{subtitle}</p>}
        </div>
      </div>
      
      <div className="rate-display">
        <div className="rate-circle" style={{ borderColor: rateColor }}>
          <span className="rate-value" style={{ color: rateColor }}>
            {rate.toFixed(1)}%
          </span>
        </div>
      </div>
      
      <div className="rate-bar">
        <div 
          className="rate-fill" 
          style={{ 
            width: `${Math.min(rate, 100)}%`, 
            backgroundColor: rateColor 
          }}
        />
      </div>
    </motion.div>
  );
};

export default CompletionRateCard; 