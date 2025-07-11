import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { badgeService, Badge } from '../services/badgeService';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  AnimatedCard, 
  AnimatedIcon,
  fadeInUp,
  staggerContainer,
  slideInFromLeft,
  slideInFromRight
} from './AnimatedComponents';
import './Badges.css';

const Badges: React.FC = () => {
  const { user } = useAuth();
  const [badges, setBadges] = useState<Badge[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedCategory, setSelectedCategory] = useState<string>('all');
  const [recentlyEarned, setRecentlyEarned] = useState<Badge[]>([]);

  useEffect(() => {
    if (user?.id) {
      fetchBadges();
    }
  }, [user?.id]);

  const fetchBadges = async () => {
    try {
      setLoading(true);
      const userBadges = await badgeService.getUserBadgesWithProgress(user!.id);
      const sortedBadges = badgeService.sortBadgesByDisplayOrder(userBadges);
      setBadges(sortedBadges);
      
      // Get recently earned badges for celebration
      const recent = badgeService.getRecentEarnedBadges(sortedBadges, 3);
      setRecentlyEarned(recent);
    } catch (error) {
      console.error('Error fetching badges:', error);
    } finally {
      setLoading(false);
    }
  };

  const categories = [
    { id: 'all', label: 'All Badges', icon: '🏆' },
    { id: 'milestone', label: 'Milestones', icon: '🎯' },
    { id: 'streak', label: 'Streaks', icon: '🔥' },
    { id: 'time', label: 'Time-based', icon: '⏰' },
    { id: 'challenge', label: 'Challenges', icon: '🎯' },
    { id: 'seasonal', label: 'Seasonal', icon: '🌸' },
    { id: 'rarity', label: 'Rarity', icon: '💎' },
    { id: 'chain', label: 'Chains', icon: '⛓️' },
    { id: 'consistency', label: 'Consistency', icon: '📈' }
  ];

  const filteredBadges = badgeService.filterBadgesByCategory(badges, selectedCategory);
  const earnedBadges = badgeService.getEarnedBadgesCount(badges);
  const totalBadges = badges.length;
  const completionRate = badgeService.getCompletionRate(badges);
  const remainingBadges = badgeService.getRemainingBadgesCount(badges);

  if (loading) {
    return (
      <motion.div 
        className="badges-loading"
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
          Loading your badges...
        </motion.p>
      </motion.div>
    );
  }

  return (
    <motion.div 
      className="badges"
      initial="hidden"
      animate="visible"
      variants={fadeInUp}
    >
      <motion.div 
        className="badges-header"
        variants={staggerContainer}
      >
        <motion.h1 variants={slideInFromLeft}>
          Badges & Achievements
        </motion.h1>
        <motion.p 
          className="badges-subtitle"
          variants={slideInFromRight}
        >
          Track your progress and celebrate your victories
        </motion.p>
      </motion.div>

      {/* Stats Overview */}
      <motion.div 
        className="badges-stats"
        variants={staggerContainer}
        initial="hidden"
        animate="visible"
      >
        <AnimatedCard className="stat-card" delay={0.1}>
          <AnimatedIcon className="stat-icon">🏆</AnimatedIcon>
          <div className="stat-content">
            <h3>{earnedBadges}</h3>
            <p>Badges Earned</p>
          </div>
        </AnimatedCard>
        <AnimatedCard className="stat-card" delay={0.2}>
          <AnimatedIcon className="stat-icon">📊</AnimatedIcon>
          <div className="stat-content">
            <h3>{completionRate}%</h3>
            <p>Completion Rate</p>
          </div>
        </AnimatedCard>
        <AnimatedCard className="stat-card" delay={0.3}>
          <AnimatedIcon className="stat-icon">🎯</AnimatedIcon>
          <div className="stat-content">
            <h3>{remainingBadges}</h3>
            <p>Remaining</p>
          </div>
        </AnimatedCard>
      </motion.div>

      {/* Category Filter */}
      <motion.div 
        className="category-filter"
        initial="hidden"
        whileInView="visible"
        viewport={{ once: true }}
        variants={fadeInUp}
      >
        <h2>Filter by Category</h2>
        <motion.div 
          className="category-buttons"
          variants={staggerContainer}
        >
          {categories.map((category, index) => (
            <motion.button
              key={category.id}
              className={`category-btn ${selectedCategory === category.id ? 'active' : ''}`}
              onClick={() => setSelectedCategory(category.id)}
              variants={fadeInUp}
              whileHover={{ scale: 1.05 }}
              whileTap={{ scale: 0.95 }}
            >
              <span className="category-icon">{category.icon}</span>
              {category.label}
            </motion.button>
          ))}
        </motion.div>
      </motion.div>

      {/* Badges Grid */}
      <motion.div 
        className="badges-container"
        initial="hidden"
        whileInView="visible"
        viewport={{ once: true }}
        variants={fadeInUp}
      >
        {filteredBadges.length === 0 ? (
          <motion.div 
            className="empty-state"
            initial={{ opacity: 0, scale: 0.8 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ duration: 0.5 }}
          >
            <AnimatedIcon className="empty-icon">🏆</AnimatedIcon>
            <h3>No badges in this category</h3>
            <p>Try selecting a different category or keep working towards your goals!</p>
          </motion.div>
        ) : (
          <motion.div 
            className="badges-grid"
            variants={staggerContainer}
          >
            <AnimatePresence>
              {filteredBadges.map((badge, index) => (
                <motion.div
                  key={badge.id}
                  className={`badge-card ${badge.isEarned ? 'earned' : 'locked'}`}
                  variants={fadeInUp}
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: -20 }}
                  transition={{ delay: index * 0.1 }}
                  whileHover={{ 
                    scale: 1.02,
                    boxShadow: "0 8px 25px rgba(0, 0, 0, 0.15)"
                  }}
                >
                  <div className="badge-header">
                    <div 
                      className="badge-icon"
                      style={{ backgroundColor: badge.colorTheme }}
                    >
                      {badge.emoji}
                    </div>
                    <div className="badge-rarity">
                      <span 
                        className="rarity-badge"
                        style={{ backgroundColor: badgeService.getRarityColor(badge.rarity) }}
                      >
                        {badgeService.getRarityLabel(badge.rarity)}
                      </span>
                    </div>
                  </div>
                  
                  <div className="badge-content">
                    <h3>{badge.name}</h3>
                    <p>{badge.description}</p>
                    
                    <div className="badge-category">
                      <span className="category-badge">
                        {badgeService.getCategoryIcon(badge.category)} {badgeService.getCategoryLabel(badge.category)}
                      </span>
                    </div>
                    
                    {badge.isEarned ? (
                      <div className="badge-earned">
                        <span className="earned-badge">✅ Earned</span>
                        {badge.earnedAt && (
                          <span className="earned-date">
                            {new Date(badge.earnedAt).toLocaleDateString()}
                          </span>
                        )}
                      </div>
                    ) : (
                      <div className="badge-progress">
                        {badge.progress !== undefined && badge.target !== undefined ? (
                          <>
                            <div className="progress-bar">
                              <div 
                                className="progress-fill" 
                                style={{ 
                                  width: `${badgeService.getProgressPercentage(badge)}%`,
                                  backgroundColor: badge.colorTheme
                                }}
                              ></div>
                            </div>
                            <span className="progress-text">
                              {badge.progress} / {badge.target} ({Math.round(badgeService.getProgressPercentage(badge))}%)
                            </span>
                          </>
                        ) : (
                          <span className="progress-text">Not started</span>
                        )}
                      </div>
                    )}
                  </div>
                </motion.div>
              ))}
            </AnimatePresence>
          </motion.div>
        )}
      </motion.div>

      {/* Recently Earned Achievements */}
      {recentlyEarned.length > 0 && (
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
            {recentlyEarned.map((badge, index) => (
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

      {/* Badge Earning Celebration */}
      {recentlyEarned.length > 0 && (
        <motion.div 
          className="celebration-overlay"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
        >
          <motion.div 
            className="celebration-content"
            initial={{ scale: 0, rotate: -180 }}
            animate={{ scale: 1, rotate: 0 }}
            transition={{ type: "spring", stiffness: 200 }}
          >
            <div className="celebration-icon">🎉</div>
            <h3>Congratulations!</h3>
            <p>You've earned {recentlyEarned.length} new badge{recentlyEarned.length > 1 ? 's' : ''}!</p>
          </motion.div>
        </motion.div>
      )}
    </motion.div>
  );
};

export default Badges; 