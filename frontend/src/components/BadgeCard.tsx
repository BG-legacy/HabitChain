import React from 'react';
import './BadgeCard.css';

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

interface BadgeCardProps {
  badge: Badge;
  onClick?: (badge: Badge) => void;
  showProgress?: boolean;
  showEarnedDate?: boolean;
  className?: string;
  size?: 'small' | 'medium' | 'large';
}

const BadgeCard: React.FC<BadgeCardProps> = ({
  badge,
  onClick,
  showProgress = true,
  showEarnedDate = true,
  className = '',
  size = 'medium'
}) => {
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

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const getProgressPercentage = () => {
    if (!badge.progress || !badge.target) return 0;
    return Math.min((badge.progress / badge.target) * 100, 100);
  };

  const handleClick = () => {
    if (onClick) {
      onClick(badge);
    }
  };

  return (
    <div 
      className={`badge-card ${badge.isEarned ? 'earned' : 'locked'} ${size} ${className} ${onClick ? 'clickable' : ''}`}
      onClick={handleClick}
    >
      <div className="badge-header">
        <div className="badge-icon" style={{ backgroundColor: getRarityColor(badge.rarity) }}>
          {badge.icon}
        </div>
        <div className="badge-rarity">
          <span 
            className="rarity-badge"
            style={{ backgroundColor: getRarityColor(badge.rarity) }}
          >
            {getRarityLabel(badge.rarity)}
          </span>
        </div>
      </div>

      <div className="badge-content">
        <h3 className="badge-name">{badge.name}</h3>
        <p className="badge-description">{badge.description}</p>
        
        <div className="badge-category">
          <span className="category-badge">
            {getCategoryIcon(badge.category)} {getCategoryLabel(badge.category)}
          </span>
        </div>

        {badge.isEarned && showEarnedDate && badge.earnedDate && (
          <div className="badge-earned">
            <span className="earned-badge">🏆 Earned</span>
            <span className="earned-date">{formatDate(badge.earnedDate)}</span>
          </div>
        )}

        {!badge.isEarned && showProgress && badge.progress !== undefined && badge.target && (
          <div className="badge-progress">
            <div className="progress-bar">
              <div 
                className="progress-fill"
                style={{ 
                  width: `${getProgressPercentage()}%`,
                  backgroundColor: getRarityColor(badge.rarity)
                }}
              ></div>
            </div>
            <div className="progress-text">
              {badge.progress} / {badge.target} ({Math.round(getProgressPercentage())}%)
            </div>
          </div>
        )}

        {!badge.isEarned && (!showProgress || badge.progress === undefined) && (
          <div className="badge-locked">
            <span className="locked-text">🔒 Locked</span>
          </div>
        )}
      </div>

      {onClick && (
        <div className="badge-overlay">
          <span className="overlay-text">
            {badge.isEarned ? 'View Details' : 'View Progress'}
          </span>
        </div>
      )}
    </div>
  );
};

export default BadgeCard; 