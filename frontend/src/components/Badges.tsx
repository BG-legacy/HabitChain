import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import './Badges.css';

interface Badge {
  id: string;
  name: string;
  description: string;
  icon: string;
  category: string;
  isEarned: boolean;
  earnedDate?: string;
  progress?: number;
  target?: number;
  rarity: 'common' | 'rare' | 'epic' | 'legendary';
}

const Badges: React.FC = () => {
  const { user } = useAuth();
  const [badges, setBadges] = useState<Badge[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedCategory, setSelectedCategory] = useState<string>('all');

  useEffect(() => {
    fetchBadges();
  }, []);

  const fetchBadges = async () => {
    try {
      // TODO: Replace with actual API call
      // const response = await fetch('/api/badges');
      // const data = await response.json();
      
      // Initialize with empty data
      setBadges([]);
    } catch (error) {
      console.error('Error fetching badges:', error);
    } finally {
      setLoading(false);
    }
  };

  const getRarityColor = (rarity: string) => {
    switch (rarity) {
      case 'common': return '#6c757d';
      case 'rare': return '#007bff';
      case 'epic': return '#6f42c1';
      case 'legendary': return '#fd7e14';
      default: return '#6c757d';
    }
  };

  const getRarityLabel = (rarity: string) => {
    switch (rarity) {
      case 'common': return 'Common';
      case 'rare': return 'Rare';
      case 'epic': return 'Epic';
      case 'legendary': return 'Legendary';
      default: return 'Common';
    }
  };

  const getCategoryIcon = (category: string) => {
    switch (category) {
      case 'milestone': return '🎯';
      case 'streak': return '🔥';
      case 'social': return '👥';
      case 'creation': return '✨';
      case 'time': return '⏰';
      default: return '🏆';
    }
  };

  const getCategoryLabel = (category: string) => {
    switch (category) {
      case 'milestone': return 'Milestones';
      case 'streak': return 'Streaks';
      case 'social': return 'Social';
      case 'creation': return 'Creation';
      case 'time': return 'Time-based';
      default: return 'Other';
    }
  };

  const categories = [
    { id: 'all', label: 'All Badges', icon: '🏆' },
    { id: 'milestone', label: 'Milestones', icon: '🎯' },
    { id: 'streak', label: 'Streaks', icon: '🔥' },
    { id: 'social', label: 'Social', icon: '👥' },
    { id: 'creation', label: 'Creation', icon: '✨' },
    { id: 'time', label: 'Time-based', icon: '⏰' }
  ];

  const filteredBadges = selectedCategory === 'all' 
    ? badges 
    : badges.filter(badge => badge.category === selectedCategory);

  const earnedBadges = badges.filter(badge => badge.isEarned);
  const totalBadges = badges.length;
  const completionRate = Math.round((earnedBadges.length / totalBadges) * 100);

  if (loading) {
    return (
      <div className="badges-loading">
        <div className="spinner"></div>
        <p>Loading your badges...</p>
      </div>
    );
  }

  return (
    <div className="badges">
      <div className="badges-header">
        <h1>Badges & Achievements</h1>
        <p className="badges-subtitle">Track your progress and celebrate your victories</p>
      </div>

      {/* Stats Overview */}
      <div className="badges-stats">
        <div className="stat-card">
          <div className="stat-icon">🏆</div>
          <div className="stat-content">
            <h3>{earnedBadges.length}</h3>
            <p>Badges Earned</p>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">📊</div>
          <div className="stat-content">
            <h3>{completionRate}%</h3>
            <p>Completion Rate</p>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">🎯</div>
          <div className="stat-content">
            <h3>{totalBadges - earnedBadges.length}</h3>
            <p>Remaining</p>
          </div>
        </div>
      </div>

      {/* Category Filter */}
      <div className="category-filter">
        <h2>Filter by Category</h2>
        <div className="category-buttons">
          {categories.map(category => (
            <button
              key={category.id}
              className={`category-btn ${selectedCategory === category.id ? 'active' : ''}`}
              onClick={() => setSelectedCategory(category.id)}
            >
              <span className="category-icon">{category.icon}</span>
              {category.label}
            </button>
          ))}
        </div>
      </div>

      {/* Badges Grid */}
      <div className="badges-container">
        {filteredBadges.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">🏆</div>
            <h3>No badges in this category</h3>
            <p>Try selecting a different category or keep working towards your goals!</p>
          </div>
        ) : (
          <div className="badges-grid">
            {filteredBadges.map(badge => (
              <div key={badge.id} className={`badge-card ${badge.isEarned ? 'earned' : 'locked'}`}>
                <div className="badge-header">
                  <div className="badge-icon">{badge.icon}</div>
                  <div className="badge-rarity" style={{ color: getRarityColor(badge.rarity) }}>
                    {getRarityLabel(badge.rarity)}
                  </div>
                </div>
                
                <div className="badge-content">
                  <h3>{badge.name}</h3>
                  <p>{badge.description}</p>
                  
                  {badge.isEarned ? (
                    <div className="badge-earned">
                      <span className="earned-date">
                        Earned {badge.earnedDate ? new Date(badge.earnedDate).toLocaleDateString() : 'recently'}
                      </span>
                      <div className="earned-badge">✅ Earned</div>
                    </div>
                  ) : (
                    <div className="badge-progress">
                      {badge.progress !== undefined && badge.target !== undefined ? (
                        <>
                          <div className="progress-bar">
                            <div 
                              className="progress-fill" 
                              style={{ width: `${(badge.progress / badge.target) * 100}%` }}
                            ></div>
                          </div>
                          <span className="progress-text">
                            {badge.progress} / {badge.target}
                          </span>
                        </>
                      ) : (
                        <span className="progress-text">Not started</span>
                      )}
                    </div>
                  )}
                </div>
                
                <div className="badge-category">
                  <span className="category-badge">
                    {getCategoryIcon(badge.category)} {getCategoryLabel(badge.category)}
                  </span>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Recent Achievements */}
      {earnedBadges.length > 0 && (
        <div className="recent-achievements">
          <h2>Recent Achievements</h2>
          <div className="achievements-list">
            {earnedBadges
              .sort((a, b) => new Date(b.earnedDate || '').getTime() - new Date(a.earnedDate || '').getTime())
              .slice(0, 3)
              .map(badge => (
                <div key={badge.id} className="achievement-item">
                  <div className="achievement-icon">{badge.icon}</div>
                  <div className="achievement-content">
                    <h4>{badge.name}</h4>
                    <p>{badge.description}</p>
                    <span className="achievement-date">
                      {badge.earnedDate ? new Date(badge.earnedDate).toLocaleDateString() : 'Recently'}
                    </span>
                  </div>
                </div>
              ))}
          </div>
        </div>
      )}
    </div>
  );
};

export default Badges; 