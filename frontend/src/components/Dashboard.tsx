import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { useAuth } from '../contexts/AuthContext';
import { 
  AnimatedCard, 
  AnimatedButton, 
  AnimatedLink,
  AnimatedIcon,
  AnimatedCounter,
  AnimatedProgress,
  fadeInUp,
  staggerContainer,
  slideInFromLeft,
  slideInFromRight
} from './AnimatedComponents';
import './Dashboard.css';

interface Habit {
  id: string;
  name: string;
  description: string;
  frequency: string;
  currentStreak: number;
  longestStreak: number;
  nextCheckIn: string;
}

interface DashboardStats {
  totalHabits: number;
  activeStreaks: number;
  totalCheckIns: number;
  completionRate: number;
}

const Dashboard: React.FC = () => {
  const { user } = useAuth();
  const [habits, setHabits] = useState<Habit[]>([]);
  const [stats, setStats] = useState<DashboardStats>({
    totalHabits: 0,
    activeStreaks: 0,
    totalCheckIns: 0,
    completionRate: 0
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const fetchDashboardData = async () => {
    try {
      // TODO: Replace with actual API calls
      // const habitsResponse = await fetch('/api/habits');
      // const statsResponse = await fetch('/api/dashboard/stats');
      
      // Initialize with empty data
      setHabits([]);
      setStats({
        totalHabits: 0,
        activeStreaks: 0,
        totalCheckIns: 0,
        completionRate: 0
      });
    } catch (error) {
      console.error('Error fetching dashboard data:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <motion.div 
        className="dashboard-loading"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
      >
        <motion.div 
          className="spinner"
          animate={{ rotate: 360 }}
          transition={{ duration: 1, repeat: Infinity, ease: "linear" }}
        ></motion.div>
        <motion.p
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.5 }}
        >
          Loading your dashboard...
        </motion.p>
      </motion.div>
    );
  }

  return (
    <motion.div 
      className="dashboard"
      initial="hidden"
      animate="visible"
      variants={fadeInUp}
    >
      <motion.div 
        className="dashboard-header"
        variants={staggerContainer}
      >
        <motion.h1 variants={slideInFromLeft}>
          Welcome back, {user?.firstName || 'User'}! 👋
        </motion.h1>
        <motion.p 
          className="dashboard-subtitle"
          variants={slideInFromRight}
        >
          Let's continue building your habits
        </motion.p>
      </motion.div>

      {/* Stats Cards */}
      <motion.div 
        className="stats-grid"
        variants={staggerContainer}
        initial="hidden"
        animate="visible"
      >
        <AnimatedCard className="stat-card" delay={0.1}>
          <AnimatedIcon className="stat-icon">📊</AnimatedIcon>
          <div className="stat-content">
            <AnimatedCounter value={stats.totalHabits} className="stat-number" />
            <p>Total Habits</p>
          </div>
        </AnimatedCard>
        <AnimatedCard className="stat-card" delay={0.2}>
          <AnimatedIcon className="stat-icon">🔥</AnimatedIcon>
          <div className="stat-content">
            <AnimatedCounter value={stats.activeStreaks} className="stat-number" />
            <p>Active Streaks</p>
          </div>
        </AnimatedCard>
        <AnimatedCard className="stat-card" delay={0.3}>
          <AnimatedIcon className="stat-icon">✅</AnimatedIcon>
          <div className="stat-content">
            <AnimatedCounter value={stats.totalCheckIns} className="stat-number" />
            <p>Total Check-ins</p>
          </div>
        </AnimatedCard>
        <AnimatedCard className="stat-card" delay={0.4}>
          <AnimatedIcon className="stat-icon">📈</AnimatedIcon>
          <div className="stat-content">
            <AnimatedCounter value={stats.completionRate} className="stat-number" />
            <span>%</span>
            <p>Completion Rate</p>
          </div>
        </AnimatedCard>
      </motion.div>

      {/* Quick Actions */}
      <motion.div 
        className="quick-actions"
        initial="hidden"
        whileInView="visible"
        viewport={{ once: true }}
        variants={fadeInUp}
      >
        <h2>Quick Actions</h2>
        <motion.div 
          className="action-buttons"
          variants={staggerContainer}
        >
          <motion.div variants={fadeInUp}>
            <AnimatedLink to="/check-in" className="action-btn primary">
              <AnimatedIcon>📝</AnimatedIcon>
              Daily Check-in
            </AnimatedLink>
          </motion.div>
          <motion.div variants={fadeInUp}>
            <AnimatedLink to="/habits" className="action-btn secondary">
              <AnimatedIcon>➕</AnimatedIcon>
              Add New Habit
            </AnimatedLink>
          </motion.div>
        </motion.div>
      </motion.div>

      {/* Current Habits */}
      <motion.div 
        className="habits-section"
        initial="hidden"
        whileInView="visible"
        viewport={{ once: true }}
        variants={fadeInUp}
      >
        <div className="section-header">
          <h2>Your Habits</h2>
          <Link to="/habits" className="view-all-link">View All</Link>
        </div>
        
        {habits.length === 0 ? (
          <motion.div 
            className="empty-state"
            initial={{ opacity: 0, scale: 0.8 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ duration: 0.5 }}
          >
            <AnimatedIcon className="empty-icon">🎯</AnimatedIcon>
            <h3>No habits yet</h3>
            <p>Start building your habit chain by creating your first habit!</p>
            <AnimatedLink to="/habits" className="btn-primary">
              Create First Habit
            </AnimatedLink>
          </motion.div>
        ) : (
          <motion.div 
            className="habits-grid"
            variants={staggerContainer}
          >
            {habits.map((habit, index) => (
              <AnimatedCard 
                key={habit.id} 
                className="habit-card"
                delay={index * 0.1}
              >
                <div className="habit-header">
                  <h3>{habit.name}</h3>
                  <span className="habit-frequency">{habit.frequency}</span>
                </div>
                <p className="habit-description">{habit.description}</p>
                <div className="habit-stats">
                  <div className="streak-info">
                    <motion.span 
                      className="current-streak"
                      whileHover={{ scale: 1.05 }}
                    >
                      🔥 {habit.currentStreak} days
                    </motion.span>
                    <motion.span 
                      className="longest-streak"
                      whileHover={{ scale: 1.05 }}
                    >
                      🏆 {habit.longestStreak} days
                    </motion.span>
                  </div>
                  <div className="next-checkin">
                    Next: {new Date(habit.nextCheckIn).toLocaleDateString()}
                  </div>
                </div>
                <div className="habit-actions">
                  <AnimatedLink to={`/check-in?habit=${habit.id}`} className="btn-checkin">
                    Check-in
                  </AnimatedLink>
                  <AnimatedLink to={`/habits/${habit.id}`} className="btn-edit">
                    Edit
                  </AnimatedLink>
                </div>
              </AnimatedCard>
            ))}
          </motion.div>
        )}
      </motion.div>

      {/* Recent Activity */}
      <motion.div 
        className="recent-activity"
        initial="hidden"
        whileInView="visible"
        viewport={{ once: true }}
        variants={fadeInUp}
      >
        <h2>Recent Activity</h2>
        <motion.div 
          className="activity-list"
          variants={staggerContainer}
        >
          <motion.div 
            className="activity-item"
            variants={fadeInUp}
            whileHover={{ x: 5 }}
          >
            <AnimatedIcon className="activity-icon">🎯</AnimatedIcon>
            <div className="activity-content">
              <p>Welcome to HabitChain! Start building your first habit.</p>
              <span className="activity-time">Just now</span>
            </div>
          </motion.div>
        </motion.div>
      </motion.div>
    </motion.div>
  );
};

export default Dashboard; 