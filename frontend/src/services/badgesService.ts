import { ApiService } from './api';

// Types
export interface Badge {
  id: string;
  name: string;
  description: string;
  icon: string;
  color: string;
  type: BadgeType;
  criteria: string;
  isUnlocked: boolean;
  unlockedAt?: string;
  progress?: number;
  maxProgress?: number;
}

export interface UserBadge {
  id: string;
  userId: string;
  badgeId: string;
  unlockedAt: string;
  badge: Badge;
}

export enum BadgeType {
  Streak = 'Streak',
  Completion = 'Completion',
  Social = 'Social',
  Special = 'Special'
}

// Badges Service
export class BadgesService {
  // Get all available badges
  static async getBadges(): Promise<Badge[]> {
    return ApiService.get<Badge[]>('/badges');
  }

  // Get a specific badge by ID
  static async getBadge(id: string): Promise<Badge> {
    return ApiService.get<Badge>(`/badges/${id}`);
  }

  // Get user's unlocked badges
  static async getUserBadges(): Promise<UserBadge[]> {
    return ApiService.get<UserBadge[]>('/user-badges');
  }

  // Get user's badge progress
  static async getBadgeProgress(): Promise<Badge[]> {
    return ApiService.get<Badge[]>('/badges/progress');
  }

  // Unlock a badge for the current user
  static async unlockBadge(badgeId: string): Promise<UserBadge> {
    return ApiService.post<UserBadge>('/user-badges', { badgeId });
  }

  // Get badge statistics for the current user
  static async getBadgeStats(): Promise<any> {
    return ApiService.get<any>('/badges/stats');
  }
}

export default BadgesService; 