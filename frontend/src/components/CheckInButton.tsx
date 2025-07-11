import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { HabitsService } from '../services/habitsService';
import { useDashboard } from '../contexts/DashboardContext';
import { useToast } from '../hooks/useToast';
import './CheckInButton.css';

interface CheckInButtonProps {
  habitId: string;
  habitName: string;
  isCompleted?: boolean;
  onCheckIn?: () => void;
  className?: string;
  size?: 'small' | 'medium' | 'large';
  variant?: 'default' | 'minimal' | 'outlined' | 'detailed';
  showModal?: boolean;
}

const CheckInButton: React.FC<CheckInButtonProps> = ({
  habitId,
  habitName,
  isCompleted = false,
  onCheckIn,
  className = '',
  size = 'medium',
  variant = 'default',
  showModal = false
}) => {
  const navigate = useNavigate();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showQuickModal, setShowQuickModal] = useState(false);
  const [notes, setNotes] = useState('');
  const [completedAt, setCompletedAt] = useState(new Date().toISOString().slice(0, 16));
  const { refreshDashboard } = useDashboard();
  const { showSuccess, showError } = useToast();

  const handleQuickCheckIn = async () => {
    if (isCompleted || isSubmitting) return;

    if (variant === 'detailed' || showModal) {
      setShowQuickModal(true);
      return;
    }

    // Navigate to full check-in page
    console.log('Navigating to check-in page with habit ID:', habitId);
    navigate(`/check-in?habit=${habitId}`);
  };

  const handleModalSubmit = async () => {
    setIsSubmitting(true);
    try {
      await HabitsService.createCheckIn(
        habitId, 
        notes.trim(), 
        new Date(completedAt).toISOString()
      );
      showSuccess(`✅ Check-in recorded for "${habitName}"`);
      
      // Refresh dashboard state
      await refreshDashboard();
      
      // Call custom callback if provided
      if (onCheckIn) {
        onCheckIn();
      }

      // Close modal and reset form
      setShowQuickModal(false);
      setNotes('');
      setCompletedAt(new Date().toISOString().slice(0, 16));
    } catch (error) {
      console.error('Failed to create check-in:', error);
      showError('Failed to record check-in. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const getButtonText = () => {
    if (isSubmitting) return 'Recording...';
    if (isCompleted) return 'Checked In';
    
    switch (variant) {
      case 'detailed':
        return 'Quick Check-in';
      case 'minimal':
        return 'Check-in';
      default:
        return 'Check-in';
    }
  };

  const getButtonIcon = () => {
    if (isSubmitting) return '⏳';
    if (isCompleted) return '✅';
    
    switch (variant) {
      case 'detailed':
        return '📝';
      case 'minimal':
        return '✓';
      default:
        return '📋';
    }
  };

  return (
    <>
      <motion.button
        className={`checkin-btn ${size} ${variant} ${isCompleted ? 'completed' : ''} ${className}`}
        onClick={handleQuickCheckIn}
        disabled={isCompleted || isSubmitting}
        whileHover={!isCompleted && !isSubmitting ? { scale: 1.05 } : {}}
        whileTap={!isCompleted && !isSubmitting ? { scale: 0.95 } : {}}
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.2 }}
      >
        <span className="btn-icon">{getButtonIcon()}</span>
        <span className="btn-text">{getButtonText()}</span>
      </motion.button>

      {/* Quick Check-in Modal */}
      {showQuickModal && (
        <div className="checkin-modal-overlay">
          <motion.div 
            className="checkin-modal"
            initial={{ opacity: 0, scale: 0.8 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ duration: 0.2 }}
          >
            <div className="modal-header">
              <h3>Check-in: {habitName}</h3>
              <button 
                className="close-btn"
                onClick={() => setShowQuickModal(false)}
                disabled={isSubmitting}
              >
                ✕
              </button>
            </div>
            
            <div className="modal-body">
              <div className="form-group">
                <label htmlFor="completedAt">Completed At</label>
                <input
                  type="datetime-local"
                  id="completedAt"
                  value={completedAt}
                  onChange={(e) => setCompletedAt(e.target.value)}
                  max={new Date().toISOString().slice(0, 16)}
                  disabled={isSubmitting}
                />
              </div>
              
              <div className="form-group">
                <label htmlFor="notes">Notes (optional)</label>
                <textarea
                  id="notes"
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  placeholder="How did it go? Any thoughts or achievements?"
                  rows={3}
                  maxLength={500}
                  disabled={isSubmitting}
                />
                <div className="char-count">{notes.length}/500</div>
              </div>
            </div>
            
            <div className="modal-footer">
              <button 
                className="btn-secondary"
                onClick={() => setShowQuickModal(false)}
                disabled={isSubmitting}
              >
                Cancel
              </button>
              <button 
                className="btn-primary"
                onClick={handleModalSubmit}
                disabled={isSubmitting}
              >
                {isSubmitting ? 'Recording...' : 'Record Check-in'}
              </button>
            </div>
          </motion.div>
        </div>
      )}
    </>
  );
};

export default CheckInButton; 