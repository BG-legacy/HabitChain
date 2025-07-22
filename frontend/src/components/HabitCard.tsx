import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { HabitsService } from '../services/habitsService';
import { useDashboard } from '../contexts/DashboardContext';
import './HabitCard.css';

interface HabitCardProps {
  habit: {
    id: string;
    name: string;
    description: string;
    frequency: string;
    targetDays: number;
    currentStreak: number;
    longestStreak: number;
    totalCheckIns: number;
    completionRate?: number;
    weeklyCompletionRate?: number;
    monthlyCompletionRate?: number;
    totalPossibleCompletions?: number;
    totalActualCompletions?: number;
    createdAt: string;
    isActive: boolean;
  };
  onToggleActive?: (id: string) => void;
  onDelete?: (id: string) => void;
  onEdit?: (id: string) => void;
  showActions?: boolean;
  className?: string;
}

const HabitCard: React.FC<HabitCardProps> = ({
  habit,
  onToggleActive,
  onDelete,
  onEdit,
  showActions = true,
  className = ''
}) => {
  const [completing, setCompleting] = useState(false);
  const { refreshDashboard } = useDashboard();

  const getFrequencyIcon = (frequency: string) => {
    switch (frequency.toLowerCase()) {
      case 'daily': return '📅';
      case 'weekly': return '📊';
      case 'monthly': return '📈';
      default: return '🎯';
    }
  };

  const getStreakColor = (streak: number) => {
    if (streak >= 30) return '#28a745'; // Green for 30+ days
    if (streak >= 7) return '#ffc107'; // Yellow for 7+ days
    if (streak >= 3) return '#fd7e14'; // Orange for 3+ days
    return '#6c757d'; // Gray for less than 3 days
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const handleComplete = async () => {
    setCompleting(true);
    try {
      await HabitsService.completeHabit(habit.id);
      await refreshDashboard();
    } catch (err) {
      // Optionally show a toast or error
      console.error('Failed to complete habit', err);
    } finally {
      setCompleting(false);
    }
  };

  return (
    <div className={`habit-card ${habit.isActive ? 'active' : 'inactive'} ${className}`}>
      <div className="habit-header">
        <div className="habit-icon">
          {getFrequencyIcon(habit.frequency)}
        </div>
        <div className="habit-status">
          {showActions && (
            <>
              <button 
                className="action-btn"
                onClick={() => onToggleActive?.(habit.id)}
                title={habit.isActive ? 'Pause habit' : 'Activate habit'}
              >
                {habit.isActive ? '⏸️' : '▶️'}
              </button>
              <button 
                className="action-btn"
                onClick={() => onEdit?.(habit.id)}
                title="Edit habit"
              >
                ✏️
              </button>
              <button 
                className="action-btn"
                onClick={() => onDelete?.(habit.id)}
                title="Delete habit"
              >
                🗑️
              </button>
            </>
          )}
          <span className={`status-badge ${habit.isActive ? 'active' : 'inactive'}`}>
            {habit.isActive ? 'Active' : 'Inactive'}
          </span>
        </div>
      </div>

      <div className="habit-content">
        <h3 className="habit-name">{habit.name}</h3>
        {habit.description && <p className="habit-description">{habit.description}</p>}
        
        <div className="habit-details">
          <div className="detail-item">
            <span className="detail-label">Frequency:</span>
            <span className="detail-value">{habit.frequency}</span>
          </div>
          <div className="detail-item">
            <span className="detail-label">Target:</span>
            <span className="detail-value">{habit.targetDays} day{habit.targetDays !== 1 ? 's' : ''}</span>
          </div>
        </div>

        <div className="habit-stats">
          <div className="stat-item">
            <div className="stat-value" style={{ color: getStreakColor(habit.currentStreak) }}>
              🔥 {habit.currentStreak}
            </div>
            <div className="stat-label">Current Streak</div>
          </div>
          <div className="stat-item">
            <div className="stat-value">
              🏆 {habit.longestStreak}
            </div>
            <div className="stat-label">Longest Streak</div>
          </div>
          <div className="stat-item">
            <div className="stat-value">
              ✅ {habit.totalCheckIns}
            </div>
            <div className="stat-label">Total Check-ins</div>
          </div>
        </div>

        <div className="habit-meta">
          <span className="created-date">Created: {formatDate(habit.createdAt)}</span>
        </div>
      </div>

      {showActions && (
        <div className="habit-actions">
          <button 
            className="action-btn success"
            onClick={handleComplete}
            disabled={completing}
          >
            {completing ? '⏳ Completing...' : '🎯 Complete'}
          </button>
          <Link to={`/habits/${habit.id}`} className="action-btn primary">
            📊 View Details
          </Link>
        </div>
      )}
    </div>
  );
};

export default HabitCard; 