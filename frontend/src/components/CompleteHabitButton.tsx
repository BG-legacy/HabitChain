import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { HabitsService } from '../services/habitsService';
import { useDashboard } from '../contexts/DashboardContext';
import { useToast } from '../hooks/useToast';
import './CompleteHabitButton.css';

interface CompleteHabitButtonProps {
  habitId: string;
  habitName: string;
  isCompleted?: boolean;
  onComplete?: () => void;
  className?: string;
  size?: 'small' | 'medium' | 'large';
  variant?: 'default' | 'minimal' | 'outlined';
}

const CompleteHabitButton: React.FC<CompleteHabitButtonProps> = ({
  habitId,
  habitName,
  isCompleted = false,
  onComplete,
  className = '',
  size = 'medium',
  variant = 'default'
}) => {
  const [isCompleting, setIsCompleting] = useState(false);
  const [isCompletedToday, setIsCompletedToday] = useState(isCompleted);
  const { refreshDashboard } = useDashboard();
  const { showSuccess, showError } = useToast();

  // Check if habit is already completed today
  useEffect(() => {
    const checkCompletionStatus = async () => {
      try {
        const completedToday = await HabitsService.isHabitCompletedToday(habitId);
        setIsCompletedToday(completedToday);
      } catch (error) {
        console.error('Failed to check completion status:', error);
        setIsCompletedToday(isCompleted);
      }
    };

    checkCompletionStatus();
  }, [habitId, isCompleted]);

  const handleComplete = async () => {
    if (isCompletedToday || isCompleting) return;

    setIsCompleting(true);
    try {
      await HabitsService.completeHabit(habitId);
      setIsCompletedToday(true);
      showSuccess(`🎉 Completed "${habitName}"!`);
      
      // Refresh dashboard state
      await refreshDashboard();
      
      // Call custom onComplete callback if provided
      if (onComplete) {
        onComplete();
      }
    } catch (error) {
      console.error('Failed to complete habit:', error);
      showError('Failed to complete habit. Please try again.');
    } finally {
      setIsCompleting(false);
    }
  };

  const getButtonText = () => {
    if (isCompleting) return 'Completing...';
    if (isCompletedToday) return 'Completed Today';
    return 'Complete';
  };

  const getButtonIcon = () => {
    if (isCompleting) return '⏳';
    if (isCompletedToday) return '✅';
    return '🎯';
  };

  return (
    <motion.button
      className={`complete-habit-btn ${size} ${variant} ${isCompletedToday ? 'completed' : ''} ${className}`}
      onClick={handleComplete}
      disabled={isCompletedToday || isCompleting}
      whileHover={!isCompletedToday && !isCompleting ? { scale: 1.05 } : {}}
      whileTap={!isCompletedToday && !isCompleting ? { scale: 0.95 } : {}}
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.2 }}
    >
      <span className="btn-icon">{getButtonIcon()}</span>
      <span className="btn-text">{getButtonText()}</span>
    </motion.button>
  );
};

export default CompleteHabitButton; 