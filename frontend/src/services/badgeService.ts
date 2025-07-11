import api from './api';

export interface Badge {
  id: string;
  name: string;
  description: string;
  iconUrl: string;
  emoji: string;
  type: string;
  category: string;
  rarity: 'common' | 'rare' | 'epic' | 'legendary';
  requiredValue: number;
  isActive: boolean;
  colorTheme: string;
  isSecret: boolean;
  displayOrder: number;
  createdAt: string;
  updatedAt: string;
  isEarned?: boolean;
  progress?: number;
  target?: number;
  earnedAt?: string;
  habitId?: string;
}

export interface UserBadge {
  id: string;
  userId: string;
  badgeId: string;
  habitId?: string;
  earnedAt: string;
  badge: Badge;
  habit?: any;
}

class BadgeService {
  async getAllBadges(): Promise<Badge[]> {
    try {
      const response = await api.get('/Badges');
      return response.data;
    } catch (error) {
      console.error('Error fetching all badges:', error);
      throw error;
    }
  }

  async getActiveBadges(): Promise<Badge[]> {
    try {
      const response = await api.get('/Badges/active');
      return response.data;
    } catch (error) {
      console.error('Error fetching active badges:', error);
      throw error;
    }
  }

  async getBadgeById(id: string): Promise<Badge> {
    try {
      const response = await api.get(`/Badges/${id}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching badge by ID:', error);
      throw error;
    }
  }

  async getUserBadgesWithProgress(userId: string): Promise<Badge[]> {
    try {
      const response = await api.get(`/Badges/user/${userId}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching user badges with progress:', error);
      throw error;
    }
  }

  async getUserEarnedBadges(userId: string): Promise<UserBadge[]> {
    try {
      const response = await api.get(`/Badges/user/${userId}/earned`);
      return response.data;
    } catch (error) {
      console.error('Error fetching user earned badges:', error);
      throw error;
    }
  }

  async checkAndAwardBadges(userId: string, habitId?: string): Promise<Badge[]> {
    try {
      const response = await api.post('/Badges/check-earning', {
        userId,
        habitId
      });
      return response.data;
    } catch (error) {
      console.error('Error checking and awarding badges:', error);
      throw error;
    }
  }

  getRarityColor(rarity: string): string {
    switch (rarity) {
      case 'common': return '#6c757d';
      case 'rare': return '#007bff';
      case 'epic': return '#6f42c1';
      case 'legendary': return '#fd7e14';
      default: return '#6c757d';
    }
  }

  getRarityLabel(rarity: string): string {
    switch (rarity) {
      case 'common': return 'Common';
      case 'rare': return 'Rare';
      case 'epic': return 'Epic';
      case 'legendary': return 'Legendary';
      default: return 'Common';
    }
  }

  getCategoryIcon(category: string): string {
    switch (category) {
      case 'milestone': return '🎯';
      case 'streak': return '🔥';
      case 'social': return '👥';
      case 'creation': return '✨';
      case 'time': return '⏰';
      case 'challenge': return '🎯';
      case 'seasonal': return '🌸';
      case 'rarity': return '💎';
      case 'chain': return '⛓️';
      case 'consistency': return '📈';
      default: return '🏆';
    }
  }

  getCategoryLabel(category: string): string {
    switch (category) {
      case 'milestone': return 'Milestones';
      case 'streak': return 'Streaks';
      case 'social': return 'Social';
      case 'creation': return 'Creation';
      case 'time': return 'Time-based';
      case 'challenge': return 'Challenges';
      case 'seasonal': return 'Seasonal';
      case 'rarity': return 'Rarity';
      case 'chain': return 'Chains';
      case 'consistency': return 'Consistency';
      default: return 'Other';
    }
  }

  getProgressPercentage(badge: Badge): number {
    if (!badge.progress || !badge.target) return 0;
    return Math.min((badge.progress / badge.target) * 100, 100);
  }

  getEarnedBadgesCount(badges: Badge[]): number {
    return badges.filter(badge => badge.isEarned).length;
  }

  getCompletionRate(badges: Badge[]): number {
    if (badges.length === 0) return 0;
    const earnedCount = this.getEarnedBadgesCount(badges);
    return Math.round((earnedCount / badges.length) * 100);
  }

  getRemainingBadgesCount(badges: Badge[]): number {
    return badges.length - this.getEarnedBadgesCount(badges);
  }

  filterBadgesByCategory(badges: Badge[], category: string): Badge[] {
    if (category === 'all') return badges;
    return badges.filter(badge => badge.category === category);
  }

  sortBadgesByDisplayOrder(badges: Badge[]): Badge[] {
    return badges.sort((a, b) => a.displayOrder - b.displayOrder);
  }

  getRecentEarnedBadges(badges: Badge[], limit: number = 3): Badge[] {
    return badges
      .filter(badge => badge.isEarned && badge.earnedAt)
      .sort((a, b) => new Date(b.earnedAt!).getTime() - new Date(a.earnedAt!).getTime())
      .slice(0, limit);
  }
}

export const badgeService = new BadgeService(); 