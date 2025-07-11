import React from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { useAuth } from '../contexts/AuthContext';
import { useDashboard } from '../contexts/DashboardContext';
import { badgeService, Badge } from '../services/badgeService';
import { 
  AnimatedCard, 
  AnimatedLink,
  AnimatedIcon,
  AnimatedCounter,
  fadeInUp,
  staggerContainer,
  slideInFromLeft,
  slideInFromRight
} from './AnimatedComponents';
import CompleteHabitButton from './CompleteHabitButton';
import CheckInButton from './CheckInButton';
import './Dashboard.css';
import './BadgeProgress.css';
import HabitCard from './HabitCard';

const Dashboard: React.FC = () => {
  const { user } = useAuth();
  const { habits, stats, loading, refreshDashboard } = useDashboard();
  const [badges, setBadges] = React.useState<Badge[]>([]);
  const [recentAchievements, setRecentAchievements] = React.useState<Badge[]>([]);

  React.useEffect(() => {
    if (user?.id) {
      fetchBadges();
    }
  }, [user?.id]);

  const fetchBadges = async () => {
    try {
      const userBadges = await badgeService.getUserBadgesWithProgress(user!.id);
      const sortedBadges = badgeService.sortBadgesByDisplayOrder(userBadges);
      setBadges(sortedBadges);
      
      // Get recent achievements
      const recent = badgeService.getRecentEarnedBadges(sortedBadges, 3);
      setRecentAchievements(recent);
    } catch (error) {
      console.error('Error fetching badges:', error);
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

  const earnedBadges = badgeService.getEarnedBadgesCount(badges);
  const totalBadges = badges.length;
  const badgeCompletionRate = badgeService.getCompletionRate(badges);

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

      </motion.div>

      {/* Badge Progress */}
      <motion.div 
        className="badge-progress-section"
        initial="hidden"
        whileInView="visible"
        viewport={{ once: true }}
        variants={fadeInUp}
      >
        <div className="section-header">
          <h2>🏆 Badge Progress</h2>
          <Link to="/badges" className="view-all-link">View All Badges</Link>
        </div>
        
        <motion.div 
          className="badge-stats"
          variants={staggerContainer}
        >
          <AnimatedCard className="badge-stat-card" delay={0.1}>
            <div className="badge-stat-icon">🏆</div>
            <div className="badge-stat-content">
              <h3>{earnedBadges}</h3>
              <p>Badges Earned</p>
            </div>
          </AnimatedCard>
          <AnimatedCard className="badge-stat-card" delay={0.2}>
            <div className="badge-stat-icon">📊</div>
            <div className="badge-stat-content">
              <h3>{badgeCompletionRate}%</h3>
              <p>Badge Completion</p>
            </div>
          </AnimatedCard>
          <AnimatedCard className="badge-stat-card" delay={0.3}>
            <div className="badge-stat-icon">🎯</div>
            <div className="badge-stat-content">
              <h3>{totalBadges - earnedBadges}</h3>
              <p>Remaining</p>
            </div>
          </AnimatedCard>
        </motion.div>
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
          <motion.div variants={fadeInUp}>
            <AnimatedLink to="/completion-rates" className="action-btn tertiary">
              <AnimatedIcon>📊</AnimatedIcon>
              View Completion Rates
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
                <HabitCard
                  habit={{
                    ...(habit as any),
                    targetDays: (habit as any).targetDays ?? 1,
                    createdAt: (habit as any).createdAt || new Date().toISOString(),
                  }}
                  showActions={false}
                />
              </AnimatedCard>
            ))}
          </motion.div>
        )}
      </motion.div>

      {/* Recent Achievements */}
      {recentAchievements.length > 0 && (
        <motion.div 
          className="recent-achievements"
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true }}
          variants={fadeInUp}
        >
          <h2>Recent Achievements</h2>
          <motion.div 
            className="achievements-list"
            variants={staggerContainer}
          >
            {recentAchievements.map((badge, index) => (
              <motion.div 
                key={badge.id} 
                className="achievement-item"
                variants={fadeInUp}
                whileHover={{ x: 5 }}
              >
                <div 
                  className="achievement-icon"
                  style={{ backgroundColor: badge.colorTheme }}
                >
                  {badge.emoji}
                </div>
                <div className="achievement-content">
                  <h4>{badge.name}</h4>
                  <p>{badge.description}</p>
                  <span className="achievement-date">
                    {badge.earnedAt ? new Date(badge.earnedAt).toLocaleDateString() : 'Recently'}
                  </span>
                </div>
              </motion.div>
            ))}
          </motion.div>
        </motion.div>
      )}


    </motion.div>
  );
};

export default Dashboard; 