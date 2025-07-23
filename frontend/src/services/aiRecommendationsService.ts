import { ApiService } from './api';

// Types
export interface HabitRecommendation {
  name: string;
  description: string;
  reasoning: string;
  frequency: string;
  targetDays: number;
  category: string;
  confidence: number;
  relatedHabits: string[];
  suggestedTime: string;
  difficulty: string;
}

export interface HabitSummary {
  id: string;
  name: string;
  description?: string;
  frequency: string;
  currentStreak: number;
  totalCheckIns: number;
  isActive: boolean;
}

export interface CheckInSummary {
  id: string;
  habitId: string;
  habitName: string;
  completedAt: string;
  streakDay: number;
}

export interface HabitPatterns {
  strongCategories: string[];
  weakCategories: string[];
  preferredTime: string;
  preferredFrequency: string;
  averageCompletionRate: number;
  totalActiveHabits: number;
  totalCompletedHabits: number;
}

export interface UserHabitAnalysis {
  userId: string;
  userName: string;
  currentHabits: HabitSummary[];
  recentCheckIns: CheckInSummary[];
  patterns: HabitPatterns;
  recommendations: HabitRecommendation[];
}

// API Response types
interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  error?: string;
}

// AI Recommendations Service
export class AiRecommendationsService {
  // Get AI-generated habit recommendations
  static async getHabitRecommendations(): Promise<HabitRecommendation[]> {
    try {
      const response = await ApiService.get<ApiResponse<HabitRecommendation[]>>('/ai-recommendations/habits');
      return response.data;
    } catch (error) {
      console.error('Error fetching habit recommendations:', error);
      throw error;
    }
  }

  // Create a habit from an AI recommendation
  static async createHabitFromRecommendation(recommendation: HabitRecommendation): Promise<any> {
    try {
      const response = await ApiService.post<ApiResponse<any>>('/ai-recommendations/create-habit', recommendation);
      return response.data;
    } catch (error) {
      console.error('Error creating habit from recommendation:', error);
      throw error;
    }
  }

  // Get detailed user habit analysis
  static async getUserAnalysis(): Promise<UserHabitAnalysis> {
    try {
      const response = await ApiService.get<ApiResponse<UserHabitAnalysis>>('/ai-recommendations/analysis');
      return response.data;
    } catch (error) {
      console.error('Error fetching user analysis:', error);
      throw error;
    }
  }

  // Get personalized motivation message
  static async getPersonalizedMotivation(): Promise<string> {
    try {
      const response = await ApiService.get<ApiResponse<string>>('/ai-recommendations/motivation');
      return response.data;
    } catch (error) {
      console.error('Error fetching motivation:', error);
      throw error;
    }
  }

  // Get complementary habits for a specific habit
  static async getComplementaryHabits(habitId: string): Promise<HabitRecommendation[]> {
    try {
      const response = await ApiService.get<ApiResponse<HabitRecommendation[]>>(`/ai-recommendations/complementary/${habitId}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching complementary habits:', error);
      throw error;
    }
  }
}

export default AiRecommendationsService; 