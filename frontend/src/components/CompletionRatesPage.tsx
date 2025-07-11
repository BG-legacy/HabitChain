import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { useAuth } from '../contexts/AuthContext';
import { HabitsService, UserCompletionRate } from '../services/habitsService';
import CompletionRateCard from './CompletionRateCard';
import './CompletionRatesPage.css';

const CompletionRatesPage: React.FC = () => {
  const { user } = useAuth();
  const [completionRates, setCompletionRates] = useState<UserCompletionRate | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchCompletionRates = async () => {
      if (!user?.id) return;
      
      try {
        setLoading(true);
        setError(null);
        const data = await HabitsService.getUserCompletionRates(user.id);
        setCompletionRates(data);
      } catch (err) {
        console.error('Error fetching completion rates:', err);
        setError('Failed to load completion rates. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    fetchCompletionRates();
  }, [user?.id]);

  if (loading) {
    return (
      <div className="completion-rates-page">
        <div className="loading-container">
          <motion.div
            animate={{ rotate: 360 }}
            transition={{ duration: 1, repeat: Infinity, ease: 'linear' }}
            className="loading-spinner"
          />
          <p>Loading completion rates...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="completion-rates-page">
        <div className="error-container">
          <h2>Error</h2>
          <p>{error}</p>
          <button onClick={() => window.location.reload()}>Try Again</button>
        </div>
      </div>
    );
  }

  if (!completionRates) {
    return (
      <div className="completion-rates-page">
        <div className="empty-container">
          <h2>No Data Available</h2>
          <p>Start creating habits to see your completion rates!</p>
        </div>
      </div>
    );
  }

  return (
    <div className="completion-rates-page">
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
        className="page-header"
      >
        <h1>Completion Rates</h1>
        <p>Track your habit completion performance over time</p>
      </motion.div>

      <div className="overview-section">
        <h2>Overview</h2>
        <div className="overview-grid">
          <CompletionRateCard
            title="Overall Rate"
            rate={completionRates.overallCompletionRate}
            subtitle="All time completion rate"
            color="#667eea"
            icon="📊"
          />
          <CompletionRateCard
            title="Weekly Rate"
            rate={completionRates.weeklyCompletionRate}
            subtitle="Last 7 days"
            color="#10b981"
            icon="📅"
          />
          <CompletionRateCard
            title="Monthly Rate"
            rate={completionRates.monthlyCompletionRate}
            subtitle="Last 30 days"
            color="#f59e0b"
            icon="📈"
          />
          <CompletionRateCard
            title="Today's Progress"
            rate={completionRates.activeHabits > 0 ? 
              (completionRates.completedHabitsToday / completionRates.activeHabits) * 100 : 0}
            subtitle={`${completionRates.completedHabitsToday}/${completionRates.activeHabits} habits`}
            color="#8b5cf6"
            icon="✅"
          />
        </div>
      </div>

      <div className="stats-section">
        <h2>Statistics</h2>
        <div className="stats-grid">
          <div className="stat-item">
            <div className="stat-value">{completionRates.totalHabits}</div>
            <div className="stat-label">Total Habits</div>
          </div>
          <div className="stat-item">
            <div className="stat-value">{completionRates.activeHabits}</div>
            <div className="stat-label">Active Habits</div>
          </div>
          <div className="stat-item">
            <div className="stat-value">{completionRates.completedHabitsToday}</div>
            <div className="stat-label">Completed Today</div>
          </div>
          <div className="stat-item">
            <div className="stat-value">
              {completionRates.activeHabits > 0 ? 
                Math.round((completionRates.completedHabitsToday / completionRates.activeHabits) * 100) : 0}%
            </div>
            <div className="stat-label">Today's Rate</div>
          </div>
        </div>
      </div>

      <div className="habits-section">
        <h2>Individual Habit Rates</h2>
        <div className="habits-grid">
          {completionRates.habitCompletionRates.map((habit, index) => (
            <motion.div
              key={habit.habitId}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.3, delay: index * 0.1 }}
              className="habit-rate-item"
            >
              <div className="habit-header">
                <h3>{habit.habitName}</h3>
                <span className={`status-badge ${habit.isActive ? 'active' : 'inactive'}`}>
                  {habit.isActive ? 'Active' : 'Inactive'}
                </span>
              </div>
              
              <div className="habit-rates">
                <div className="rate-item">
                  <span className="rate-label">Overall:</span>
                  <span className="rate-value">{habit.overallCompletionRate.toFixed(1)}%</span>
                </div>
                <div className="rate-item">
                  <span className="rate-label">Weekly:</span>
                  <span className="rate-value">{habit.weeklyCompletionRate.toFixed(1)}%</span>
                </div>
                <div className="rate-item">
                  <span className="rate-label">Monthly:</span>
                  <span className="rate-value">{habit.monthlyCompletionRate.toFixed(1)}%</span>
                </div>
              </div>
              
              <div className="habit-details">
                <div className="detail-item">
                  <span className="detail-label">Streak:</span>
                  <span className="detail-value">🔥 {habit.currentStreak}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label">Best:</span>
                  <span className="detail-value">🏆 {habit.longestStreak}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label">Completions:</span>
                  <span className="detail-value">{habit.totalActualCompletions}/{habit.totalPossibleCompletions}</span>
                </div>
              </div>
              
              {habit.lastCompletedAt && (
                <div className="last-completed">
                  Last completed: {new Date(habit.lastCompletedAt).toLocaleDateString()}
                </div>
              )}
            </motion.div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default CompletionRatesPage; 